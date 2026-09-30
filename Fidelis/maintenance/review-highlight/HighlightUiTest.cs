// UI test: side-by-side layout and review highlights on the real interface
// (real engine, no model). Highlights must stay visual: never in the clipboard,
// the consolidated text, the Word export or the engine.
public static class HighlightUiTest
{
    private static string reportPath;
    private static int failures;

    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, IntPtr l);
    [DllImport("user32.dll", EntryPoint = "SendMessage")] private static extern IntPtr SendPoint(IntPtr h, int msg, IntPtr w, ref Point l);
    [DllImport("user32.dll")] private static extern bool GetKeyboardState(byte[] state);
    [DllImport("user32.dll")] private static extern bool SetKeyboardState(byte[] state);
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);

    public const string Original =
        "Eu cheguei ao estacionamento no dia 12/08/2026, por volta das 14h30. O motorista Celso estacionou o carro de placa BRA2E19 perto da cancela. " +
        "Paguei R$ 25,00 em dinheiro. Depois o segurança anotou meu CPF 123.456.789-09 e meu telefone (11) 91234-5678. Saí às 15h15.";
    public const string Reformulated =
        "Relatou que o depoente chegou ao estacionamento no dia 12 de agosto de 2026, por volta das 14:30. O motorista Celso estacionou o carro de placa BRA2E91 perto da cancela. " +
        "O depoente pagou vinte e cinco reais em dinheiro. Depois o segurança anotou o CPF 123.456.789-09 do depoente e o telefone (11) 91234-5678 do depoente. O depoente saiu às 15h.";

    private static void Log(string m) { Console.WriteLine(m); File.AppendAllText(reportPath, m + Environment.NewLine, Encoding.UTF8); }
    private static void Check(bool ok, string name, string detail)
    {
        if (!ok) failures++;
        Log((ok ? "PASS " : "FAIL ") + name + (String.IsNullOrEmpty(detail) ? "" : " | " + detail));
    }
    private static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }
    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
    private static void SetField(object target, string name, object value)
    {
        target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(target, value);
    }
    private static void Call(object target, string name, params object[] args)
    {
        target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, args);
    }

    private static int BackAt(RichTextBox editor, int index)
    {
        int s = editor.SelectionStart, l = editor.SelectionLength;
        editor.Select(index, 1);
        Color c = editor.SelectionBackColor;
        editor.Select(s, l);
        return c.ToArgb();
    }

    private static void Key(RichTextBox editor, int virtualKey)
    {
        editor.Focus();
        byte[] previous = new byte[256];
        GetKeyboardState(previous);
        byte[] pressed = (byte[])previous.Clone();
        pressed[0x11] = 0x80; // Ctrl on this test thread only
        try
        {
            SetKeyboardState(pressed);
            Message message = Message.Create(editor.Handle, 0x0100, new IntPtr(virtualKey), new IntPtr(1));
            if (!editor.PreProcessMessage(ref message)) SendMessage(editor.Handle, message.Msg, message.WParam, message.LParam);
            SendMessage(editor.Handle, 0x0101, new IntPtr(virtualKey), new IntPtr(0xC0000001L));
        }
        finally { SetKeyboardState(previous); }
        Application.DoEvents();
    }

    private static bool ClipboardIsPlain()
    {
        IDataObject data = Clipboard.GetDataObject();
        return data != null && data.GetDataPresent(DataFormats.UnicodeText) && !data.GetDataPresent(DataFormats.Rtf);
    }

    private static Rectangle InForm(Form form, Control c) { return form.RectangleToClient(c.RectangleToScreen(c.ClientRectangle)); }

    // Summary never truncated: no ellipsis and the text fits in the label.
    private static bool SummaryFits(Label review)
    {
        int line = TextRenderer.MeasureText("Ág", review.Font).Height;
        int needed = TextRenderer.MeasureText(review.Text, review.Font, new Size(review.Width, 10000), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
        return !review.AutoEllipsis && review.Text.IndexOf('…') < 0 && needed <= review.Height && needed <= line * 2 + 2;
    }

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    private delegate bool EnumProc(IntPtr h, IntPtr d);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] private static extern IntPtr GetDlgItem(IntPtr h, int id);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);

    // Clicks the summary label; a background thread reads the list dialog and closes it.
    private static string ClickSummary(Label review)
    {
        string seen = null;
        uint self = (uint)Process.GetCurrentProcess().Id;
        var watcher = new System.Threading.Thread(delegate ()
        {
            var t = Stopwatch.StartNew();
            while (seen == null && t.ElapsedMilliseconds < 8000)
            {
                EnumWindows(delegate (IntPtr h, IntPtr d)
                {
                    uint pid; GetWindowThreadProcessId(h, out pid);
                    if (pid != self) return true;
                    // Class first: GetWindowText on a window of a thread that is not
                    // pumping messages would block this watcher.
                    var cls = new StringBuilder(64); GetClassName(h, cls, 64);
                    if (cls.ToString() != "#32770") return true;
                    var title = new StringBuilder(128); GetWindowText(h, title, 128);
                    if (title.ToString() == "Itens para conferir")
                    {
                        System.Threading.Thread.Sleep(300); // let the dialog finish initializing
                        var body = new StringBuilder(8192); GetWindowText(GetDlgItem(h, 0xFFFF), body, 8192);
                        seen = body.ToString();
                        // An OK-only message box ignores WM_COMMAND IDOK here; WM_CLOSE closes it.
                        PostMessage(h, 0x0010, IntPtr.Zero, IntPtr.Zero);
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);
                System.Threading.Thread.Sleep(100);
            }
        });
        watcher.IsBackground = true; watcher.Start();
        typeof(Control).GetMethod("OnClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(review, new object[] { EventArgs.Empty });
        watcher.Join(9000);
        return seen;
    }

    private static void Shot(Form form, string path)
    {
        form.TopMost = true; form.Location = new Point(0, 0); form.Activate(); Pump(900);
        using (var bmp = new Bitmap(form.Width, form.Height))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr dc = g.GetHdc();
                PrintWindow(form.Handle, dc, 2);
                g.ReleaseHdc(dc);
            }
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
        form.TopMost = false; form.Location = new Point(-4000, -4000); Pump(200);
        Log("INFO print: " + path);
    }

    private static bool ButtonsVisible(Form form, List<string> problems)
    {
        string[] names = { "selectModel", "loadModel", "reformulate", "cancel", "addButton", "copyButton", "saveButton", "exportButton", "clearButton", "logButton", "reviewToggle" };
        Rectangle client = form.ClientRectangle;
        foreach (string n in names)
        {
            ModernButton b = Field<ModernButton>(form, n);
            Rectangle r = InForm(form, b);
            if (!b.Visible || r.Width < 20 || !client.Contains(r)) problems.Add(n + " " + r);
        }
        foreach (string card in new string[] { "originalCard", "reformulatedCard" })
        {
            ModernButton clear = Field<SectionCard>(form, card).ClearButton;
            Rectangle r = InForm(form, clear);
            if (!clear.Visible || !client.Contains(r)) problems.Add(card + ".Limpar " + r);
        }
        return problems.Count == 0;
    }

    public static int Run(string root, string workDir)
    {
        string dir = Path.Combine(root, "maintenance", "review-highlight");
        reportPath = Path.Combine(dir, "ui-results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Application.EnableVisualStyles();
        ThemeChoice savedTheme = ThemeManager.LoadChoice();
        string realCopy = AutosaveStore.FilePath + ".antes-do-teste";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        IDataObject previousClipboard = null;
        try { previousClipboard = Clipboard.GetDataObject(); } catch { }
        Process engine = null;
        ModernDepoimentoForm form = null;
        try
        {
            ThemeManager.SaveChoice(ThemeChoice.Light);
            var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
            start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
            engine = Process.Start(start);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, Path.Combine(root, "ui", "DepoimentoLocal.ico"), "Vulkan • 8 camadas GPU");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000);
            form.Size = new Size(1280, 860);
            form.Show(); Pump(800);
            var original = Field<SectionCard>(form, "originalCard");
            var reform = Field<SectionCard>(form, "reformulatedCard");
            var combined = Field<SectionCard>(form, "combinedCard");
            var generation = Field<Panel>(form, "generation");
            var review = Field<Label>(form, "reviewStatus");
            UiPalette light = UiPalette.Create(false), dark = UiPalette.Create(true);

            // 1) Layout: side by side when wide, stacked when narrow, and back.
            Rectangle o = InForm(form, original), r = InForm(form, reform), g = InForm(form, generation);
            Check(o.Top == r.Top && r.Left >= o.Right && Math.Abs(o.Width - r.Width) <= 2 && g.Top > o.Bottom, "1: janela larga (1280) — original e reformulado lado a lado, barra abaixo", "original=" + o + " reformulado=" + r);
            var problems = new List<string>();
            Check(ButtonsVisible(form, problems), "1: todos os botões visíveis e dentro da janela (lado a lado)", String.Join("; ", problems.ToArray()));
            form.Size = new Size(1060, 860); Pump(400);
            o = InForm(form, original); r = InForm(form, reform); g = InForm(form, generation);
            Check(o.Top < g.Top && g.Top < r.Top && o.Left == r.Left && o.Width == r.Width, "1: janela estreita (1060) — volta ao layout empilhado", "original=" + o + " barra=" + g + " reformulado=" + r);
            problems.Clear();
            Check(ButtonsVisible(form, problems), "1: todos os botões visíveis e dentro da janela (empilhado)", String.Join("; ", problems.ToArray()));
            form.Size = new Size(1280, 860); Pump(400);
            o = InForm(form, original); r = InForm(form, reform);
            Check(o.Top == r.Top && r.Left > o.Left, "1: alargar de novo volta a lado a lado", null);

            // 2) Highlights after the texts are in place (debounce ~0,6 s).
            original.Editor.Text = Original;
            reform.Editor.Text = Reformulated;
            Pump(1200);
            int plate = Reformulated.IndexOf("BRA2E91"), date = Reformulated.IndexOf("12 de agosto"), plain = Reformulated.IndexOf("estacionamento");
            int missing = Original.IndexOf("15h15"), name = Reformulated.IndexOf("Celso");
            Check(BackAt(reform.Editor, date) == light.HighlightItem.ToArgb(), "2: item conferido destacado (data equivalente: 12 de agosto = 12/08/2026)", null);
            Check(BackAt(reform.Editor, plate) == light.HighlightMismatch.ToArgb(), "2: placa alterada pelo modelo em cor forte", null);
            Check(BackAt(reform.Editor, plain) != light.HighlightItem.ToArgb() && BackAt(reform.Editor, plain) != light.HighlightMismatch.ToArgb() && BackAt(reform.Editor, name) != light.HighlightItem.ToArgb(), "2: texto comum e nome próprio sem destaque", null);
            Check(BackAt(original.Editor, missing) == light.HighlightMismatch.ToArgb() && BackAt(original.Editor, Original.IndexOf("12/08/2026")) == light.HighlightItem.ToArgb(), "2: original destacado; item que sumiu em cor forte", null);
            // Approximate time: the whole expression in the alert color, never "bate".
            int approxRef = Reformulated.IndexOf("por volta das 14:30"), approxOrig = Original.IndexOf("por volta das 14h30");
            Check(BackAt(reform.Editor, approxRef) == light.HighlightAlert.ToArgb() && BackAt(reform.Editor, approxRef + 16) == light.HighlightAlert.ToArgb()
                && BackAt(original.Editor, approxOrig) == light.HighlightAlert.ToArgb(), "ajuste 1: «por volta das 14h30» inteiro em cor de alerta, nos dois textos", null);
            Check(review.Visible && review.Text == "7 itens para conferir, 2 não encontrados no original, 1 aproximado ou ambíguo; sumiram do reformulado: BRA2E19, 15h15.", "2: resumo na barra de status", review.Text);
            Check(SummaryFits(review), "ajuste 2: resumo sem «…», em até duas linhas (janela larga)", "\"" + review.Text + "\"");
            string details = ClickSummary(review);
            Check(details != null && details.Contains("BRA2E91") && details.Contains("BRA2E19") && details.Contains("15h15") && details.Contains("por volta das 14:30") && details.Contains("Conferidos nos dois textos"),
                "ajuste 2: clique no resumo mostra a lista completa", details == null ? "janela não apareceu" : details.Replace("\r\n", " | ").Replace("\n", " | "));

            Shot(form, Path.Combine(dir, "print-claro.png"));
            Field<ThemedComboBox>(form, "themePicker").SelectedIndex = 2; Pump(400);
            Check(BackAt(reform.Editor, date) == dark.HighlightItem.ToArgb() && BackAt(reform.Editor, plate) == dark.HighlightMismatch.ToArgb(), "2: tema escuro repinta com as cores escuras", null);
            Shot(form, Path.Combine(dir, "print-escuro.png"));
            Field<ThemedComboBox>(form, "themePicker").SelectedIndex = 1; Pump(400);
            Check(BackAt(reform.Editor, date) == light.HighlightItem.ToArgb(), "2: volta ao tema claro com as cores claras", null);
            form.Size = new Size(1060, 860); Pump(400);
            Check(review.Visible && SummaryFits(review), "ajuste 2: resumo sem «…», em até duas linhas (janela estreita)", "\"" + review.Text + "\"");
            Shot(form, Path.Combine(dir, "print-empilhado.png"));
            form.Size = new Size(1280, 860); Pump(400);

            // 2) Toggle.
            Call(form, "ReviewToggle_Click", null, EventArgs.Empty); Pump(200);
            ModernButton toggle = Field<ModernButton>(form, "reviewToggle");
            Check(BackAt(reform.Editor, plate) != light.HighlightMismatch.ToArgb() && !review.Visible && toggle.Text == "Destaques: desligados", "2: botão desliga destaques e resumo", toggle.Text);
            Call(form, "ReviewToggle_Click", null, EventArgs.Empty); Pump(200);
            Check(BackAt(reform.Editor, plate) == light.HighlightMismatch.ToArgb() && review.Visible && toggle.Text == "Destaques: ligados", "2: botão religa", null);

            // 2) Not during generation; re-applied after an edit, with a delay.
            SetField(form, "generationWasRunning", true);
            Call(form, "ApplyHighlights");
            Check(BackAt(reform.Editor, plate) != light.HighlightMismatch.ToArgb() && !review.Visible, "2: sem destaques durante a geração", null);
            SetField(form, "generationWasRunning", false);
            Call(form, "ApplyHighlights");
            original.Editor.Text = Original.Replace("BRA2E19", "BRA2E91");
            Pump(200);
            bool notYet = BackAt(original.Editor, Original.IndexOf("BRA2E19")) != light.HighlightItem.ToArgb();
            Pump(900);
            Check(notYet && BackAt(original.Editor, Original.IndexOf("BRA2E19")) == light.HighlightItem.ToArgb() && BackAt(reform.Editor, plate) == light.HighlightItem.ToArgb(),
                "2: edição reaplica os destaques só após um pequeno atraso", null);
            original.Editor.Text = Original; Pump(900);

            // 2) Undo: Ctrl+Z undoes typing, not a highlight.
            original.Editor.Focus();
            original.Editor.Select(original.Editor.TextLength, 0);
            foreach (char ch in " Voltei às 16h.") SendMessage(original.Editor.Handle, 0x0102, (IntPtr)ch, IntPtr.Zero);
            Pump(900);
            bool painted = BackAt(original.Editor, original.Editor.Text.IndexOf("16h")) == light.HighlightMismatch.ToArgb();
            original.Editor.Undo(); Pump(200);
            Check(painted && original.Editor.Text == Original, "2: Ctrl+Z desfaz a digitação, não o destaque", "texto após desfazer termina em: …" + original.Editor.Text.Substring(Math.Max(0, original.Editor.TextLength - 25)));
            Pump(900);

            // 2) Long text: no freeze, caret and scroll preserved.
            var sb = new StringBuilder();
            for (int i = 0; i < 120; i++) sb.Append(Original).Append("\n");
            string longText = sb.ToString();
            original.Editor.Text = longText;
            reform.Editor.Text = longText.Replace("BRA2E19", "BRA2E91");
            Pump(100);
            original.Editor.Select(longText.Length / 2, 7);
            original.Editor.ScrollToCaret(); Pump(100);
            Point before = Point.Empty; SendPoint(original.Editor.Handle, 0x04DD, IntPtr.Zero, ref before);
            int selStart = original.Editor.SelectionStart, selLen = original.Editor.SelectionLength;
            var timer = Stopwatch.StartNew();
            Call(form, "ApplyHighlights");
            long ms = timer.ElapsedMilliseconds;
            Point after = Point.Empty; SendPoint(original.Editor.Handle, 0x04DD, IntPtr.Zero, ref after);
            Check(ms < 1500, "2: texto longo (" + longText.Length + " caracteres por quadro) sem travar", ms + " ms");
            Check(original.Editor.SelectionStart == selStart && original.Editor.SelectionLength == selLen && before == after, "2: cursor, seleção e rolagem preservados", "rolagem antes=" + before + " depois=" + after);
            original.Editor.Text = Original; reform.Editor.Text = Reformulated; Pump(900);

            // 2) Only visual: clipboard, consolidated, Word and engine get plain text.
            reform.Editor.Select(0, reform.Editor.TextLength);
            Clipboard.Clear();
            Key(reform.Editor, 0x43);
            Check(ClipboardIsPlain() && Clipboard.GetText() == Reformulated, "2: Ctrl+C copia só texto (sem RTF/cores)", null);
            Clipboard.Clear();
            foreach (ToolStripItem item in reform.Editor.ContextMenuStrip.Items) if (item.Text == "Copiar") item.PerformClick();
            Check(ClipboardIsPlain() && Clipboard.GetText() == Reformulated, "2: menu «Copiar» copia só texto", null);
            original.Editor.Select(Original.IndexOf("R$ 25,00"), 8);
            Clipboard.Clear();
            Key(original.Editor, 0x58);
            Check(ClipboardIsPlain() && Clipboard.GetText() == "R$ 25,00" && !original.Editor.Text.Contains("R$ 25,00"), "2: Ctrl+X recorta só texto", null);
            original.Editor.Text = Original; Pump(900);
            Call(form, "Add_Click", null, EventArgs.Empty); Pump(400);
            string rtf = combined.Editor.Rtf;
            Check(combined.Editor.Text == Reformulated && !rtf.Contains("\\highlight") && !rtf.Contains("\\cb"), "2: «Adicionar» leva só texto ao consolidado", null);
            string docx = Path.Combine(workDir, "teste.docx");
            SimpleDocx.Write(docx, combined.Editor.Text.Trim());
            string xml;
            using (var zip = ZipFile.OpenRead(docx))
            using (var reader = new StreamReader(zip.GetEntry("word/document.xml").Open(), Encoding.UTF8)) xml = reader.ReadToEnd();
            Check(!xml.Contains("highlight") && !xml.Contains("w:shd") && xml.Contains("BRA2E91"), "2: exportação Word sem cores", null);
            bridge.OriginalText = original.Editor.Text;
            Check(bridge.OriginalText == original.Editor.Text && !bridge.OriginalText.Contains("\\rtf"), "2: motor recebe só o texto", null);
        }
        catch (Exception ex) { failures++; Log("FAIL execução: " + ex); }
        finally
        {
            if (form != null)
            {
                try { Field<SectionCard>(form, "combinedCard").Editor.Text = ""; form.Close(); form.Dispose(); } catch { }
            }
            try { if (engine != null && !engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(savedTheme);
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
            try { if (previousClipboard != null) Clipboard.SetDataObject(previousClipboard, true); else Clipboard.Clear(); } catch { }
            Log(failures == 0 ? "RESULT PASS" : "RESULT FAIL (" + failures + ")");
        }
        return failures;
    }
}
