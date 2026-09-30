// UI test of the consolidated-text protections: clear confirmation, close
// prompt, autosave, recovery on start and silent system shutdown. Uses the
// real engine (no model needed) and answers dialogs from a background thread.
public static class AutosaveTest
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
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, string l);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
        [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr data);
        [DllImport("user32.dll")] public static extern IntPtr GetParent(IntPtr h);
    }

    // File-name box of the Windows "Salvar como" dialog: the visible Edit
    // inside a ComboBox (the type selector has no Edit; the address bar is hidden).
    private static IntPtr FileNameBox(IntPtr dialog)
    {
        IntPtr found = IntPtr.Zero;
        W.EnumChildWindows(dialog, delegate (IntPtr h, IntPtr d)
        {
            var c = new StringBuilder(64); W.GetClassName(h, c, 64);
            var p = new StringBuilder(64); W.GetClassName(W.GetParent(h), p, 64);
            if (c.ToString() == "Edit" && p.ToString() == "ComboBox" && W.IsWindowVisible(h)) { found = h; return false; }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    // One expected dialog: title fragment, button to press, optional file name.
    private sealed class Expect
    {
        public string Title; public int Button; public string FileName;
        public string SeenText; public int DefaultId; public bool Done;
    }

    private static readonly object gate = new object();
    private static List<Expect> queue = new List<Expect>();
    private static List<string> unexpected = new List<string>();
    private static volatile bool watching;
    private static Dictionary<IntPtr, DateTime> firstSeen = new Dictionary<IntPtr, DateTime>();

    private static string Text(IntPtr h)
    {
        var s = new StringBuilder(1024); W.GetWindowText(h, s, 1024); return s.ToString();
    }

    private static void Watch()
    {
        uint self = (uint)Process.GetCurrentProcess().Id;
        while (watching)
        {
            var dialogs = new List<IntPtr>();
            W.EnumWindows(delegate (IntPtr h, IntPtr d)
            {
                uint pid; W.GetWindowThreadProcessId(h, out pid);
                if (pid == self && W.IsWindowVisible(h))
                {
                    var c = new StringBuilder(64); W.GetClassName(h, c, 64);
                    if (c.ToString() == "#32770") dialogs.Add(h);
                }
                return true;
            }, IntPtr.Zero);
            foreach (IntPtr dlg in dialogs)
            {
                string title = Text(dlg);
                Expect next = null;
                lock (gate) { foreach (Expect e in queue) if (!e.Done) { next = e; break; } }
                if (next == null || title.IndexOf(next.Title, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    lock (gate) { if (!unexpected.Contains(title)) unexpected.Add(title); }
                    // A dialog nobody expects would block the test forever: cancel it after 5 s.
                    if (!firstSeen.ContainsKey(dlg)) firstSeen[dlg] = DateTime.Now;
                    else if ((DateTime.Now - firstSeen[dlg]).TotalSeconds > 5) W.PostMessage(dlg, 0x0111, (IntPtr)2, IntPtr.Zero);
                    continue;
                }
                System.Threading.Thread.Sleep(next.FileName != null ? 900 : 250);
                IntPtr body = W.GetDlgItem(dlg, 0xFFFF);
                next.SeenText = body == IntPtr.Zero ? "" : Text(body);
                next.DefaultId = W.SendMessage(dlg, 0x0400 /*DM_GETDEFID*/, IntPtr.Zero, IntPtr.Zero).ToInt32() & 0xFFFF;
                int button = next.Button;
                if (next.FileName != null)
                {
                    IntPtr name = FileNameBox(dlg);
                    if (name != IntPtr.Zero)
                    {
                        // Typed as keystrokes: the dialog ignores WM_SETTEXT on its name box.
                        W.SendMessage(name, 0x000C /*WM_SETTEXT*/, IntPtr.Zero, "");
                        foreach (char ch in next.FileName) W.SendMessage(name, 0x0102 /*WM_CHAR*/, (IntPtr)ch, IntPtr.Zero);
                    }
                    System.Threading.Thread.Sleep(300);
                    // Never let the dialog save under its default name elsewhere.
                    if (name == IntPtr.Zero || Text(name) != next.FileName) { button = 2 /*IDCANCEL*/; next.SeenText = "nome do arquivo não pôde ser preenchido"; }
                }
                W.PostMessage(dlg, 0x0111 /*WM_COMMAND*/, (IntPtr)button, IntPtr.Zero);
                next.Done = true;
                System.Threading.Thread.Sleep(300);
            }
            System.Threading.Thread.Sleep(60);
        }
    }

    private static Expect Arm(string title, int button, string fileName)
    {
        var e = new Expect(); e.Title = title; e.Button = button; e.FileName = fileName;
        lock (gate) { queue.Add(e); }
        return e;
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(reportPath, message + Environment.NewLine, Encoding.UTF8);
    }

    private static void Check(bool ok, string name, string detail)
    {
        if (!ok) failures++;
        Log((ok ? "PASS " : "FAIL ") + name + (String.IsNullOrEmpty(detail) ? "" : " | " + detail));
    }

    private static void Pump(int ms)
    {
        var t = Stopwatch.StartNew();
        while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); }
    }

    private static bool WaitFor(Func<bool> done, int ms)
    {
        var t = Stopwatch.StartNew();
        while (!done()) { if (t.ElapsedMilliseconds > ms) return false; Application.DoEvents(); System.Threading.Thread.Sleep(15); }
        return true;
    }

    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }

    private static void Call(object target, string name)
    {
        target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, new object[] { null, EventArgs.Empty });
    }

    // Safety net: if a save dialog used its default name and folder, remove
    // that file only when it is recent and holds this test's own sentence.
    private static bool StrayRemoved(string defaultName, string testText)
    {
        bool removed = false;
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] folders = {
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(profile, "OneDrive", "Documentos"), Path.Combine(profile, "OneDrive", "Documents"),
            Path.Combine(profile, "Documents"), Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) };
        foreach (string folder in folders)
        {
            string path = Path.Combine(folder, defaultName);
            if (!File.Exists(path) || (DateTime.Now - File.GetLastWriteTime(path)).TotalSeconds > 60) continue;
            bool mine = defaultName.EndsWith(".docx")
                ? DocxText(path).Contains(testText)
                : DraftText(File.ReadAllText(path, Encoding.UTF8)) == testText;
            if (mine) { File.Delete(path); removed = true; Log("LIMPEZA arquivo gravado fora da pasta de teste e removido: " + path); }
        }
        return removed;
    }

    private static string DocxText(string path)
    {
        using (ZipArchive zip = ZipFile.OpenRead(path))
        using (var reader = new StreamReader(zip.GetEntry("word/document.xml").Open(), Encoding.UTF8))
            return reader.ReadToEnd();
    }

    // The text part of a draft (the recovery copy and «Salvar rascunho» also keep the qualification fields).
    private static string DraftText(string content)
    {
        string text; Qualification q;
        DraftFile.Parse(content, out text, out q);
        return text;
    }

    private static string Saved()
    {
        return File.Exists(AutosaveStore.FilePath) ? DraftText(File.ReadAllText(AutosaveStore.FilePath, Encoding.UTF8)) : null;
    }

    private static Process StartEngine(string root)
    {
        var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
        start.WorkingDirectory = Path.Combine(root, "engine");
        start.UseShellExecute = false; start.CreateNoWindow = true; start.WindowStyle = ProcessWindowStyle.Hidden;
        return Process.Start(start);
    }

    public static int Run(string root, string workDir)
    {
        reportPath = Path.Combine(root, "maintenance", "autosave", "results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Application.EnableVisualStyles();
        string realCopy = AutosaveStore.FilePath + ".antes-do-teste";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        watching = true;
        var watcher = new System.Threading.Thread(Watch); watcher.IsBackground = true; watcher.Start();
        Process engine = null;
        try
        {
            // 4) Recovery offered on start, with date and time.
            string pending = "Trecho de teste que ficou sem salvar.";
            AutosaveStore.Write(pending);
            File.SetLastWriteTime(AutosaveStore.FilePath, new DateTime(2026, 1, 2, 3, 4, 0));
            engine = StartEngine(root);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            var form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            var combined = Field<SectionCard>(form, "combinedCard");
            var status = Field<Label>(form, "status");
            Expect recovery = Arm("Recuperar depoimento", 6 /*IDYES*/, null);
            form.Show();
            WaitFor(delegate { return recovery.Done; }, 8000); Pump(400);
            Check(recovery.Done && recovery.SeenText.Contains("02/01/2026 às 03:04"), "4: recuperação oferecida ao abrir, com data e hora", recovery.SeenText.Replace("\n", " "));
            Check(combined.Editor.Text == pending && status.Text.Contains("recuperado"), "4: «Sim» restaura o texto no consolidado", null);

            // 3) Autosave 1.5 s after the last change; the timer restarts on each change.
            combined.Editor.Text = pending + " Primeira alteração.";
            Pump(700);
            combined.Editor.Text = pending + " Segunda alteração.";
            var sw = Stopwatch.StartNew();
            Pump(1000);
            Check(Saved() == pending, "3: nada gravado antes de 1,5 s da última alteração", null);
            bool written = WaitFor(delegate { return Saved() == combined.Editor.Text; }, 3000);
            Check(written && sw.ElapsedMilliseconds >= 1400, "3: cópia gravada cerca de 1,5 s após a última alteração", sw.ElapsedMilliseconds + " ms");

            // 1) Clear asks first, with «Não» as default.
            Expect keep = Arm("Limpar consolidado", 7 /*IDNO*/, null);
            string before = combined.Editor.Text;
            Call(form, "Clear_Click"); Pump(300);
            Check(keep.Done && keep.DefaultId == 7, "1: confirmação de limpar com «Não» como botão padrão", "padrão=" + keep.DefaultId);
            Check(combined.Editor.Text == before && Saved() == before, "1: «Não» mantém o texto e a cópia", null);
            Expect clear = Arm("Limpar consolidado", 6, null);
            Call(form, "Clear_Click"); Pump(300);
            Check(clear.Done && combined.Editor.Text.Length == 0 && Saved() == null, "1/3: «Sim» limpa e apaga a cópia de recuperação", null);

            // 3) Saving and exporting delete the copy.
            combined.Editor.Text = "Texto para salvar como rascunho.";
            WaitFor(delegate { return Saved() != null; }, 3000);
            string txt = Path.Combine(workDir, "rascunho-teste.txt");
            Expect save = Arm("Salvar", 1 /*IDOK*/, txt);
            Call(form, "Save_Click"); Pump(500);
            bool straySave = StrayRemoved("depoimento-rascunho.txt", "Texto para salvar como rascunho.");
            Check(save.Done && File.Exists(txt) && DraftText(File.ReadAllText(txt, Encoding.UTF8)) == "Texto para salvar como rascunho." && Saved() == null && !straySave, "3: «Salvar rascunho» grava o arquivo e apaga a cópia", null);
            combined.Editor.Text = "Texto para exportar em Word.";
            WaitFor(delegate { return Saved() != null; }, 3000);
            string docx = Path.Combine(workDir, "exportado-teste.docx");
            Expect export = Arm("Salvar", 1, docx);
            Call(form, "Export_Click"); Pump(500);
            bool strayExport = StrayRemoved("depoimento.docx", "Texto para exportar em Word.");
            Check(export.Done && File.Exists(docx) && Saved() == null && !strayExport, "3: «Exportar Word» grava o .docx e apaga a cópia", null);

            // 2) Close with unsaved text: Cancelar, and Sim + cancelled save, keep it open.
            combined.Editor.Text = "Texto não salvo ao fechar.";
            WaitFor(delegate { return Saved() != null; }, 3000);
            Expect cancelClose = Arm("Fidelis", 2 /*IDCANCEL*/, null);
            form.Close(); Pump(500);
            Check(cancelClose.Done && cancelClose.SeenText.Contains("não foi salvo") && form.Visible && !engine.HasExited, "2: «Cancelar» mantém a janela e o motor abertos", null);
            Expect yes = Arm("Fidelis", 6, null);
            Expect giveUp = Arm("Salvar", 2 /*IDCANCEL*/, null);
            form.Close(); Pump(500);
            Check(yes.Done && giveUp.Done && form.Visible && !engine.HasExited && Saved() == "Texto não salvo ao fechar.", "2: «Sim» e cancelar a janela de salvar não fecha o programa", null);
            Expect saveOnClose = Arm("Fidelis", 6, null);
            string closeDocx = Path.Combine(workDir, "ao-fechar.docx");
            Arm("Salvar", 1, closeDocx);
            form.Close(); Pump(800);
            bool strayClose = StrayRemoved("depoimento.docx", "Texto não salvo ao fechar.");
            Check(saveOnClose.Done && File.Exists(closeDocx) && !form.Visible && Saved() == null && !strayClose, "2: «Sim» oferece Word e salva antes de fechar", null);
            Check(WaitFor(delegate { return engine.HasExited; }, 5000), "2: motor encerrado só depois da decisão", null);
            form.Dispose();

            // 2) «Não» discards the copy and closes.
            engine = StartEngine(root);
            bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            combined = Field<SectionCard>(form, "combinedCard");
            form.Show(); Pump(500);
            combined.Editor.Text = "Texto que será descartado.";
            WaitFor(delegate { return Saved() != null; }, 3000);
            Expect no = Arm("Fidelis", 7, null);
            form.Close(); Pump(500);
            Check(no.Done && !form.Visible && Saved() == null && WaitFor(delegate { return engine.HasExited; }, 5000), "2/3: «Não» fecha, apaga a cópia e encerra o motor", null);
            form.Dispose();

            // 5) Windows shutdown / forced end: no question, copy kept.
            foreach (CloseReason reason in new CloseReason[] { CloseReason.WindowsShutDown, CloseReason.TaskManagerClosing })
            {
                engine = StartEngine(root);
                bridge = new OriginalAppBridge(engine);
                if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
                form = new ModernDepoimentoForm(bridge, "", "Vulkan");
                combined = Field<SectionCard>(form, "combinedCard");
                form.Show(); Pump(500);
                combined.Editor.Text = "Texto em " + reason + ".";
                Pump(300);
                lock (gate) { unexpected.Clear(); }
                var args = new FormClosingEventArgs(reason, false);
                form.GetType().GetMethod("Form_Closing", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(form, new object[] { form, args });
                Pump(600);
                bool quiet; lock (gate) { quiet = unexpected.Count == 0; }
                Check(quiet && !args.Cancel && Saved() == "Texto em " + reason + "." && WaitFor(delegate { return engine.HasExited; }, 5000),
                    "5: " + reason + " não pergunta nada e mantém a cópia", null);
                form.Dispose();
                AutosaveStore.Delete();
            }

            // Recovery «Não» discards the copy.
            AutosaveStore.Write("Cópia antiga a descartar.");
            engine = StartEngine(root);
            bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            combined = Field<SectionCard>(form, "combinedCard");
            Expect discard = Arm("Recuperar depoimento", 7, null);
            form.Show(); WaitFor(delegate { return discard.Done; }, 8000); Pump(400);
            Check(discard.Done && combined.Editor.Text.Length == 0 && Saved() == null, "4: «Não» na recuperação descarta a cópia", null);
            form.Close(); Pump(400); form.Dispose();
        }
        catch (Exception ex) { failures++; Log("FAIL execução: " + ex); }
        finally
        {
            watching = false;
            StrayRemoved("depoimento-rascunho.txt", "Texto para salvar como rascunho.");
            StrayRemoved("depoimento.docx", "Texto para exportar em Word.");
            StrayRemoved("depoimento.docx", "Texto não salvo ao fechar.");
            try { if (engine != null && !engine.HasExited) engine.Kill(); } catch { }
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
            lock (gate) { if (unexpected.Count > 0) Log("INFO diálogos não esperados: " + String.Join(" | ", unexpected.ToArray())); }
            Log(failures == 0 ? "RESULT PASS" : "RESULT FAIL (" + failures + ")");
        }
        return failures;
    }
}
