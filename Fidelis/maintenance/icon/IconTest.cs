// Fidelis icon on the window and taskbar, and on the shortcut from Criar-Atalho.
public static class IconTest
{
    private static string reportPath;
    private static int failures;
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
    [DllImport("user32.dll")] private static extern bool DrawIconEx(IntPtr dc, int x, int y, IntPtr icon, int w, int h, int step, IntPtr brush, int flags);

    private static void Log(string m) { Console.WriteLine(m); File.AppendAllText(reportPath, m + Environment.NewLine, Encoding.UTF8); }
    private static void Check(bool ok, string name, string detail)
    {
        if (!ok) failures++;
        Log((ok ? "PASS " : "FAIL ") + name + (String.IsNullOrEmpty(detail) ? "" : " | " + detail));
    }
    private static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }

    // Same pixels at a given size (compares what Windows draws, not file bytes).
    private static bool SamePixels(Icon a, Icon b, int size)
    {
        // Drawn by Windows (DrawIconEx through the handle), like the title bar and the
        // taskbar; ToBitmap() does not support PNG frames on .NET Framework.
        using (var ia = new Icon(a, size, size)) using (var ib = new Icon(b, size, size))
        using (Bitmap x = Draw(ia, size)) using (Bitmap y = Draw(ib, size))
        {
            if (x.Size != y.Size) return false;
            for (int i = 0; i < x.Width; i++) for (int j = 0; j < x.Height; j++) if (x.GetPixel(i, j) != y.GetPixel(i, j)) return false;
            return true;
        }
    }

    private static Bitmap Draw(Icon icon, int size)
    {
        var bmp = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.Magenta); IntPtr dc = g.GetHdc(); DrawIconEx(dc, 0, 0, icon.Handle, size, size, 0, IntPtr.Zero, 3); g.ReleaseHdc(dc); }
        return bmp;
    }

    public static int Run(string root, string work)
    {
        reportPath = Path.Combine(root, "maintenance", "icon", "results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Application.EnableVisualStyles();
        string fidelis = Path.Combine(root, "ui", "Fidelis.ico"), old = Path.Combine(root, "ui", "DepoimentoLocal.ico");
        string realCopy = AutosaveStore.FilePath + ".antes-do-teste";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        ThemeChoice savedTheme = ThemeManager.LoadChoice();
        Process engine = null;
        ModernDepoimentoForm form = null;
        try
        {
            Check(File.Exists(fidelis), "ícone ui/Fidelis.ico presente", null);
            using (var fi = new Icon(fidelis)) using (var oi = new Icon(old))
                Check(!SamePixels(fi, oi, 32), "o ícone novo é diferente do antigo (o teste distingue os dois)", null);

            var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
            start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
            engine = Process.Start(start);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, fidelis, "Vulkan");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
            form.Show(); Pump(800);
            using (var expected = new Icon(fidelis))
            {
                Check(form.Icon != null && SamePixels(form.Icon, expected, 16) && SamePixels(form.Icon, expected, 32) && SamePixels(form.Icon, expected, 48),
                    "janela usa o ícone Fidelis (16, 32 e 48 px)", null);
            }
            // WM_GETICON: what the title bar (small) and the taskbar / Alt+Tab (big) show.
            IntPtr small = SendMessage(form.Handle, 0x007F, (IntPtr)0, IntPtr.Zero), big = SendMessage(form.Handle, 0x007F, (IntPtr)1, IntPtr.Zero);
            bool smallOk = false, bigOk = false;
            using (var expected = new Icon(fidelis))
            {
                if (small != IntPtr.Zero) using (Icon s = Icon.FromHandle(small)) smallOk = SamePixels(s, expected, s.Width);
                if (big != IntPtr.Zero) using (Icon b = Icon.FromHandle(big)) bigOk = SamePixels(b, expected, b.Width);
            }
            Check(smallOk && bigOk, "ícones da barra de título e da barra de tarefas (WM_GETICON pequeno e grande) são os do Fidelis", "pequeno=" + small + " grande=" + big);

            // Title-bar print for approval.
            form.TopMost = true; form.Location = new Point(0, 0); form.Activate(); Pump(900);
            using (var bmp = new Bitmap(form.Width, 60))
            {
                using (var full = new Bitmap(form.Width, form.Height))
                {
                    using (Graphics g = Graphics.FromImage(full)) { IntPtr dc = g.GetHdc(); PrintWindow(form.Handle, dc, 2); g.ReleaseHdc(dc); }
                    using (Graphics g = Graphics.FromImage(bmp)) g.DrawImage(full, new Rectangle(0, 0, bmp.Width, bmp.Height), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                }
                bmp.Save(Path.Combine(root, "maintenance", "icon", "print-titulo.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            form.TopMost = false; form.Location = new Point(-4000, -4000);

            // Shortcut from Criar-Atalho.ps1 (written to a temporary folder, not the Desktop).
            Directory.CreateDirectory(work);
            bool tempExe = !File.Exists(Path.Combine(root, "Fidelis.exe"));
            if (tempExe) File.Copy(Path.Combine(root, "engine", "DepoimentoLocal.exe"), Path.Combine(root, "Fidelis.exe"));
            try
            {
                var psi = new ProcessStartInfo("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File \"" + Path.Combine(root, "Criar-Atalho.ps1") + "\" -Destino \"" + work + "\"");
                psi.UseShellExecute = false; psi.CreateNoWindow = true;
                using (Process p = Process.Start(psi)) p.WaitForExit(30000);
            }
            finally { if (tempExe) File.Delete(Path.Combine(root, "Fidelis.exe")); }
            string lnk = Path.Combine(work, "Fidelis.lnk");
            string iconLocation = "";
            if (File.Exists(lnk))
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { lnk });
                iconLocation = (string)shortcut.GetType().InvokeMember("IconLocation", System.Reflection.BindingFlags.GetProperty, null, shortcut, null);
            }
            Check(iconLocation.StartsWith(fidelis, StringComparison.OrdinalIgnoreCase), "atalho do Criar-Atalho usa ui/Fidelis.ico", iconLocation);
            Check(!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Fidelis.lnk")) || File.GetLastWriteTime(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Fidelis.lnk")) < DateTime.Now.AddMinutes(-1),
                "o atalho real da Área de Trabalho não foi tocado pelo teste", null);
        }
        catch (Exception ex) { failures++; Log("FAIL execução: " + ex); }
        finally
        {
            if (form != null) { try { form.Close(); form.Dispose(); } catch { } }
            try { if (engine != null && !engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(savedTheme);
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
            Log(failures == 0 ? "RESULT PASS" : "RESULT FAIL (" + failures + ")");
        }
        return failures;
    }
}
