public static class ReadinessTest
{
    private static string reportPath;
    private static int engineId;
    private static int beats;
    private static int maxGap;
    private static Action diagnostic;
    private static Stopwatch beatClock = new Stopwatch();
    private delegate bool EnumProc(IntPtr window, IntPtr data);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint id);

    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
    private static void Call(object target, string name)
    {
        target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, new object[] { null, EventArgs.Empty });
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    private static void Log(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(reportPath, message + Environment.NewLine, Encoding.UTF8);
    }
    private static void WaitFor(Func<bool> done, int seconds, string operation)
    {
        Stopwatch timer = Stopwatch.StartNew();
        while (!done())
        {
            if (timer.Elapsed.TotalSeconds > seconds)
            {
                if (diagnostic != null) diagnostic();
                throw new Exception("Timeout: " + operation);
            }
            Application.DoEvents();
            System.Threading.Thread.Sleep(15);
        }
        Log("WAIT " + operation + " " + timer.ElapsedMilliseconds + "ms");
    }
    public static void Run(string root)
    {
        string testDir = Path.Combine(root, "maintenance", "model-readiness");
        reportPath = Path.Combine(testDir, "results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
        start.WorkingDirectory = Path.Combine(root, "engine");
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.WindowStyle = ProcessWindowStyle.Hidden;
        Process engine = Process.Start(start);
        engineId = engine.Id;
        OriginalAppBridge bridge = new OriginalAppBridge(engine);
        try
        {
            Check(bridge.Connect(12000), "Engine did not connect");
            using (var form = new ModernDepoimentoForm(bridge, "", "Vulkan"))
            using (var pulse = new Timer())
            {
                form.Show();
                Application.DoEvents();
                pulse.Interval = 50;
                pulse.Tick += delegate { maxGap = Math.Max(maxGap, (int)beatClock.ElapsedMilliseconds); beatClock.Restart(); beats++; };
                beatClock.Start(); pulse.Start();
                var button = Field<ModernButton>(form, "reformulate");
                var load = Field<ModernButton>(form, "loadModel");
                var input = Field<SectionCard>(form, "originalCard");
                var output = Field<SectionCard>(form, "reformulatedCard");
                var status = Field<Label>(form, "status");
                string previousStatus = "";
                pulse.Tick += delegate
                {
                    if (status.Text != previousStatus) { previousStatus = status.Text; Log("STATUS " + previousStatus); }
                };
                diagnostic = delegate { Log("DIAGNOSTIC UI=" + status.Text + " engine=" + bridge.StatusText + " loadButton=" + bridge.ButtonEnabled("Carregar modelo") + " starting=" + Field<bool>(form, "modelStarting") + " sync=" + Field<bool>(form, "syncInProgress")); };
                Log("PASS 1: abertura da interface com modelo ainda não carregado");

                input.Editor.Text = "Eu cheguei em casa as oito horas e encontrei meu vizinho na portaria.";
                string originalBefore = bridge.OriginalText;
                var clickTimer = Stopwatch.StartNew();
                button.PerformClick();
                clickTimer.Stop();
                Check(clickTimer.ElapsedMilliseconds < 300, "Unloaded click blocked UI");
                Check(status.Text == "Carregue o modelo antes de reformular o texto.", "Missing readiness message");
                Check(!Field<bool>(form, "generationWasRunning") && !bridge.ModelReady, "Unloaded generation started");
                Check(bridge.OriginalText == originalBefore, "Unloaded click sent input to engine");
                Check(!bridge.Click("Reformular") && !bridge.Click("Cancelar"), "Bridge readiness guard bypassed");
                int baseline = beats;
                WaitFor(delegate { return beats >= baseline + 5; }, 3, "responsividade sem modelo");
                Log("PASS 2-3: clique sem modelo retornou em " + clickTimer.ElapsedMilliseconds + "ms; apenas aviso local, sem envio ao motor; UI responsiva");

                string invalid = Path.Combine(testDir, "invalid-model.gguf");
                File.WriteAllText(invalid, "invalid model fixture");
                Field<TextBox>(form, "modelPath").Text = invalid;
                load.PerformClick();
                Check(!button.Enabled && Field<bool>(form, "modelWasLoading"), "Loading did not disable generation");
                Call(form, "Reformulate_Click");
                Check(!Field<bool>(form, "generationWasRunning"), "Generation allowed during loading");
                WaitFor(delegate { return !Field<bool>(form, "modelWasLoading"); }, 40, "falha de carregamento");
                Check(!bridge.ModelReady && !button.Enabled, "Failed load enabled generation");
                Check(bridge.StatusText.StartsWith("Falha ao carregar"), "Expected actual engine load failure");
                Log("PASS falha real de GGUF inválido: Reformular permanece desabilitado");

                Field<TextBox>(form, "modelPath").Text = Path.Combine("..", "modelo", "qwen2.5-3b-instruct-q4_k_m.gguf");
                load.PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "modelWasLoading"); }, 180, "carregamento real do GGUF");
                Check(bridge.ModelReady && button.Enabled, "Successful load did not enable generation: " + status.Text);
                Log("PASS 4: modelo carregado e confirmado; Reformular habilitado");

                for (int run = 1; run <= 2; run++)
                {
                    input.Editor.Text = run == 1
                        ? "Eu cheguei em casa as oito horas e encontrei meu vizinho na portaria. Ele me entregou uma encomenda."
                        : "Na segunda-feira, fui ao mercado comprar arroz. Paguei em dinheiro e voltei para casa de onibus.";
                    baseline = beats; maxGap = 0; beatClock.Restart(); clickTimer.Restart();
                    button.PerformClick(); clickTimer.Stop();
                    Check(clickTimer.ElapsedMilliseconds < 300, "Generation dispatch blocked UI");
                    Check(Field<bool>(form, "generationWasRunning") && !button.Enabled && !load.Enabled, "Missing generation lock");
                    Call(form, "Reformulate_Click"); // duplicate event must be ignored
                    Call(form, "LoadModel_Click"); // no model replacement during generation
                    WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 240, "geracao " + run);
                    Check(!String.IsNullOrWhiteSpace(output.Editor.Text) && bridge.StatusText.StartsWith("Conclu"), "Generation failed: " + status.Text);
                    Check(button.Enabled && load.Enabled && !Field<ModernButton>(form, "cancel").Enabled, "UI not restored");
                    Check(beats > baseline + 3 && maxGap < 1500, "UI heartbeat stalled during generation: " + maxGap);
                    Log("PASS 5 geracao " + run + ": clique=" + clickTimer.ElapsedMilliseconds + "ms; UI ticks=" + (beats - baseline) + "; maior intervalo=" + maxGap + "ms; texto=" + output.Editor.Text);
                }

                button.PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "generationStarting"); }, 5, "dispatch antes de cancelar");
                Field<ModernButton>(form, "cancel").PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 60, "cancelamento");
                Check(button.Enabled && bridge.ModelReady, "Cancellation discarded ready model");
                Log("PASS cancelamento protegido e modelo preservado");

                bridge.Stop(); engine.WaitForExit(5000);
                button.PerformClick();
                WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 5, "motor encerrado");
                Check(!button.Enabled && !bridge.ModelReady, "Dead engine left generation enabled");
                Log("PASS motor encerrado: UI continua responsiva e geração desabilitada");
                pulse.Stop(); form.Close();
                Log("ALL PASS");
            }
        }
        catch (Exception ex) { Log("FAIL " + ex); throw; }
        finally { bridge.Stop(); engine.Dispose(); }
    }
}
