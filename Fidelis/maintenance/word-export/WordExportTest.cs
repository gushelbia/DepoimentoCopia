// Word export: formatting (Times New Roman 12, justified, 1.5, A4, margins
// 3/3/2/2 cm, page number), qualification header without empty fields,
// signature lines, text unchanged, and the program's buttons using it.
public static class WordExportTest
{
    private const string WNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private static string reportPath;
    private static int failures;

    private static void Log(string m) { Console.WriteLine(m); File.AppendAllText(reportPath, m + Environment.NewLine, Encoding.UTF8); }
    private static void Check(bool ok, string name, string detail)
    {
        if (!ok) failures++;
        Log((ok ? "PASS " : "FAIL ") + name + (String.IsNullOrEmpty(detail) ? "" : " | " + detail));
    }
    private static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }
    private static bool WaitFor(Func<bool> done, int ms) { var t = Stopwatch.StartNew(); while (!done()) { if (t.ElapsedMilliseconds > ms) return false; Application.DoEvents(); System.Threading.Thread.Sleep(20); } return true; }
    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
    private static object Call(object target, string name, params object[] args)
    {
        return target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, args);
    }

    // The parts of a .docx, parsed.
    public sealed class Docx
    {
        public Dictionary<string, System.Xml.XmlDocument> Parts = new Dictionary<string, System.Xml.XmlDocument>();
        public System.Xml.XmlNamespaceManager Ns;
        public System.Xml.XmlDocument Document { get { return Parts["word/document.xml"]; } }
        public List<System.Xml.XmlElement> Paragraphs()
        {
            var list = new List<System.Xml.XmlElement>();
            foreach (System.Xml.XmlNode p in Document.SelectNodes("//w:body/w:p", Ns)) list.Add((System.Xml.XmlElement)p);
            return list;
        }
        public string Text(System.Xml.XmlElement p)
        {
            var sb = new StringBuilder();
            foreach (System.Xml.XmlNode t in p.SelectNodes(".//w:t", Ns)) sb.Append(t.InnerText);
            return sb.ToString();
        }
        public string Attr(System.Xml.XmlNode n, string xpath, string attr)
        {
            var e = n.SelectSingleNode(xpath, Ns) as System.Xml.XmlElement;
            return e == null ? null : e.GetAttribute(attr, WNs);
        }
    }

    public static Docx Open(string path)
    {
        var d = new Docx();
        using (ZipArchive zip = ZipFile.OpenRead(path))
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                var x = new System.Xml.XmlDocument();
                using (Stream s = entry.Open()) x.Load(s); // throws if not well-formed
                d.Parts[entry.FullName] = x;
            }
        d.Ns = new System.Xml.XmlNamespaceManager(new System.Xml.NameTable());
        d.Ns.AddNamespace("w", WNs);
        d.Ns.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
        d.Ns.AddNamespace("rel", "http://schemas.openxmlformats.org/package/2006/relationships");
        d.Ns.AddNamespace("ct", "http://schemas.openxmlformats.org/package/2006/content-types");
        return d;
    }

    public static Qualification Sample()
    {
        var q = new Qualification();
        q[Qualification.Procedimento] = "IP 0457/2026";
        q[Qualification.Unidade] = "1ª Delegacia de Polícia Civil de Exemplópolis";
        q[Qualification.Local] = "Sala de oitivas 2";
        q[Qualification.DataHora] = "30/09/2026 14:30";
        q[Qualification.Depoente] = "Maria Exemplo da Silva";
        q[Qualification.Documento] = "CPF 529.982.247-25";
        q[Qualification.Condicao] = "testemunha";
        q[Qualification.Endereco] = "Rua das Amostras, 100, Centro, Exemplópolis";
        q[Qualification.Telefone] = "(31) 99999-0000";
        q[Qualification.Autoridade] = "Dr. João Fictício Pereira";
        q[Qualification.Escrivao] = "Ana Modelo Souza";
        return q;
    }

    public static string SampleText()
    {
        return "A depoente declarou que, no dia 12 de agosto de 2026, por volta das 14h30, estava na portaria do Edifício Horizonte, onde trabalha como recepcionista há cerca de três anos. Afirmou que um homem de aproximadamente 40 anos, usando boné azul, entrou no prédio e pediu para falar com o síndico.\r\n\r\n" +
            "Disse que informou ao homem que o síndico não estava e que ele poderia deixar um recado. Relatou que o homem se mostrou nervoso, bateu com a mão no balcão e disse que voltaria mais tarde. Acrescentou que o fato foi presenciado pelo porteiro, de nome Carlos.\r\n\r\n" +
            "Esclareceu que o homem saiu em um veículo de cor prata, placa ABC1D23, e que não conseguiu ver quem dirigia. Afirmou que, no dia seguinte, recebeu uma ligação do número (31) 98888-1111, na qual uma voz masculina perguntou se o síndico já havia voltado de viagem.\r\n\r\n" +
            "Informou que comunicou os fatos ao síndico por mensagem e que ele orientou a registrar a ocorrência. Declarou que não conhecia o homem anteriormente e que não sabe informar o motivo da visita. Nada mais disse nem lhe foi perguntado.";
    }

    private static List<string> HeaderLines(Docx d)
    {
        var lines = new List<string>();
        foreach (System.Xml.XmlElement p in d.Paragraphs())
            if (p.SelectSingleNode("w:r[1]/w:rPr/w:b", d.Ns) != null && d.Attr(p, "w:pPr/w:jc", "val") == "left") lines.Add(d.Text(p));
        return lines;
    }

    private static List<string> BodyLines(Docx d)
    {
        var lines = new List<string>();
        foreach (System.Xml.XmlElement p in d.Paragraphs())
            if (d.Attr(p, "w:pPr/w:jc", "val") == "both") lines.Add(d.Text(p));
        return lines;
    }

    private static List<string> CenterLines(Docx d)
    {
        var lines = new List<string>();
        foreach (System.Xml.XmlElement p in d.Paragraphs())
            if (d.Attr(p, "w:pPr/w:jc", "val") == "center") lines.Add(d.Text(p));
        return lines;
    }

    private static string Expected(string text)
    {
        var lines = new List<string>();
        foreach (string l in text.Replace("\r\n", "\n").Split('\n')) if (l.Trim().Length > 0) lines.Add(l);
        return String.Join("\n", lines.ToArray());
    }

    public static int Run(string root, string outDir)
    {
        reportPath = Path.Combine(root, "maintenance", "word-export", "results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Directory.CreateDirectory(outDir);
        try
        {
            // 1. Full example: formatting and header.
            string full = Path.Combine(outDir, "exemplo-completo.docx");
            SimpleDocx.Write(full, SampleText(), Sample());
            Docx d = Open(full);
            string[] parts = { "[Content_Types].xml", "_rels/.rels", "word/document.xml", "word/_rels/document.xml.rels", "word/styles.xml", "word/footer1.xml" };
            var missing = new List<string>(); foreach (string p in parts) if (!d.Parts.ContainsKey(p)) missing.Add(p);
            Check(missing.Count == 0, "1: .docx completo e com XML válido (documento, estilos, rodapé, relações)", String.Join(", ", missing.ToArray()));

            var sect = d.Document.SelectSingleNode("//w:body/w:sectPr", d.Ns);
            Check(d.Attr(sect, "w:pgSz", "w") == "11906" && d.Attr(sect, "w:pgSz", "h") == "16838", "1: página A4", null);
            Check(d.Attr(sect, "w:pgMar", "top") == "1701" && d.Attr(sect, "w:pgMar", "left") == "1701" && d.Attr(sect, "w:pgMar", "right") == "1134" && d.Attr(sect, "w:pgMar", "bottom") == "1134",
                "1: margens 3 cm (esquerda, em cima) e 2 cm (direita, embaixo)",
                "top=" + d.Attr(sect, "w:pgMar", "top") + " left=" + d.Attr(sect, "w:pgMar", "left") + " right=" + d.Attr(sect, "w:pgMar", "right") + " bottom=" + d.Attr(sect, "w:pgMar", "bottom"));

            var styles = d.Parts["word/styles.xml"];
            var rpr = styles.SelectSingleNode("//w:docDefaults/w:rPrDefault/w:rPr", d.Ns);
            Check(d.Attr(rpr, "w:rFonts", "ascii") == "Times New Roman" && d.Attr(rpr, "w:rFonts", "hAnsi") == "Times New Roman" && d.Attr(rpr, "w:rFonts", "cs") == "Times New Roman" && d.Attr(rpr, "w:sz", "val") == "24",
                "1: fonte Times New Roman 12 em todo o documento", null);
            bool noOtherFont = d.Document.SelectNodes("//w:rFonts", d.Ns).Count == 0 && d.Document.SelectNodes("//w:sz", d.Ns).Count == 0;
            Check(noOtherFont, "1: nenhum trecho do corpo troca a fonte ou o tamanho", null);

            List<string> body = BodyLines(d);
            bool bodyOk = body.Count > 0;
            foreach (System.Xml.XmlElement p in d.Paragraphs())
                if (d.Attr(p, "w:pPr/w:jc", "val") == "both" && d.Attr(p, "w:pPr/w:spacing", "line") != "360") bodyOk = false;
            Check(bodyOk, "1: texto justificado com espaçamento 1,5", body.Count + " parágrafos");
            Check(String.Join("\n", body.ToArray()) == Expected(SampleText()), "1: texto do consolidado vai para o Word sem alteração", null);

            var footerRef = sect.SelectSingleNode("w:footerReference", d.Ns) as System.Xml.XmlElement;
            string rid = footerRef == null ? "" : footerRef.GetAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            var rel = d.Parts["word/_rels/document.xml.rels"].SelectSingleNode("//rel:Relationship[@Id='" + rid + "']", d.Ns) as System.Xml.XmlElement;
            var footer = d.Parts["word/footer1.xml"];
            bool pageField = footer.SelectSingleNode("//w:fldSimple[contains(@w:instr,'PAGE')]", d.Ns) != null;
            bool ctFooter = d.Parts["[Content_Types].xml"].SelectSingleNode("//ct:Override[@PartName='/word/footer1.xml']", d.Ns) != null;
            Check(rel != null && rel.GetAttribute("Target") == "footer1.xml" && pageField && ctFooter, "1: numeração de páginas no rodapé", "rid=" + rid);

            List<string> header = HeaderLines(d);
            string[] expectedHeader = {
                "Procedimento nº: IP 0457/2026", "Unidade: 1ª Delegacia de Polícia Civil de Exemplópolis", "Local: Sala de oitivas 2",
                "Data e hora: 30/09/2026 14:30", "Depoente: Maria Exemplo da Silva", "Documento: CPF 529.982.247-25", "Condição: testemunha",
                "Endereço: Rua das Amostras, 100, Centro, Exemplópolis", "Telefone: (31) 99999-0000", "Autoridade: Dr. João Fictício Pereira", "Escrivão: Ana Modelo Souza" };
            Check(String.Join("|", header.ToArray()) == String.Join("|", expectedHeader), "1: cabeçalho com os 11 campos, rótulos em negrito", String.Join(" / ", header.ToArray()));
            List<string> center = CenterLines(d);
            Check(center.Count > 0 && center[0] == "TERMO DE DEPOIMENTO", "1: título conforme a condição (testemunha → TERMO DE DEPOIMENTO)", center.Count > 0 ? center[0] : "");

            // Signatures at the end, kept together.
            string tail = String.Join("|", center.GetRange(1, center.Count - 1).ToArray());
            string line = "_______________________________________";
            Check(tail == "|" + line + "|Maria Exemplo da Silva|Depoente|" + line + "|Dr. João Fictício Pereira|Autoridade|" + line + "|Ana Modelo Souza|Escrivão", "1: linhas de assinatura do depoente, da autoridade e do escrivão, com os nomes", tail);
            var paras = d.Paragraphs();
            bool atEnd = d.Text(paras[paras.Count - 1]) == "Escrivão";
            bool together = true;
            for (int i = paras.Count - 10; i < paras.Count - 1; i++) if (paras[i].SelectSingleNode("w:pPr/w:keepNext", d.Ns) == null) together = false;
            var bodyParas = new List<System.Xml.XmlElement>(); foreach (System.Xml.XmlElement p in paras) if (d.Attr(p, "w:pPr/w:jc", "val") == "both") bodyParas.Add(p);
            bool lastWithSignatures = bodyParas[bodyParas.Count - 1].SelectSingleNode("w:pPr/w:keepNext", d.Ns) != null && bodyParas[0].SelectSingleNode("w:pPr/w:keepNext", d.Ns) == null;
            Check(atEnd && together && lastWithSignatures, "1: assinaturas no final, sem se separar entre páginas e junto com o último parágrafo", null);

            // 2. Empty fields do not appear.
            var partial = new Qualification();
            partial[Qualification.DataHora] = "30/09/2026 15:00";
            partial[Qualification.Depoente] = "José Teste";
            partial[Qualification.Condicao] = "vítima";
            string partialPath = Path.Combine(outDir, "exemplo-parcial.docx");
            SimpleDocx.Write(partialPath, "Texto curto.", partial);
            Docx dp = Open(partialPath);
            List<string> ph = HeaderLines(dp);
            string allText = dp.Document.InnerText;
            Check(String.Join("|", ph.ToArray()) == "Data e hora: 30/09/2026 15:00|Depoente: José Teste|Condição: vítima"
                && !allText.Contains("Procedimento") && !allText.Contains("Endereço") && !allText.Contains("Telefone") && !allText.Contains("Escrivão:") && !allText.Contains("Unidade"),
                "1: campos vazios não aparecem no cabeçalho", String.Join(" / ", ph.ToArray()));
            List<string> pc = CenterLines(dp);
            Check(pc[0] == "TERMO DE DECLARAÇÕES" && String.Join("|", pc.GetRange(1, pc.Count - 1).ToArray()) == "|" + line + "|José Teste|Depoente|" + line + "|Autoridade|" + line + "|Escrivão",
                "1: autoridade e escrivão sem nome ficam só com a linha e o rótulo", String.Join("|", pc.ToArray()));

            // 3. No qualification at all (old callers).
            string plainPath = Path.Combine(outDir, "exemplo-sem-qualificacao.docx");
            SimpleDocx.Write(plainPath, "Linha 1\r\nLinha 2");
            Docx dn = Open(plainPath);
            Check(HeaderLines(dn).Count == 0 && String.Join("\n", BodyLines(dn).ToArray()) == "Linha 1\nLinha 2" && CenterLines(dn)[0] == "TERMO DE DEPOIMENTO",
                "1: sem qualificação, sem cabeçalho de campos (compatível com a chamada antiga)", null);

            // 4. Special characters survive.
            var odd = new Qualification();
            odd[Qualification.Depoente] = "Ana \"Nina\" O'Neil & Cia <teste>";
            string oddText = "Disse: \"R$ 50,00 < R$ 100,00 & mais\" — ação, fé, coração.\tFim.";
            string oddPath = Path.Combine(outDir, "exemplo-caracteres.docx");
            SimpleDocx.Write(oddPath, oddText, odd);
            Docx dx = Open(oddPath);
            Check(BodyLines(dx)[0] == oddText && HeaderLines(dx)[0] == "Depoente: Ana \"Nina\" O'Neil & Cia <teste>", "1: aspas, &, <, acentos e tabulação preservados", null);

            // 5. Title by condition.
            string[] conds = { "vítima", "testemunha", "investigado", "declarante", "" };
            string[] titles = { "TERMO DE DECLARAÇÕES", "TERMO DE DEPOIMENTO", "TERMO DE INTERROGATÓRIO", "TERMO DE DECLARAÇÕES", "TERMO DE DEPOIMENTO" };
            bool titlesOk = true; var got = new List<string>();
            for (int i = 0; i < conds.Length; i++)
            {
                var q = new Qualification(); q[Qualification.Condicao] = conds[i];
                got.Add(SimpleDocx.Title(q)); if (SimpleDocx.Title(q) != titles[i]) titlesOk = false;
            }
            Check(titlesOk, "1: título por condição (vítima/declarante → declarações; investigado → interrogatório; testemunha/vazio → depoimento)", String.Join(", ", got.ToArray()));

            // 6. Overwrite an existing file.
            SimpleDocx.Write(oddPath, "Substituído.", null);
            Check(BodyLines(Open(oddPath))[0] == "Substituído.", "1: substitui um arquivo existente", null);
        }
        catch (Exception ex) { failures++; Log("FAIL execução (unidade): " + ex); }

        RunUi(root, outDir);
        Log(failures == 0 ? "RESULT PASS" : "RESULT FAIL (" + failures + ")");
        return failures;
    }

    // The buttons: «Exportar Word» and the Word option when closing use the panel fields.
    private static void RunUi(string root, string outDir)
    {
        Application.EnableVisualStyles();
        ThemeChoice savedTheme = ThemeManager.LoadChoice();
        string realCopy = AutosaveStore.FilePath + ".antes-do-teste";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        DialogKit.Start();
        Process engine = null;
        ModernDepoimentoForm form = null;
        try
        {
            var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
            start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
            engine = Process.Start(start);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
            form.Show(); Pump(800);

            Qualification sample = Sample();
            sample[Qualification.Documento] = "CPF 123.456.789-00"; // invalid: warns, never blocks
            Call(form, "ApplyQualification", sample); Pump(200);
            var combined = Field<SectionCard>(form, "combinedCard");
            combined.Editor.Text = SampleText(); Pump(200);

            string exported = Path.Combine(outDir, "ui-exportar.docx");
            if (File.Exists(exported)) File.Delete(exported);
            DialogKit.Expect save = DialogKit.ArmFile("Salvar", exported);
            Call(form, "Export_Click", null, EventArgs.Empty);
            Pump(600);
            bool written = WaitFor(delegate { return File.Exists(exported); }, 8000);
            Check(save.Done && written, "1: «Exportar Word» grava o arquivo escolhido", save.Text);
            if (written)
            {
                Docx d = Open(exported);
                List<string> header = HeaderLines(d);
                Check(header.Contains("Depoente: Maria Exemplo da Silva") && header.Contains("Procedimento nº: IP 0457/2026") && header.Contains("Escrivão: Ana Modelo Souza"),
                    "1: «Exportar Word» usa os campos de qualificação da tela", String.Join(" / ", header.ToArray()));
                Check(header.Contains("Documento: CPF 123.456.789-00"), "1: CPF inválido só avisa, não impede o Word", null);
                Check(String.Join("\n", BodyLines(d).ToArray()) == Expected(SampleText()), "1: texto do consolidado sem alteração no Word exportado", null);
            }
            Check(!(bool)Call(form, "HasUnsavedChanges"), "1: exportar marca o consolidado como salvo (proteção de saída mantida)", null);

            // The Word option offered when closing with unsaved text.
            combined.Editor.Text = SampleText() + "\r\n\r\nParágrafo novo."; Pump(200);
            string closing = Path.Combine(outDir, "ui-ao-fechar.docx");
            if (File.Exists(closing)) File.Delete(closing);
            DialogKit.Expect save2 = DialogKit.ArmFile("Salvar", closing);
            bool ok = (bool)Call(form, "SaveCombinedToFile", true);
            bool written2 = WaitFor(delegate { return File.Exists(closing); }, 8000);
            Check(ok && written2 && HeaderLines(Open(closing)).Contains("Autoridade: Dr. João Fictício Pereira") && BodyLines(Open(closing)).Contains("Parágrafo novo."),
                "1: Word salvo ao fechar também leva o cabeçalho", null);

            // Cancel in the dialog writes nothing.
            string cancelled = Path.Combine(outDir, "ui-cancelado.docx");
            if (File.Exists(cancelled)) File.Delete(cancelled);
            DialogKit.Expect cancel = DialogKit.ArmFile("Salvar", null);
            Call(form, "Export_Click", null, EventArgs.Empty); Pump(600);
            Check(cancel.Done && !File.Exists(cancelled), "1: cancelar a janela de salvar não grava nada", null);
            Check(DialogKit.Unexpected.Count == 0, "1: nenhuma janela inesperada", String.Join(", ", DialogKit.Unexpected.ToArray()));
        }
        catch (Exception ex) { failures++; Log("FAIL execução (interface): " + ex); }
        finally
        {
            DialogKit.Stop();
            if (form != null) { try { Field<SectionCard>(form, "combinedCard").Editor.Text = ""; Call(form, "ApplyQualification", new Qualification()); form.Close(); form.Dispose(); } catch { } }
            try { if (engine != null && !engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(savedTheme);
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
        }
    }
}
