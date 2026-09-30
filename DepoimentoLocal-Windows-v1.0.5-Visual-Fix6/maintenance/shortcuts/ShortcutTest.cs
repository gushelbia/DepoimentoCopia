// Keyboard shortcuts: Ctrl+Enter, Esc, Ctrl+S, Ctrl+O, Ctrl+E, Ctrl+D; native
// RichEdit formatting shortcuts blocked in the editors; tooltips. Keys are
// simulated on this test thread only (no global input injection), through the
// same path as the message loop: PreProcessMessage, then WM_KEYDOWN and WM_CHAR.
public static class ShortcutTest
{
    private static string reportPath;
    private static int failures;

    [DllImport("user32.dll")] private static extern bool GetKeyboardState(byte[] state);
    [DllImport("user32.dll")] private static extern bool SetKeyboardState(byte[] state);
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, IntPtr l);

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

    // Presses a key on `target`, with Ctrl/Shift held on this thread. Returns true
    // when a command handler (ProcessCmdKey) took the key before the control.
    public static bool Key(Control target, Keys key, bool ctrl, bool shift)
    {
        target.Focus();
        byte[] previous = new byte[256];
        GetKeyboardState(previous);
        byte[] pressed = (byte[])previous.Clone();
        if (ctrl) { pressed[0x11] = 0x80; pressed[0xA2] = 0x80; }
        if (shift) { pressed[0x10] = 0x80; pressed[0xA0] = 0x80; }
        bool taken;
        try
        {
            SetKeyboardState(pressed);
            int vk = (int)key;
            Message message = Message.Create(target.Handle, 0x0100, new IntPtr(vk), new IntPtr(1));
            taken = target.PreProcessMessage(ref message);
            if (!taken)
            {
                SendMessage(target.Handle, 0x0100, new IntPtr(vk), new IntPtr(1));
                // What TranslateMessage would produce for Ctrl+letter (control character) or Enter/Esc.
                int ch = 0;
                if (key >= Keys.A && key <= Keys.Z) ch = ctrl ? vk - 0x40 : vk + (shift ? 0 : 0x20);
                else if (key == Keys.Enter) ch = ctrl ? 10 : 13;
                else if (key == Keys.Escape) ch = 27;
                if (ch != 0) SendMessage(target.Handle, 0x0102, new IntPtr(ch), new IntPtr(1));
            }
            SendMessage(target.Handle, 0x0101, new IntPtr(vk), new IntPtr(unchecked((int)0xC0000001)));
        }
        finally { SetKeyboardState(previous); }
        Application.DoEvents();
        return taken;
    }

    private static Process StartEngine(string root)
    {
        var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
        start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
        return Process.Start(start);
    }

    public static int Run(string root, string work)
    {
        reportPath = Path.Combine(root, "maintenance", "shortcuts", "results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Directory.CreateDirectory(work);
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
            // The simulation itself: on a plain RichTextBox, Ctrl+R does right-align.
            using (var probeForm = new Form())
            {
                probeForm.StartPosition = FormStartPosition.Manual; probeForm.Location = new Point(-4000, -4000);
                var plain = new RichTextBox(); plain.Dock = DockStyle.Fill; probeForm.Controls.Add(plain);
                probeForm.Show(); plain.Text = "linha"; plain.SelectAll(); Pump(100);
                Key(plain, Keys.R, true, false); Pump(50);
                Check(plain.SelectionAlignment == HorizontalAlignment.Right, "controle: sem bloqueio, Ctrl+R alinha à direita num RichTextBox comum (a simulação funciona)", plain.SelectionAlignment.ToString());
            }

            engine = StartEngine(root);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
            form.Show(); Pump(800);
            var original = Field<SectionCard>(form, "originalCard");
            var reform = Field<SectionCard>(form, "reformulatedCard");
            var combined = Field<SectionCard>(form, "combinedCard");
            var status = Field<Label>(form, "status");
            var tip = Field<ToolTip>(form, "reviewTip");

            // Tooltips show the shortcut.
            string[,] tips = { { "reformulate", "Ctrl+Enter" }, { "cancel", "Esc" }, { "saveButton", "Ctrl+S" }, { "openButton", "Ctrl+O" }, { "exportButton", "Ctrl+E" }, { "reviewToggle", "Ctrl+D" } };
            var tipText = new List<string>(); bool tipsOk = true;
            for (int i = 0; i < tips.GetLength(0); i++)
            {
                string t = tip.GetToolTip(Field<Control>(form, tips[i, 0]));
                tipText.Add(t);
                if (!t.Contains("(" + tips[i, 1] + ")")) tipsOk = false;
            }
            Check(tipsOk, "dica de cada botão mostra o atalho", String.Join(" / ", tipText.ToArray()));

            // Formatting shortcuts do nothing in the three editors (text and formatting unchanged).
            combined.Editor.Text = "Texto consolidado.";
            Keys[] plainKeys = { Keys.R, Keys.L, Keys.J, Keys.D1, Keys.D2, Keys.D5, Keys.NumPad1, Keys.NumPad2, Keys.NumPad5, Keys.Oemplus };
            Keys[] shiftKeys = { Keys.L, Keys.Oemplus, Keys.Oemcomma, Keys.OemPeriod };
            string[] cardNames = { "original", "reformulado", "consolidado" }; int cardIndex = 0;
            foreach (SectionCard card in new SectionCard[] { original, reform, combined })
            {
                RichTextBox ed = card.Editor;
                ed.Text = "Primeira linha do texto.\nSegunda linha.";
                ed.SelectAll(); Pump(50);
                string rtf = ed.Rtf, text = ed.Text;
                var changed = new List<string>();
                foreach (Keys k in plainKeys) { ed.SelectAll(); Key(ed, k, true, false); if (ed.Rtf != rtf || ed.Text != text) { changed.Add("Ctrl+" + k); ed.Text = text; rtf = ed.Rtf; } }
                foreach (Keys k in shiftKeys) { ed.SelectAll(); Key(ed, k, true, true); if (ed.Rtf != rtf || ed.Text != text) { changed.Add("Ctrl+Shift+" + k); ed.Text = text; rtf = ed.Rtf; } }
                // Ctrl+E is «Exportar Word» now: cancel the dialog, formatting untouched.
                DialogKit.ArmFile("Salvar", null);
                ed.SelectAll(); bool taken = Key(ed, Keys.E, true, false); Pump(600);
                if (ed.Rtf != rtf || ed.Text != text) changed.Add("Ctrl+E");
                ed.SelectAll();
                Check(changed.Count == 0 && taken && ed.SelectionAlignment == HorizontalAlignment.Left,
                    "Ctrl+E/R/L/J e demais atalhos de formatação não mudam o texto nem o alinhamento (" + cardNames[cardIndex++] + ")", String.Join(", ", changed.ToArray()));
            }
            original.Editor.Text = ""; reform.Editor.Text = "";

            // Ctrl+S / Ctrl+O / Ctrl+E from an editor and from a qualification field.
            Call(form, "ApplyQualification", QualSample());
            combined.Editor.Text = "Texto consolidado para os atalhos.";
            string draft = Path.Combine(work, "atalho-rascunho.txt");
            DialogKit.Expect save = DialogKit.ArmFile("Salvar", draft);
            Key(combined.Editor, Keys.S, true, false); Pump(500);
            Check(save.Done && File.Exists(draft) && File.ReadAllText(draft, Encoding.UTF8).Contains("Texto consolidado para os atalhos.") && File.ReadAllText(draft, Encoding.UTF8).Contains("depoente: Nome Atalho"),
                "Ctrl+S salva o rascunho (texto e campos)", null);
            Check(combined.Editor.Text == "Texto consolidado para os atalhos.", "Ctrl+S não altera o texto do editor", null);

            combined.Editor.Text = "Outro texto.";
            DialogKit.ArmFile("Abrir rascunho", draft);
            DialogKit.Arm("Abrir rascunho", 6);
            Control field = Field<Control[]>(form, "qualInputs")[Qualification.Local];
            Call(form, "SetQualExpanded", true); Pump(200);
            Key(field, Keys.O, true, false);
            WaitFor(delegate { return combined.Editor.Text == "Texto consolidado para os atalhos."; }, 4000); Pump(300);
            Check(combined.Editor.Text == "Texto consolidado para os atalhos." && status.Text.StartsWith("Rascunho aberto"), "Ctrl+O abre o rascunho (também com o foco num campo de qualificação)", status.Text);
            Call(form, "SetQualExpanded", false); Pump(200);

            string docx = Path.Combine(work, "atalho.docx");
            DialogKit.Expect export = DialogKit.ArmFile("Salvar", docx);
            Key(original.Editor, Keys.E, true, false); Pump(500);
            Check(export.Done && File.Exists(docx) && WordHas(docx, "Texto consolidado para os atalhos.") && WordHas(docx, "Nome Atalho"), "Ctrl+E exporta o Word", null);

            // Ctrl+D toggles the highlights.
            var toggle = Field<ModernButton>(form, "reviewToggle");
            Key(original.Editor, Keys.D, true, false); Pump(200);
            bool off = !Field<bool>(form, "highlightsEnabled") && toggle.Text == "Destaques: desligados";
            Key(reform.Editor, Keys.D, true, false); Pump(200);
            bool on = Field<bool>(form, "highlightsEnabled") && toggle.Text == "Destaques: ligados";
            Check(off && on, "Ctrl+D liga e desliga os destaques", toggle.Text);

            // Esc outside a generation: nothing; Ctrl+Enter without a model: only the notice, no line break.
            original.Editor.Text = "Texto de teste.";
            original.Editor.SelectionStart = original.Editor.TextLength;
            bool escTaken = Key(original.Editor, Keys.Escape, false, false); Pump(200);
            Check(!escTaken && !Field<bool>(form, "cancelRequested") && original.Editor.Text == "Texto de teste.", "Esc sem geração em andamento não faz nada", null);
            Key(original.Editor, Keys.Enter, true, false); Pump(300);
            Check(original.Editor.Text == "Texto de teste." && status.Text.StartsWith("Carregue o modelo") && !Field<bool>(form, "generationWasRunning"),
                "Ctrl+Enter sem modelo só avisa e não quebra linha", status.Text);
            Key(original.Editor, Keys.Enter, false, false); Pump(100);
            Check(original.Editor.Text == "Texto de teste.\n", "Enter sozinho continua quebrando linha", null);

            // With the model: Ctrl+Enter starts, Esc cancels, then Ctrl+Enter again completes.
            bridge.ModelPath = @"..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf";
            Field<TextBox>(form, "modelPath").Text = bridge.ModelPath;
            Call(form, "LoadModel_Click", null, EventArgs.Empty);
            bool loaded = WaitFor(delegate { return bridge.ModelReady; }, 180000); Pump(800);
            string longText = "Eu cheguei no prédio umas nove horas e fiquei esperando na recepção. Depois a moça da recepção falou que o gerente ia demorar, " +
                "aí eu saí pra tomar um café na padaria da esquina e voltei umas dez e meia. Quando voltei o gerente já tinha saído e ninguém sabia pra onde. " +
                "Aí eu liguei pra ele umas três vezes e ele não atendeu, então eu fui embora e só no outro dia ele me retornou dizendo que estava numa reunião.";
            original.Editor.Text = longText;
            Key(original.Editor, Keys.Enter, true, false);
            bool started = WaitFor(delegate { return Field<bool>(form, "generationWasRunning"); }, 3000);
            Check(loaded && started && original.Editor.Text == longText, "Ctrl+Enter reformula (sem quebrar linha no texto original)", null);
            WaitFor(delegate { return Field<ModernButton>(form, "cancel").Enabled; }, 15000); Pump(300);
            bool escDuring = Key(original.Editor, Keys.Escape, false, false);
            bool stopped = WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 60000); Pump(500);
            Check(escDuring && stopped && status.Text.Contains("cancelada"), "Esc cancela a geração em andamento", status.Text);
            original.Editor.Text = "Eu cheguei às nove horas.";
            Key(combined.Editor, Keys.Enter, true, false);
            bool again = WaitFor(delegate { return Field<bool>(form, "generationWasRunning"); }, 3000);
            bool done = WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 240000); Pump(800);
            Check(again && done && reform.Editor.Text.Trim().Length > 0 && combined.Editor.Text == "Texto consolidado para os atalhos.", "depois de cancelar, Ctrl+Enter gera normalmente (foco no consolidado, que não é alterado)", "saída: «" + reform.Editor.Text.Trim() + "»");
            Check(DialogKit.Unexpected.Count == 0, "nenhuma janela inesperada", String.Join(", ", DialogKit.Unexpected.ToArray()));
        }
        catch (Exception ex) { failures++; Log("FAIL execução: " + ex); }
        finally
        {
            DialogKit.Stop();
            if (form != null)
            {
                try
                {
                    Field<SectionCard>(form, "combinedCard").Editor.Text = "";
                    Call(form, "ApplyQualification", new Qualification());
                    form.Close(); form.Dispose();
                }
                catch { }
            }
            try { if (engine != null && !engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(savedTheme);
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
            Log(failures == 0 ? "RESULT PASS" : "RESULT FAIL (" + failures + ")");
        }
        return failures;
    }

    private static Qualification QualSample()
    {
        var q = new Qualification();
        q[Qualification.Depoente] = "Nome Atalho";
        q[Qualification.DataHora] = "30/09/2026 15:00";
        return q;
    }

    private static bool WordHas(string path, string text)
    {
        using (ZipArchive zip = ZipFile.OpenRead(path))
        using (var reader = new StreamReader(zip.GetEntry("word/document.xml").Open(), Encoding.UTF8))
            return reader.ReadToEnd().Contains(text);
    }
}
