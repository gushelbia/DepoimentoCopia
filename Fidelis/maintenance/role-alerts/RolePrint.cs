public static class RolePrint
{
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
    private static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }
    private static T Field<T>(object target, string name) { return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target); }
    public static void Run(string root, string png)
    {
        Application.EnableVisualStyles();
        ThemeChoice saved = ThemeManager.LoadChoice();
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
            Field<SectionCard>(form, "originalCard").Editor.Text = "Ele me disse que tinha visto ela sair com o carro. Eu estava correndo na praia quando ele me atacou por trás. Ela me atacou primeiro e eu só me defendi. Eu fiquei nervosa e liguei para o meu irmão.";
            Field<SectionCard>(form, "reformulatedCard").Editor.Text = "Relatou que o depoente lhe disse que tinha visto ela sair com o carro. O depoente estava correndo na praia quando ele o atacou por trás. Ela lhe atacou primeiro e o depoente só lhe defendeu. O depoente ficou nervoso e ligou para o seu irmão.";
            Pump(1500);
            form.TopMost = true; form.Location = new Point(0, 0); form.Activate(); Pump(900);
            using (var bmp = new Bitmap(form.Width, form.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { IntPtr dc = g.GetHdc(); PrintWindow(form.Handle, dc, 2); g.ReleaseHdc(dc); }
                bmp.Save(png, System.Drawing.Imaging.ImageFormat.Png);
            }
            Console.WriteLine(ReviewScanner.Details((ReviewResult)form.GetType().GetField("lastReview", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(form)));
        }
        finally
        {
            if (form != null) { try { Field<SectionCard>(form, "combinedCard").Editor.Text = ""; form.Close(); form.Dispose(); } catch { } }
            try { if (!engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(saved);
            AutosaveStore.Delete();
        }
    }
}