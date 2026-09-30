// Answers the program's dialogs during UI tests, from a background thread:
// message boxes (Sim/Não/OK) and the Windows save/open file dialogs (types the
// file name and confirms). Any dialog nobody expects is closed after 5 s so a
// test never hangs.
public static class DialogKit
{
    private static class W
    {
        public delegate bool EnumProc(IntPtr window, IntPtr data);
        [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback, IntPtr data);
        [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr data);
        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr window, out uint id);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetClassName(IntPtr h, StringBuilder s, int n);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
        [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] public static extern IntPtr GetDlgItem(IntPtr h, int id);
        [DllImport("user32.dll")] public static extern int GetDlgCtrlID(IntPtr h);
        [DllImport("user32.dll")] public static extern IntPtr GetParent(IntPtr h);
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, string l);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, StringBuilder l);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
    }

    // Button ids: 1 = OK/Salvar/Abrir, 2 = Cancelar, 6 = Sim, 7 = Não.
    public sealed class Expect
    {
        public string Title; public int Button; public string FileName;
        public int DefaultId; public string Text; public volatile bool Done;
    }

    private static readonly object gate = new object();
    private static readonly List<Expect> queue = new List<Expect>();
    private static volatile bool watching;
    private static System.Threading.Thread thread;
    public static readonly List<string> Unexpected = new List<string>();

    public static void Start()
    {
        lock (gate) { queue.Clear(); Unexpected.Clear(); }
        watching = true;
        thread = new System.Threading.Thread(Watch); thread.IsBackground = true; thread.Start();
    }

    public static void Stop() { watching = false; }

    public static Expect Arm(string title, int button)
    {
        var e = new Expect(); e.Title = title; e.Button = button;
        lock (gate) { queue.Add(e); }
        return e;
    }

    // A file dialog: types `fileName` and confirms (or cancels when fileName is null).
    public static Expect ArmFile(string title, string fileName)
    {
        var e = new Expect(); e.Title = title; e.FileName = fileName; e.Button = fileName == null ? 2 : 1;
        lock (gate) { queue.Add(e); }
        return e;
    }

    private static IntPtr FileNameEdit(IntPtr dialog)
    {
        IntPtr found = IntPtr.Zero;
        W.EnumChildWindows(dialog, delegate (IntPtr h, IntPtr d)
        {
            var c = new StringBuilder(64); W.GetClassName(h, c, 64);
            if (c.ToString() != "Edit") return true;
            // The file name box: in the save dialog an Edit with id 0x3E9 inside a
            // ComboBox; in the open dialog an Edit inside the combo with id 0x47C.
            int id = W.GetDlgCtrlID(h);
            var parent = new StringBuilder(64); W.GetClassName(W.GetParent(h), parent, 64);
            if (id == 0x3E9 && parent.ToString() == "ComboBox") { found = h; return false; }
            for (IntPtr p = h; p != IntPtr.Zero && p != dialog; p = W.GetParent(p))
                if (W.GetDlgCtrlID(p) == 0x47C) { found = h; return false; }
            return true;
        }, IntPtr.Zero);
        return found;
    }

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
                    else if ((DateTime.Now - seen[dlg]).TotalSeconds > 5)
                    {
                        lock (gate) { Unexpected.Add(t.ToString()); }
                        // Cancelar, then Não (a Sim/Não box ignores WM_CLOSE and Cancelar), then close.
                        W.PostMessage(dlg, 0x0111, (IntPtr)2, IntPtr.Zero);
                        W.PostMessage(dlg, 0x0111, (IntPtr)7, IntPtr.Zero);
                        W.PostMessage(dlg, 0x0010, IntPtr.Zero, IntPtr.Zero);
                        seen[dlg] = DateTime.Now;
                    }
                    continue;
                }
                System.Threading.Thread.Sleep(next.FileName != null ? 700 : 300);
                if (next.FileName != null)
                {
                    IntPtr edit = FileNameEdit(dlg);
                    if (edit == IntPtr.Zero)
                    {
                        if (!seen.ContainsKey(dlg)) seen[dlg] = DateTime.Now;
                        if ((DateTime.Now - seen[dlg]).TotalSeconds > 10) { lock (gate) { Unexpected.Add(t + " (campo de nome não encontrado)"); } W.PostMessage(dlg, 0x0111, (IntPtr)2, IntPtr.Zero); next.Done = true; }
                        continue;
                    }
                    // Typed as characters: the dialog ignores a name set with WM_SETTEXT.
                    W.SendMessage(edit, 0x00B1, IntPtr.Zero, (IntPtr)(-1)); // EM_SETSEL all
                    W.SendMessage(edit, 0x0102, (IntPtr)8, IntPtr.Zero);   // backspace
                    foreach (char ch in next.FileName) W.SendMessage(edit, 0x0102, (IntPtr)ch, IntPtr.Zero); // WM_CHAR
                    System.Threading.Thread.Sleep(200);
                    var typed = new StringBuilder(1024); W.SendMessage(edit, 0x000D, (IntPtr)1024, typed); // WM_GETTEXT
                    next.Text = t + " | " + typed;
                    // Safety: never confirm a name other than the one asked for.
                    bool same = String.Equals(typed.ToString(), next.FileName, StringComparison.OrdinalIgnoreCase);
                    if (!same) lock (gate) { Unexpected.Add("nome digitado difere: " + typed); }
                    W.PostMessage(dlg, 0x0111, (IntPtr)(same ? 1 : 2), IntPtr.Zero); // IDOK / IDCANCEL
                }
                else
                {
                    var body = new StringBuilder(2048); W.GetWindowText(W.GetDlgItem(dlg, 0xFFFF), body, 2048);
                    next.Text = body.ToString();
                    next.DefaultId = W.SendMessage(dlg, 0x0400, IntPtr.Zero, IntPtr.Zero).ToInt32() & 0xFFFF; // DM_GETDEFID
                    if (next.Button == 1 && W.GetDlgItem(dlg, 7) == IntPtr.Zero && W.GetDlgItem(dlg, 6) == IntPtr.Zero)
                        W.PostMessage(dlg, 0x0010, IntPtr.Zero, IntPtr.Zero); // OK-only box ignores WM_COMMAND
                    else
                        W.PostMessage(dlg, 0x0111, (IntPtr)next.Button, IntPtr.Zero);
                }
                next.Done = true;
                System.Threading.Thread.Sleep(300);
            }
            System.Threading.Thread.Sleep(60);
        }
    }
}
