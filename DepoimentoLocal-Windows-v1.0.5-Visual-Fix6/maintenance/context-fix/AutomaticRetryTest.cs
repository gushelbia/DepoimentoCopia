public static class AutomaticRetryTest
{
    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
    private static string ReadLog(string path)
    {
        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) return reader.ReadToEnd();
    }
    public static void Run(string root)
    {
        string work = Path.Combine(root, "maintenance", "context-fix");
        string source = File.ReadAllText(Path.Combine(work, "exact-original.txt"), Encoding.UTF8);
        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DepoimentoLocal", "logs", "depoimento-local.log");
        int logStart = File.Exists(logPath) ? ReadLog(logPath).Length : 0;
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
        psi.WorkingDirectory = Path.Combine(root, "engine");
        psi.UseShellExecute = true;
        psi.WindowStyle = ProcessWindowStyle.Minimized;
        Process engine = Process.Start(psi);
        OriginalAppBridge bridge = new OriginalAppBridge(engine);
        try
        {
            if (!bridge.Connect(12000)) throw new Exception("Engine connection failed");
            using (ModernDepoimentoForm form = new ModernDepoimentoForm(bridge, "", ""))
            {
                form.Show();
                Application.DoEvents();
                Field<ModernButton>(form, "loadModel").PerformClick();
                Stopwatch clock = Stopwatch.StartNew();
                while (Field<bool>(form, "modelWasLoading"))
                {
                    if (clock.Elapsed.TotalSeconds > 180) throw new Exception("Model load timeout");
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(50);
                }
                Field<SectionCard>(form, "originalCard").Editor.Text = source;
                Field<ModernButton>(form, "reformulate").PerformClick();
                clock.Restart();
                string runLog;
                while (true)
                {
                    if (clock.Elapsed.TotalSeconds > 240) throw new Exception("Automatic retry timeout");
                    Application.DoEvents();
                    string all = ReadLog(logPath);
                    if (all.Length < logStart) throw new Exception("Log rotated during test");
                    runLog = all.Substring(logStart);
                    if (runLog.Contains("[GENERATION_OK]") || runLog.Contains("[GENERATION_FAILED]")) break;
                    System.Threading.Thread.Sleep(100);
                }
                string output = bridge.ReformulatedText;
                File.WriteAllText(Path.Combine(work, "automatic-retry-log.txt"), runLog, Encoding.UTF8);
                File.WriteAllText(Path.Combine(work, "automatic-retry-output.txt"), output, Encoding.UTF8);
                string report = "UI input chars after normal Trim: " + source.Trim().Length + Environment.NewLine +
                    "Automatic retry started: " + runLog.Contains("[BLOCK_RETRY_START]") + Environment.NewLine +
                    "Automatic retry completed: " + runLog.Contains("[BLOCK_RETRY_END]") + Environment.NewLine +
                    "ContextOverflowException: " + runLog.Contains("ContextOverflowException") + Environment.NewLine +
                    "Generation accepted: " + runLog.Contains("[GENERATION_OK]") + Environment.NewLine +
                    "Generation rejected: " + runLog.Contains("[BLOCK_REJECTED]") + Environment.NewLine;
                Console.WriteLine(report);
                File.WriteAllText(Path.Combine(work, "automatic-retry-results.txt"), report, Encoding.UTF8);
                if (runLog.Contains("ContextOverflowException")) throw new Exception("Context overflow reproduced");
                if (runLog.Contains("[BLOCK_RETRY_START]") && !runLog.Contains("[BLOCK_RETRY_END]")) throw new Exception("Automatic retry did not complete");
                if (!runLog.Contains("[GENERATION_OK]") && !runLog.Contains("[BLOCK_RETRY_END]")) throw new Exception("Neither initial generation nor retry completed");
                // A validator rejection may show an engine-owned modal. Stop only
                // this test process after collecting its terminal log and output.
                bridge.Stop();
                form.Close();
            }
        }
        finally { bridge.Stop(); }
    }
}
