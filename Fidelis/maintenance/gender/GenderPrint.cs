// Window prints for the gender field: suggestion with a clear clue, and a text
// converted to the feminine (invented sentences; nothing goes to the model).
public static class GenderPrint
{
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
    private static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }
    private static T Field<T>(object target, string name) { return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target); }
    private static void Call(object target, string name, params object[] args) { target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, args); }
    private static void Shot(Form form, string png)
    {
        form.TopMost = true; form.Location = new Point(0, 0); form.Activate(); Pump(900);
        using (var bmp = new Bitmap(form.Width, form.Height))
        {
            using (Graphics g = Graphics.FromImage(bmp)) { IntPtr dc = g.GetHdc(); PrintWindow(form.Handle, dc, 2); g.ReleaseHdc(dc); }
            bmp.Save(png, System.Drawing.Imaging.ImageFormat.Png);
        }
        form.TopMost = false; form.Location = new Point(-4000, -4000); Pump(200);
    }
    public static void Run(string root, string folder)
    {
        Application.EnableVisualStyles();
        ThemeChoice saved = ThemeManager.LoadChoice();
        string realCopy = AutosaveStore.FilePath + ".antes-do-print";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
        start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
        Process engine = Process.Start(start);
        ModernDepoimentoForm form = null;
        try
        {
            ThemeManager.SaveChoice(ThemeChoice.Light);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
            form.Show(); Pump(800);
            Field<ModernButton>(form, "qualToggle").PerformClick(); Pump(300);
            Field<SectionCard>(form, "originalCard").Editor.Text = "Eu fiquei nervosa e liguei para o meu irmão. Ele estava nervoso também e foi embora. Depois fui agredida pelo vizinho na frente do prédio.";
            Pump(1500);
            Shot(form, Path.Combine(folder, "print-sugestao.png"));
            Console.WriteLine("sugestão visível: " + Field<ModernButton>(form, "qualSuggest").Visible + " | " + Field<ModernButton>(form, "qualSuggest").Text);

            Field<ModernButton>(form, "qualSuggest").PerformClick(); Pump(300);
            Call(form, "ShowEngineOutput", "O depoente relatou que ficou nervoso e ligou para o seu irmão. Ele estava nervoso também e foi embora. Depois o depoente foi agredido pelo vizinho na frente do prédio.");
            Call(form, "ApplyHighlights");
            Pump(1500);
            Shot(form, Path.Combine(folder, "print-feminino.png"));
            Console.WriteLine("tela: " + Field<SectionCard>(form, "reformulatedCard").Editor.Text);
            Field<ModernButton>(form, "qualToggle").PerformClick(); Pump(300);
            Shot(form, Path.Combine(folder, "print-feminino-fechado.png"));
            Console.WriteLine("resumo: " + Field<Label>(form, "qualSummary").Text);
        }
        finally
        {
            if (form != null) { try { Field<SectionCard>(form, "combinedCard").Editor.Text = ""; Call(form, "ApplyQualification", new Qualification()); form.Close(); form.Dispose(); } catch { } }
            try { if (!engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(saved);
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
        }
    }
}
