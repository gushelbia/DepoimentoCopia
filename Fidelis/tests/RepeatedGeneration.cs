using System.Reflection;
using System.Threading;

public static class RepeatedGeneration
{
    private static string logPath;

    private static void Log(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(logPath, message + Environment.NewLine, Encoding.UTF8);
    }

    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    }

    private static void WaitFor(Func<bool> done, int seconds, string operation)
    {
        Stopwatch timer = Stopwatch.StartNew();
        while (!done())
        {
            if (timer.Elapsed.TotalSeconds > seconds) throw new Exception("Timeout: " + operation);
            Application.DoEvents();
            System.Threading.Thread.Sleep(50);
        }
    }

    private static void Dump(OriginalAppBridge bridge, string phase)
    {
        Log(phase + " | status=" + bridge.StatusText + " | reformulate=" + bridge.ButtonEnabled("Reformular") + " | cancel=" + bridge.ButtonEnabled("Cancelar"));
        foreach (IntPtr h in Field<List<IntPtr>>(bridge, "editors"))
            Log("editor=" + h + " valid=" + NativeBridgeApi.IsWindow(h) + " enabled=" + NativeBridgeApi.IsWindowEnabled(h) + " length=" + OriginalAppBridge.GetText(h).Length);
    }

    public static void Run(string root)
    {
        logPath = Path.Combine(root, "tests", "last-run.txt");
        File.WriteAllText(logPath, "COMPILE PASS " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
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
            using (ModernDepoimentoForm form = new ModernDepoimentoForm(bridge, "", "Vulkan"))
            {
                form.Show();
                Application.DoEvents();
                Log("ENGINE PID=" + engine.Id + " MODEL=" + bridge.ModelPath);
                Field<ModernButton>(form, "loadModel").PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "modelWasLoading"); }, 180, "load model");
                Dump(bridge, "LOADED");
                string[] inputs = {
                    "Eu estava em casa na segunda-feira quando ouvi um barulho na rua. Fui ate a janela e vi uma bicicleta caida na calcada.",
                    "Na terca-feira, cheguei ao mercado as nove horas. Comprei pao e leite e voltei para casa de onibus.",
                    "Na quarta-feira, encontrei meu vizinho na portaria. Ele me entregou uma encomenda que havia recebido pela manha."
                };
                SectionCard input = Field<SectionCard>(form, "originalCard");
                ModernButton button = Field<ModernButton>(form, "reformulate");
                string previousOutput = "";
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (i == 1)
                    {
                        // Simulate cached HWNDs invalidated between generations.
                        Field<List<IntPtr>>(bridge, "editors")[1] = IntPtr.Zero;
                        Field<Dictionary<string, IntPtr>>(bridge, "buttons")["Reformular"] = IntPtr.Zero;
                    }
                    if (!input.Editor.Enabled || input.Editor.ReadOnly || !button.Enabled) throw new Exception("UI locked before generation " + (i + 1));
                    input.Editor.SelectAll();
                    input.Editor.SelectedText = "";
                    if (input.Editor.TextLength != 0) throw new Exception("Input clear failed");
                    input.Editor.SelectedText = inputs[i];
                    button.PerformClick();
                    WaitFor(delegate { return !Field<bool>(form, "generationStarting"); }, 10, "generation dispatch");
                    Dump(bridge, "START " + (i + 1));
                    if (bridge.OriginalText != inputs[i]) throw new Exception("Input not delivered to engine on generation " + (i + 1));
                    if (button.Enabled) throw new Exception("Button remained enabled during generation");
                    WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 240, "generation " + (i + 1));
                    Dump(bridge, "END " + (i + 1));
                    string output = Field<SectionCard>(form, "reformulatedCard").Editor.Text;
                    if (String.IsNullOrWhiteSpace(output) || !bridge.StatusText.StartsWith("Conclu")) throw new Exception("Generation failed: " + bridge.StatusText);
                    if (output == previousOutput) throw new Exception("Stale output reused");
                    previousOutput = output;
                    if (!button.Enabled || Field<ModernButton>(form, "cancel").Enabled || !input.Editor.Enabled || input.Editor.ReadOnly) throw new Exception("UI not restored");
                    Log("PASS " + (i + 1) + " OUTPUT=" + output);
                }
                input.Editor.Text = inputs[0];
                button.PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "generationStarting"); }, 10, "dispatch before cancellation");
                Field<ModernButton>(form, "cancel").PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 60, "cancellation");
                if (!button.Enabled || Field<ModernButton>(form, "cancel").Enabled) throw new Exception("Cancellation left UI locked");
                Log("CANCEL PASS status=" + bridge.StatusText);
                input.Editor.Text = inputs[1];
                button.PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 240, "generation after cancellation");
                if (!bridge.StatusText.StartsWith("Conclu") || String.IsNullOrWhiteSpace(bridge.ReformulatedText)) throw new Exception("Generation after cancellation failed");
                Log("AFTER CANCEL PASS OUTPUT=" + bridge.ReformulatedText);
                bridge.Stop();
                engine.WaitForExit(5000);
                button.PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 10, "dispatch error");
                if (button.Enabled || bridge.ModelReady || Field<ModernButton>(form, "cancel").Enabled) throw new Exception("Dead engine left generation enabled");
                Log("ERROR RECOVERY PASS status=" + Field<Label>(form, "status").Text);
                form.Close();
            }
        }
        finally { bridge.Stop(); }
    }
}



