# Desenha uma prévia em PNG de um .docx gerado pelo programa (sem precisar do
# Word): lê o document.xml e aplica página, margens, fonte, alinhamento,
# espaçamento, negrito, quebra de página e o número de página do rodapé.
# É uma aproximação para conferência visual, não a renderização do Word.
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/word-export/Render-DocxPreview.ps1 -Docx arquivo.docx -Png prévia.png
param([Parameter(Mandatory = $true)][string]$Docx, [Parameter(Mandatory = $true)][string]$Png, [int]$Dpi = 110)
$ErrorActionPreference = 'Stop'
$code = @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Xml;

public static class DocxPreview
{
    private const string W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private sealed class Word { public string Text; public bool Bold; public float Width; }
    private sealed class Para
    {
        public string Align = "left"; public int Line = 240, Before, After; public bool KeepNext;
        public List<Word> Words = new List<Word>();
        public List<List<Word>> Lines = new List<List<Word>>();
        public float LineHeight, Height;
    }
    private sealed class Placed { public Para P; public int Page; public float Top; }

    private static int Int(XmlElement e, string attr, int fallback)
    {
        if (e == null) return fallback;
        string v = e.GetAttribute(attr, W);
        int n; return Int32.TryParse(v, out n) ? n : fallback;
    }

