// Qualification panel: collapsed by default, fields, CPF check, clear with
// confirmation, themes, and proof that the data never reaches the model.
public static class QualificationTest
{
    private static string reportPath;
    private static int failures;

    private static class W
    {
        public delegate bool EnumProc(IntPtr window, IntPtr data);
        [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback, IntPtr data);
        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr window, out uint id);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetClassName(IntPtr h, StringBuilder s, int n);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
        [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] public static extern IntPtr GetDlgItem(IntPtr h, int id);
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
    }

    // Answers the next message box whose title contains `title` with `button`.
    public sealed class Expect { public string Title; public int Button; public int DefaultId; public string Text; public bool Done; }
    private static readonly object gate = new object();
    private static readonly List<Expect> queue = new List<Expect>();
    private static volatile bool watching;

    private static void Watch()
    {
        uint self = (uint)Process.GetCurrentProcess().Id;
        var seen = new Dictionary<IntPtr, DateTime>();
        while (watching)
        {
            var dialogs = new List<IntPtr>();
            W.EnumWindows(delegate (IntPtr h, IntPtr d)
            {
                uint pid; W.GetWindowThreadProcessId(h, out pid);
                if (pid != self || !W.IsWindowVisible(h)) return true;
                var c = new StringBuilder(64); W.GetClassName(h, c, 64);
                if (c.ToString() == "#32770") dialogs.Add(h);
                return true;
            }, IntPtr.Zero);
            foreach (IntPtr dlg in dialogs)
            {
                var t = new StringBuilder(256); W.GetWindowText(dlg, t, 256);
                Expect next = null;
                lock (gate) { foreach (Expect e in queue) if (!e.Done) { next = e; break; } }
                if (next == null || t.ToString().IndexOf(next.Title, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    if (!seen.ContainsKey(dlg)) seen[dlg] = DateTime.Now;
                    else if ((DateTime.Now - seen[dlg]).TotalSeconds > 5) W.PostMessage(dlg, 0x0010, IntPtr.Zero, IntPtr.Zero);
                    continue;
                }
                System.Threading.Thread.Sleep(300);
                var body = new StringBuilder(2048); W.GetWindowText(W.GetDlgItem(dlg, 0xFFFF), body, 2048);
                next.Text = body.ToString();
                next.DefaultId = W.SendMessage(dlg, 0x0400, IntPtr.Zero, IntPtr.Zero).ToInt32() & 0xFFFF;
                W.PostMessage(dlg, 0x0111, (IntPtr)next.Button, IntPtr.Zero);
                next.Done = true;
                System.Threading.Thread.Sleep(300);
            }
            System.Threading.Thread.Sleep(60);
        }
    }

    public static Expect Arm(string title, int button)
    {
        var e = new Expect(); e.Title = title; e.Button = button;
        lock (gate) { queue.Add(e); }
        return e;
    }

    private static void Log(string m) { Console.WriteLine(m); File.AppendAllText(reportPath, m + Environment.NewLine, Encoding.UTF8); }
    private static void Check(bool ok, string name, string detail)
    {
        if (!ok) failures++;
        Log((ok ? "PASS " : "FAIL ") + name + (String.IsNullOrEmpty(detail) ? "" : " | " + detail));
    }
    public static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }
    private static bool WaitFor(Func<bool> done, int ms) { var t = Stopwatch.StartNew(); while (!done()) { if (t.ElapsedMilliseconds > ms) return false; Application.DoEvents(); System.Threading.Thread.Sleep(20); } return true; }
    public static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
    public static object Call(object target, string name, params object[] args)
    {
        return target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, args);
    }
    private static Rectangle InForm(Form form, Control c) { return form.RectangleToClient(c.RectangleToScreen(c.ClientRectangle)); }

    private static bool ButtonsVisible(Form form, List<string> problems)
    {
        string[] names = { "selectModel", "loadModel", "reformulate", "cancel", "addButton", "copyButton", "saveButton", "exportButton", "clearButton", "logButton", "reviewToggle", "qualToggle", "qualClear" };
        foreach (string n in names)
        {
            var f = form.GetType().GetField(n, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (f == null) continue;
            Control b = (Control)f.GetValue(form);
            Rectangle r = InForm(form, b);
            if (!b.Visible || r.Width < 20 || !form.ClientRectangle.Contains(r)) problems.Add(n + " " + r);
        }
        return problems.Count == 0;
    }

    private static void SetField(Form form, int field, string value)
    {
        Control[] inputs = Field<Control[]>(form, "qualInputs");
        var combo = inputs[field] as ComboBox;
        if (combo != null) combo.SelectedIndex = Math.Max(0, combo.FindStringExact(value));
        else inputs[field].Text = value;
    }

    public static int Run(string root)
    {
        reportPath = Path.Combine(root, "maintenance", "qualification", "results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Application.EnableVisualStyles();
        ThemeChoice savedTheme = ThemeManager.LoadChoice();
        string realCopy = AutosaveStore.FilePath + ".antes-do-teste";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        watching = true;
        var watcher = new System.Threading.Thread(Watch); watcher.IsBackground = true; watcher.Start();
        Process engine = null;
        ModernDepoimentoForm form = null;
        try
        {
            // CPF check digits (unit level).
            Check(Qualification.CpfValid("529.982.247-25") && Qualification.CpfValid("52998224725") && Qualification.CpfValid("CPF: 529.982.247-25"), "3: CPF válido reconhecido (formatado, só dígitos, com a palavra CPF)", null);
            Check(!Qualification.CpfValid("123.456.789-00") && !Qualification.CpfValid("111.111.111-11") && !Qualification.CpfValid("529.982.247-2"), "3: CPF inválido detectado (dígito errado, sequência repetida, dígito faltando)", null);
            Check(!Qualification.LooksLikeCpf("12.345.678-9") && !Qualification.LooksLikeCpf("RG MG-12.345.678") && Qualification.LooksLikeCpf("CPF 123"), "3: RG não é tratado como CPF", null);

            ThemeManager.SaveChoice(ThemeChoice.Light);
            var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
            start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
            engine = Process.Start(start);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
            form.Show(); Pump(800);
            var card = Field<RoundedPanel>(form, "qualCard");
            var grid = Field<TableLayoutPanel>(form, "qualGrid");
            var summary = Field<Label>(form, "qualSummary");
            var toggle = Field<ModernButton>(form, "qualToggle");
            Control[] inputs = Field<Control[]>(form, "qualInputs");
            Label[] labels = Field<Label[]>(form, "qualLabels");
            float k = form.DeviceDpi / 96f;

            // Collapsed by default, small.
            Check(!grid.Visible && card.Height <= 60 * k && toggle.Text == "Mostrar campos", "3: painel começa fechado e ocupa pouco espaço", "altura=" + card.Height);
            DateTime shown;
            bool parsed = DateTime.TryParseExact(inputs[Qualification.DataHora].Text, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out shown);
            Check(parsed && Math.Abs((DateTime.Now - shown).TotalMinutes) < 3, "3: data e hora preenchidas com o momento atual", inputs[Qualification.DataHora].Text);
            Check(summary.Text.StartsWith("não preenchida"), "3: resumo indica painel não preenchido", summary.Text);

            // Expand, fields, layout.
            toggle.PerformClick(); Pump(300);
            Check(grid.Visible && card.Height > 180 * k && toggle.Text == "Ocultar campos", "3: «Mostrar campos» abre o painel", "altura=" + card.Height);
            bool allFields = true;
            for (int i = 0; i < Qualification.Count; i++) if (inputs[i] == null || !inputs[i].Visible || labels[i].Text.Length == 0) allFields = false;
            Check(allFields, "3: os 12 campos aparecem com rótulo (inclui Gênero do depoente)", null);
            var condition = (ComboBox)inputs[Qualification.Condicao];
            var options = new List<string>(); foreach (object o in condition.Items) options.Add(Convert.ToString(o));
            Check(String.Join("|", options.ToArray()) == "—|vítima|testemunha|investigado|declarante", "3: condição com vítima, testemunha, investigado, declarante", String.Join(", ", options.ToArray()));
            inputs[Qualification.DataHora].Text = "01/10/2026 09:15";
            Check(Field<Control[]>(form, "qualInputs")[Qualification.DataHora].Text == "01/10/2026 09:15", "3: data e hora editáveis", null);
            var problems = new List<string>();
            Check(ButtonsVisible(form, problems), "3: botões visíveis com o painel aberto (janela larga)", String.Join("; ", problems.ToArray()));
            var original = Field<SectionCard>(form, "originalCard"); var reform = Field<SectionCard>(form, "reformulatedCard");
            Check(InForm(form, original).Top == InForm(form, reform).Top && original.Height > 100, "3: lado a lado mantido com o painel aberto", "altura dos quadros=" + original.Height);
            Check(!Field<TableLayoutPanel>(form, "root").AutoScroll, "3: janela larga com o painel aberto não precisa rolar", null);
            var combinedBox = Field<SectionCard>(form, "combinedCard");
            foreach (Size size in new Size[] { new Size(1280, 820), new Size(1060, 860), new Size(1050, 720), new Size(1280, 720) })
            {
                form.Size = size; Pump(400);
                var rootPanel = Field<TableLayoutPanel>(form, "root");
                int smallest = Math.Min(original.Height, Math.Min(reform.Height, combinedBox.Height));
                Check(smallest >= 100 * k && (size.Width != 1280 || size.Height != 820 || !rootPanel.AutoScroll), "3: quadros de texto legíveis com o painel aberto (" + size.Width + "x" + size.Height + ")", "menor quadro=" + smallest + (rootPanel.AutoScroll ? ", com rolagem" : ", sem rolagem"));
                // Every button visible, or reachable by scrolling the window.
                problems.Clear();
                ButtonsVisible(form, problems);
                var missing = new List<string>(problems);
                if (missing.Count > 0 && rootPanel.AutoScroll)
                {
                    rootPanel.AutoScrollPosition = new Point(0, rootPanel.AutoScrollMinSize.Height); Pump(300);
                    problems.Clear(); ButtonsVisible(form, problems);
                    missing.RemoveAll(delegate (string m) { string n = m.Split(' ')[0]; return !problems.Exists(delegate (string p) { return p.Split(' ')[0] == n; }); });
                    rootPanel.AutoScrollPosition = Point.Empty; Pump(200);
                }
                Check(missing.Count == 0, "3: botões visíveis ou alcançáveis rolando (" + size.Width + "x" + size.Height + ")", String.Join("; ", missing.ToArray()));
            }
            form.Size = new Size(1280, 860); Pump(300);

            // CPF warning, without blocking.
            SetField(form, Qualification.Documento, "123.456.789-00"); Pump(100);
            Check(labels[Qualification.Documento].Text.Contains("CPF inválido") && summary.Text.Contains("CPF inválido"), "3: CPF inválido gera aviso no campo e no resumo", labels[Qualification.Documento].Text);
            SetField(form, Qualification.Documento, "529.982.247-25"); Pump(100);
            Check(!labels[Qualification.Documento].Text.Contains("CPF inválido") && !summary.Text.Contains("CPF inválido"), "3: CPF válido sem aviso", null);
            SetField(form, Qualification.Documento, "RG 12.345.678-9"); Pump(100);
            Check(!labels[Qualification.Documento].Text.Contains("CPF inválido"), "3: RG sem aviso", null);

            // Summary shows what identifies the testimony.
            SetField(form, Qualification.Procedimento, "123/2026");
            SetField(form, Qualification.Depoente, "Maria da Silva");
            SetField(form, Qualification.Condicao, "testemunha"); Pump(100);
            Check(summary.Text.Contains("Proc. 123/2026") && summary.Text.Contains("Maria da Silva") && summary.Text.Contains("testemunha"), "3: resumo do painel fechado mostra procedimento, nome e condição", summary.Text);

            // Themes.
            Field<ThemedComboBox>(form, "themePicker").SelectedIndex = 2; Pump(300);
            UiPalette dark = UiPalette.Create(true), light = UiPalette.Create(false);
            Check(inputs[Qualification.Procedimento].BackColor.ToArgb() == dark.Editor.ToArgb() && labels[Qualification.Unidade].ForeColor.ToArgb() == dark.Muted.ToArgb(), "3: campos seguem o tema escuro", null);
            Field<ThemedComboBox>(form, "themePicker").SelectedIndex = 1; Pump(300);
            Check(inputs[Qualification.Procedimento].BackColor.ToArgb() == light.Editor.ToArgb(), "3: campos seguem o tema claro", null);

            // Clear with confirmation, «Não» as default.
            Expect keep = Arm("Limpar campos", 7);
            Call(form, "QualClear_Click", null, EventArgs.Empty); Pump(300);
            Check(keep.Done && keep.DefaultId == 7 && inputs[Qualification.Depoente].Text == "Maria da Silva", "3: «Limpar campos» pede confirmação, «Não» é o padrão e mantém os dados", "padrão=" + keep.DefaultId);
            Expect clear = Arm("Limpar campos", 6);
            Call(form, "QualClear_Click", null, EventArgs.Empty); Pump(300);
            bool empty = true;
            for (int i = 0; i < Qualification.Count; i++) if (i != Qualification.DataHora && Field<Control[]>(form, "qualInputs")[i] is TextBox && inputs[i].Text.Length > 0) empty = false;
            Check(clear.Done && empty && condition.SelectedIndex == 0 && inputs[Qualification.DataHora].Text != "01/10/2026 09:15", "3: «Sim» limpa os campos e renova data e hora", inputs[Qualification.DataHora].Text);
            toggle.PerformClick(); Pump(300);
            Check(!grid.Visible && card.Height <= 60 * k && !Field<TableLayoutPanel>(form, "root").AutoScroll, "3: «Ocultar campos» fecha o painel (sem rolagem)", null);

            // Never sent to the model: a real generation with unique markers in every field.
            string[] marks = { "PROC-X91", "UNID-X92", "LOCAL-X93", "30/09/2026 10:00", "NOME-X95", "529.982.247-25", "testemunha", "END-X97", "TEL-X98", "AUTO-X99", "ESCR-X90", "Masculino" };
            for (int i = 0; i < Qualification.Count; i++) SetField(form, i, marks[i]);
            string input = "Eu cheguei ao prédio às nove horas e esperei na recepção.";
            original.Editor.Text = input;
            SectionCard combinedCard = Field<SectionCard>(form, "combinedCard");
            bridge.ModelPath = @"..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf";
            Field<TextBox>(form, "modelPath").Text = bridge.ModelPath;
            Call(form, "LoadModel_Click", null, EventArgs.Empty);
            bool loaded = WaitFor(delegate { return bridge.ModelReady; }, 180000);
            Pump(800);
            Call(form, "Reformulate_Click", null, EventArgs.Empty);
            bool finished = WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 300000);
            Pump(1000);
            string sentOriginal = bridge.OriginalText, sentOut = bridge.ReformulatedText, sentCombined = bridge.CombinedText;
            bool leaked = false; string leak = "";
            foreach (string m in new string[] { "PROC-X91", "UNID-X92", "LOCAL-X93", "NOME-X95", "529.982.247-25", "END-X97", "TEL-X98", "AUTO-X99", "ESCR-X90" })
                if (sentOriginal.Contains(m) || sentOut.Contains(m) || sentCombined.Contains(m)) { leaked = true; leak += m + " "; }
            Check(loaded && finished && sentOriginal == input && sentOut.Length > 0 && !leaked, "3: qualificação nunca vai para o modelo (geração real)", "motor recebeu: «" + sentOriginal + "»; saída: «" + sentOut + "»" + (leaked ? "; VAZOU: " + leak : ""));
        }
        catch (Exception ex) { failures++; Log("FAIL execução: " + ex); }
        finally
        {
            watching = false;
            if (form != null) { try { Field<SectionCard>(form, "combinedCard").Editor.Text = ""; Call(form, "ApplyQualification", new Qualification()); form.Close(); form.Dispose(); } catch { } }
            try { if (engine != null && !engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(savedTheme);
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
            Log(failures == 0 ? "RESULT PASS" : "RESULT FAIL (" + failures + ")");
        }
        return failures;
    }
}
