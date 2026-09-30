# Renders the marked text (saved by Test-IncompleteMarker.ps1) in the production
# "Texto reformulado" card. DrawToBitmap does not paint RichTextBox content, so
# the window is shown for about a second at the top-left corner and captured from the screen. No model needed.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File Render-Marker.ps1
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source=[IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
$render=@'
public static class MarkerRender
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern bool PrintWindow(System.IntPtr hwnd, System.IntPtr hdc, uint flags);
    public static string Run(string text, string png)
    {
        var form = new Form();
        form.FormBorderStyle = FormBorderStyle.None; form.ShowInTaskbar = false;
        form.StartPosition = FormStartPosition.Manual; form.Location = new Point(0, 0); form.TopMost = true;
        form.Size = new Size(1100, 330); form.Font = new Font("Segoe UI", 10F);
        var palette = UiPalette.Create(ThemeManager.ResolveDark(ThemeManager.LoadChoice()));
        form.BackColor = palette.Window;
        var card = new SectionCard("Texto reformulado", true, true);
        card.Dock = DockStyle.Fill; form.Controls.Add(card);
        card.ApplyPalette(palette);
        form.Show();
        card.Editor.Text = text;
        Application.DoEvents();
        var wait = System.Diagnostics.Stopwatch.StartNew();
        while (wait.ElapsedMilliseconds < 1200) { Application.DoEvents(); System.Threading.Thread.Sleep(30); }
        form.Refresh(); Application.DoEvents();
        using (var bmp = new Bitmap(form.Width, form.Height))
        {
            using (var g = Graphics.FromImage(bmp)) g.CopyFromScreen(form.Location, Point.Empty, form.Size);
            bmp.Save(png, System.Drawing.Imaging.ImageFormat.Png);
        }
        string shown = card.Editor.Text;
        form.Close(); form.Dispose();
        return shown;
    }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$render) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'marker-screen.rejeicao.txt'),[Text.Encoding]::UTF8)
$shown=[MarkerRender]::Run($text,(Join-Path $PSScriptRoot 'marker-screen.png'))
$same=$shown.Replace("`r`n","`n") -ceq $text.Replace("`r`n","`n")
"editor contém o texto marcado sem alteração: $same"