    public static void Render(string docx, string png, int dpi)
    {
        var doc = new XmlDocument();
        using (ZipArchive zip = ZipFile.OpenRead(docx))
        using (Stream s = zip.GetEntry("word/document.xml").Open()) doc.Load(s);
        var ns = new XmlNamespaceManager(doc.NameTable); ns.AddNamespace("w", W);
        var sect = (XmlElement)doc.SelectSingleNode("//w:body/w:sectPr", ns);
        var size = (XmlElement)sect.SelectSingleNode("w:pgSz", ns); var mar = (XmlElement)sect.SelectSingleNode("w:pgMar", ns);
        Func<int, float> px = delegate (int tw) { return tw / 1440f * dpi; };
        float pageW = px(Int(size, "w", 11906)), pageH = px(Int(size, "h", 16838));
        float left = px(Int(mar, "left", 1440)), right = px(Int(mar, "right", 1440)), top = px(Int(mar, "top", 1440)), bottom = px(Int(mar, "bottom", 1440)), footerDist = px(Int(mar, "footer", 708));
        float contentW = pageW - left - right;

        using (var probe = new Bitmap(10, 10))
        using (Graphics g = Graphics.FromImage(probe))
        using (var regular = new Font("Times New Roman", 12f, FontStyle.Regular, GraphicsUnit.Point))
        using (var bold = new Font("Times New Roman", 12f, FontStyle.Bold, GraphicsUnit.Point))
        using (var small = new Font("Times New Roman", 10f, FontStyle.Regular, GraphicsUnit.Point))
        {
            probe.SetResolution(dpi, dpi);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            var fmt = (StringFormat)StringFormat.GenericTypographic.Clone();
            fmt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            float single = regular.GetHeight(dpi);
            float space = g.MeasureString(" ", regular, PointF.Empty, fmt).Width;

            // Paragraphs and word wrap.
            var paras = new List<Para>();
            foreach (XmlElement p in doc.SelectNodes("//w:body/w:p", ns))
            {
                var para = new Para();
                var pPr = (XmlElement)p.SelectSingleNode("w:pPr", ns);
                var jc = pPr == null ? null : (XmlElement)pPr.SelectSingleNode("w:jc", ns);
                var sp = pPr == null ? null : (XmlElement)pPr.SelectSingleNode("w:spacing", ns);
                if (jc != null) para.Align = jc.GetAttribute("val", W);
                para.Line = Int(sp, "line", 240); para.Before = Int(sp, "before", 0); para.After = Int(sp, "after", 0);
                para.KeepNext = pPr != null && pPr.SelectSingleNode("w:keepNext", ns) != null;
                foreach (XmlElement r in p.SelectNodes("w:r", ns))
                {
                    bool b = r.SelectSingleNode("w:rPr/w:b", ns) != null;
                    var t = r.SelectSingleNode("w:t", ns);
                    if (t == null) continue;
                    string text = t.InnerText.Replace("\t", "    ");
                    string[] parts = text.Split(' ');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        // A run that ends with a space ("Depoente: ") keeps the gap before the next run.
                        if (parts[i].Length == 0) { if (i > 0 && para.Words.Count > 0) para.Words.Add(null); continue; }
                        if (i > 0 && para.Words.Count > 0 && para.Words[para.Words.Count - 1] != null) para.Words.Add(null);
                        var w = new Word(); w.Text = parts[i]; w.Bold = b;
                        w.Width = g.MeasureString(w.Text, b ? bold : regular, PointF.Empty, StringFormat.GenericTypographic).Width;
                        para.Words.Add(w);
                    }
                }
                // null = a space between words; words glued without null belong together.
                var line = new List<Word>(); float width = 0; bool pendingSpace = false;
                foreach (Word w in para.Words)
                {
                    if (w == null) { pendingSpace = line.Count > 0; continue; }
                    float add = (pendingSpace ? space : 0) + w.Width;
                    if (line.Count > 0 && width + add > contentW) { para.Lines.Add(line); line = new List<Word>(); width = 0; pendingSpace = false; add = w.Width; }
                    if (pendingSpace) line.Add(null);
                    line.Add(w); width += add; pendingSpace = false;
                }
                para.Lines.Add(line);
                para.LineHeight = single * para.Line / 240f;
                para.Height = px(para.Before) + para.Lines.Count * para.LineHeight + px(para.After);
                paras.Add(para);
            }

            // Pagination with keep-with-next groups.
            var placed = new List<Placed>(); int page = 0; float y = top, limit = pageH - bottom;
            for (int i = 0; i < paras.Count; i++)
            {
                float group = 0; int j = i;
                while (true) { group += paras[j].Height; if (!paras[j].KeepNext || j + 1 >= paras.Count) break; j++; }
                if (y + group > limit && y > top && group <= limit - top) { page++; y = top; }
                if (y + paras[i].Height > limit && y > top) { page++; y = top; }
                var pl = new Placed(); pl.P = paras[i]; pl.Page = page; pl.Top = y; placed.Add(pl);
                y += paras[i].Height;
            }
            int pages = page + 1;

            // Draw pages side by side.
            int gap = 36, caption = 44;
            using (var bmp = new Bitmap((int)(pages * pageW + (pages + 1) * gap), (int)(pageH + 2 * gap + caption)))
            using (Graphics d = Graphics.FromImage(bmp))
            using (var captionFont = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point))
            {
                bmp.SetResolution(dpi, dpi);
                d.SmoothingMode = SmoothingMode.AntiAlias; d.TextRenderingHint = TextRenderingHint.AntiAlias;
                d.Clear(Color.FromArgb(222, 224, 228));
                d.DrawString("Prévia do .docx gerado (desenhada a partir do arquivo; Word não está instalado neste computador)  —  " + Path.GetFileName(docx) + "  —  " + pages + (pages == 1 ? " página" : " páginas"),
                    captionFont, new SolidBrush(Color.FromArgb(70, 70, 80)), gap, gap / 2 + 4);
                for (int pg = 0; pg < pages; pg++)
                {
                    float ox = gap + pg * (pageW + gap), oy = gap + caption;
                    d.FillRectangle(new SolidBrush(Color.FromArgb(40, 0, 0, 0)), ox + 3, oy + 4, pageW, pageH);
                    d.FillRectangle(Brushes.White, ox, oy, pageW, pageH);
                    // Faint margin corners, as Word shows them.
                    using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                    {
                        float[,] c = { { left, top, -1, -1 }, { pageW - right, top, 1, -1 }, { left, pageH - bottom, -1, 1 }, { pageW - right, pageH - bottom, 1, 1 } };
                        for (int k = 0; k < 4; k++)
                        {
                            d.DrawLine(pen, ox + c[k, 0], oy + c[k, 1], ox + c[k, 0] + 18 * c[k, 2], oy + c[k, 1]);
                            d.DrawLine(pen, ox + c[k, 0], oy + c[k, 1], ox + c[k, 0], oy + c[k, 1] + 18 * c[k, 3]);
                        }
                    }
                    string num = "Página " + (pg + 1) + " de " + pages;
                    float nw = d.MeasureString(num, small, PointF.Empty, StringFormat.GenericTypographic).Width;
                    d.DrawString(num, small, Brushes.Black, ox + pageW - right - nw, oy + pageH - footerDist - small.GetHeight(dpi), StringFormat.GenericTypographic);
                }
                foreach (Placed pl in placed)
                {
                    float ox = gap + pl.Page * (pageW + gap) + left, oy = gap + caption + pl.Top + px(pl.P.Before);
                    for (int li = 0; li < pl.P.Lines.Count; li++)
                    {
                        List<Word> line = pl.P.Lines[li];
                        float natural = 0; int gaps = 0;
                        foreach (Word w in line) { if (w == null) { natural += space; gaps++; } else natural += w.Width; }
                        float extra = 0, x = ox;
                        bool last = li == pl.P.Lines.Count - 1;
                        if (pl.P.Align == "both" && !last && gaps > 0) extra = (contentW - natural) / gaps;
                        else if (pl.P.Align == "center") x += (contentW - natural) / 2;
                        else if (pl.P.Align == "right") x += contentW - natural;
                        float baseTop = oy + li * pl.P.LineHeight;
                        foreach (Word w in line)
                        {
                            if (w == null) { x += space + extra; continue; }
                            d.DrawString(w.Text, w.Bold ? bold : regular, Brushes.Black, x, baseTop, StringFormat.GenericTypographic);
                            x += w.Width;
                        }
                    }
                }
                bmp.Save(png, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
'@
Add-Type -TypeDefinition $code -ReferencedAssemblies @('System.dll', 'System.Core.dll', 'System.Xml.dll', 'System.Drawing.dll', 'System.IO.Compression.dll', 'System.IO.Compression.FileSystem.dll')
[DocxPreview]::Render((Resolve-Path -LiteralPath $Docx).Path, [IO.Path]::GetFullPath($Png), $Dpi)
Write-Output ("Prévia gravada: " + $Png)
