public static class PromptExamples
{
    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }

    private static void WaitFor(Func<bool> done, int seconds)
    {
        Stopwatch timer = Stopwatch.StartNew();
        while (!done())
        {
            if (timer.Elapsed.TotalSeconds > seconds) throw new Exception("Timeout awaiting model");
            Application.DoEvents();
            System.Threading.Thread.Sleep(50);
        }
    }

    public static void Run(string root, string engineDirectory)
    {
        string log = Path.Combine(root, "maintenance", "prompt-update", "examples-results.txt");
        File.WriteAllText(log, "Prompt examples " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(engineDirectory, "DepoimentoLocal.exe"));
        psi.WorkingDirectory = engineDirectory;
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
                WaitFor(delegate { return !Field<bool>(form, "modelWasLoading"); }, 180);
                string[] inputs = {
                    "Não entendi o que aconteceu porque fiquei fora da sala. Acho que era o Paulo. Não sei quem abriu a porta. Não lembro a hora. Posso estar enganado e posso estar confundindo os nomes.",
                    "Quer dizer, acho que era o Paulo.",
                    "Eu saí... não, eu acho que antes de sair entrou o Carlos. É, o Carlos entrou antes.",
                    "Carlos me falou que o Paulo tentou encostar na porta. Me disseram que ela estava fechada. Eu não vi isso."
                };
                for (int i = 0; i < inputs.Length; i++)
                {
                    Field<SectionCard>(form, "originalCard").Editor.Text = inputs[i];
                    Field<ModernButton>(form, "reformulate").PerformClick();
                    WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 240);
                    string output = Field<SectionCard>(form, "reformulatedCard").Editor.Text;
                    string entry = "CASE " + (i + 1) + Environment.NewLine + "INPUT: " + inputs[i] + Environment.NewLine + "OUTPUT: " + output + Environment.NewLine + "STATUS: " + bridge.StatusText + Environment.NewLine;
                    Console.WriteLine(entry);
                    File.AppendAllText(log, entry, Encoding.UTF8);
                    if (String.IsNullOrWhiteSpace(output)) throw new Exception("Empty output");
                }
                form.Close();
            }
        }
        finally { bridge.Stop(); }
    }
}
