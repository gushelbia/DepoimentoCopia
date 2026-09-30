using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

public static class NativeBridgeApi
{
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr data);
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool IsWindowEnabled(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern int GetDlgCtrlID(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr GetParent(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam);
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam, uint flags, uint timeout, out IntPtr result);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);
    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

    public const uint WM_SETTEXT = 0x000C;
    public const uint WM_GETTEXT = 0x000D;
    public const uint WM_GETTEXTLENGTH = 0x000E;
    public const uint BM_CLICK = 0x00F5;
    public const uint WM_COMMAND = 0x0111;
    public const uint PBM_GETPOS = 0x0408;
    public const int SW_HIDE = 0;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
}

public sealed class OriginalAppBridge
{
    private Process process;
    private IntPtr mainWindow;
    private List<IntPtr> editors = new List<IntPtr>();
    private Dictionary<string, IntPtr> buttons = new Dictionary<string, IntPtr>(StringComparer.OrdinalIgnoreCase);
    private IntPtr statusLabel = IntPtr.Zero;
    private IntPtr timerLabel = IntPtr.Zero;
    private IntPtr progressBar = IntPtr.Zero;
    private readonly System.Threading.SemaphoreSlim communicationLock = new System.Threading.SemaphoreSlim(1, 1);
    private volatile bool modelReady;
    public bool ModelReady { get { return modelReady; } }

    public void InvalidateModel() { modelReady = false; }

    // Serialize HWND discovery and communication away from the form's UI thread.
    public async Task<T> InvokeAsync<T>(Func<T> operation)
    {
        await communicationLock.WaitAsync().ConfigureAwait(false);
        try { return await Task.Run(operation).ConfigureAwait(false); }
        finally { communicationLock.Release(); }
    }

    public bool ConfirmModelLoaded()
    {
        modelReady = StatusText.StartsWith("Modelo carregado", StringComparison.OrdinalIgnoreCase)
            && ButtonEnabled("Carregar modelo") && ButtonEnabled("Reformular");
        return modelReady;
    }

    public void AcknowledgeModelLoadError()
    {
        modelReady = false;
        // The engine is hidden; surface its failure in our status label and release
        // only its known load-error dialog so a corrected model can be loaded.
        NativeBridgeApi.EnumWindows(delegate(IntPtr window, IntPtr data)
        {
            uint pid;
            NativeBridgeApi.GetWindowThreadProcessId(window, out pid);
            if (pid == process.Id && GetText(window) == "Erro ao carregar modelo")
                NativeBridgeApi.PostMessage(window, 0x0010, IntPtr.Zero, IntPtr.Zero);
            return true;
        }, IntPtr.Zero);
    }

    public OriginalAppBridge(Process p)
    {
        process = p;
    }

    public bool Connect(int timeoutMs)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                process.Refresh();
                if (process.HasExited) return false;
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    mainWindow = process.MainWindowHandle;
                    if (DiscoverControls())
                    {
                        NativeBridgeApi.ShowWindow(mainWindow, NativeBridgeApi.SW_HIDE);
                        return true;
                    }
                }
            }
            catch { }
            System.Threading.Thread.Sleep(100);
        }
        return false;
    }

    private bool DiscoverControls()
    {
        editors.Clear();
        buttons.Clear();
        // Status text changes after startup; keep its binding during rediscovery.
        if (!NativeBridgeApi.IsWindow(statusLabel)) statusLabel = IntPtr.Zero;
        if (!NativeBridgeApi.IsWindow(timerLabel)) timerLabel = IntPtr.Zero;
        if (!NativeBridgeApi.IsWindow(progressBar)) progressBar = IntPtr.Zero;

        List<ChildInfo> all = new List<ChildInfo>();
        NativeBridgeApi.EnumWindowsProc cb = delegate(IntPtr h, IntPtr lp)
        {
            ChildInfo ci = new ChildInfo();
            ci.Handle = h;
            ci.ClassName = GetClass(h);
            ci.Text = GetText(h);
            NativeBridgeApi.RECT r;
            if (NativeBridgeApi.GetWindowRect(h, out r)) ci.Top = r.Top;
            all.Add(ci);
            return true;
        };
        NativeBridgeApi.EnumChildWindows(mainWindow, cb, IntPtr.Zero);

        foreach (ChildInfo ci in all)
        {
            string cls = ci.ClassName.ToUpperInvariant();
            if (cls.Contains("EDIT")) editors.Add(ci.Handle);
            if (cls.Contains("PROGRESS")) progressBar = ci.Handle;
            if (ci.Text == "Selecione o Qwen2.5 3B Q4_K_M e carregue o modelo.") statusLabel = ci.Handle;
            if (ci.Text.StartsWith("Tempo:", StringComparison.OrdinalIgnoreCase)) timerLabel = ci.Handle;
            if (IsKnownButton(ci.Text)) buttons[ci.Text] = ci.Handle;
        }

        editors.Sort(delegate(IntPtr a, IntPtr b)
        {
            NativeBridgeApi.RECT ra, rb;
            NativeBridgeApi.GetWindowRect(a, out ra);
            NativeBridgeApi.GetWindowRect(b, out rb);
            if (ra.Top != rb.Top) return ra.Top.CompareTo(rb.Top);
            return ra.Left.CompareTo(rb.Left);
        });

        return editors.Count >= 4 && buttons.ContainsKey("Carregar modelo") && buttons.ContainsKey("Reformular") && buttons.ContainsKey("Cancelar");
    }

    private static bool IsKnownButton(string s)
    {
        return s == "Selecionar GGUF" || s == "Carregar modelo" || s == "Reformular" || s == "Cancelar" ||
               s == "Adicionar ao depoimento" || s == "Copiar" || s == "Salvar rascunho" ||
               s == "Exportar Word" || s == "Limpar consolidado" || s == "Abrir log";
    }

    private static string GetClass(IntPtr h)
    {
        StringBuilder sb = new StringBuilder(256);
        NativeBridgeApi.GetClassName(h, sb, sb.Capacity);
        return sb.ToString();
    }

    public static string GetText(IntPtr h)
    {
        if (h == IntPtr.Zero) return "";
        IntPtr result;
        if (NativeBridgeApi.SendMessageTimeout(h, NativeBridgeApi.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero, 2, 1500, out result) == IntPtr.Zero)
            throw new InvalidOperationException("O motor local não respondeu. Reabra o programa.");
        int len = result.ToInt32();
        if (len < 0) len = 0;
        if (len > 2000000) len = 2000000;
        StringBuilder sb = new StringBuilder(len + 2);
        if (NativeBridgeApi.SendMessageTimeout(h, NativeBridgeApi.WM_GETTEXT, (IntPtr)sb.Capacity, sb, 2, 1500, out result) == IntPtr.Zero)
            throw new InvalidOperationException("O motor local não respondeu. Reabra o programa.");
        return sb.ToString();
    }

    private static void SetText(IntPtr h, string text)
    {
        if (h == IntPtr.Zero) return;
        IntPtr result;
        if (NativeBridgeApi.SendMessageTimeout(h, NativeBridgeApi.WM_SETTEXT, IntPtr.Zero, text ?? "", 2, 1500, out result) == IntPtr.Zero)
            throw new InvalidOperationException("O motor local não respondeu. Reabra o programa.");
    }

    private void EnsureControlsAvailable()
    {
        if (process.HasExited || !NativeBridgeApi.IsWindow(mainWindow))
            throw new InvalidOperationException("O motor do Depoimento Local foi encerrado. Reabra o programa.");

        bool valid = editors.Count >= 4 && buttons.ContainsKey("Reformular");
        foreach (IntPtr editor in editors) valid &= NativeBridgeApi.IsWindow(editor);
        foreach (IntPtr button in buttons.Values) valid &= NativeBridgeApi.IsWindow(button);
        if (!valid && !DiscoverControls())
            throw new InvalidOperationException("Não foi possível sincronizar os controles do motor local.");
    }

    private IntPtr EditorHandle(int index)
    {
        EnsureControlsAvailable();
        return editors[index];
    }

    public string ModelPath { get { return GetText(EditorHandle(0)); } set { SetText(EditorHandle(0), value); } }
    public string OriginalText { get { return GetText(EditorHandle(1)); } set { SetText(EditorHandle(1), value); } }
    public string ReformulatedText { get { return GetText(EditorHandle(2)); } set { SetText(EditorHandle(2), value); } }
    public string CombinedText { get { return GetText(EditorHandle(3)); } set { SetText(EditorHandle(3), value); } }
    public string StatusText { get { return GetText(statusLabel); } }
    public string TimerText { get { return GetText(timerLabel); } }
    public int ProgressValue
    {
        get
        {
            if (progressBar == IntPtr.Zero) return 0;
            IntPtr result;
            if (NativeBridgeApi.SendMessageTimeout(progressBar, NativeBridgeApi.PBM_GETPOS, IntPtr.Zero, IntPtr.Zero, 2, 1500, out result) == IntPtr.Zero)
                throw new InvalidOperationException("O motor local não respondeu. Reabra o programa.");
            return result.ToInt32();
        }
    }

    public bool ButtonEnabled(string text)
    {
        EnsureControlsAvailable();
        IntPtr h;
        return buttons.TryGetValue(text, out h) && NativeBridgeApi.IsWindowEnabled(h);
    }

    public bool Click(string text)
    {
        // A control being enabled does not imply LlmService.IsLoaded.
        if ((text == "Reformular" || text == "Cancelar") && !modelReady) return false;
        EnsureControlsAvailable();
        IntPtr h;
        if (!buttons.TryGetValue(text, out h) || !NativeBridgeApi.IsWindowEnabled(h))
            return false;
        if (text == "Carregar modelo")
        {
            modelReady = false;
            // Clear any success from an earlier load before queuing this request.
            SetText(statusLabel, "Carregando modelo local...");
        }
        int id = NativeBridgeApi.GetDlgCtrlID(h);
        IntPtr parent = NativeBridgeApi.GetParent(h);
        if (id > 0 && parent != IntPtr.Zero)
            return NativeBridgeApi.PostMessage(parent, NativeBridgeApi.WM_COMMAND, (IntPtr)(id & 0xFFFF), h);
        else
            return NativeBridgeApi.PostMessage(h, NativeBridgeApi.BM_CLICK, IntPtr.Zero, IntPtr.Zero);
    }

    public void HideOriginal()
    {
        if (mainWindow != IntPtr.Zero && NativeBridgeApi.IsWindow(mainWindow)) NativeBridgeApi.ShowWindow(mainWindow, NativeBridgeApi.SW_HIDE);
    }

    public void Stop()
    {
        try
        {
            if (!process.HasExited) process.Kill();
        }
        catch { }
    }

    private class ChildInfo
    {
        public IntPtr Handle;
        public string ClassName;
        public string Text;
        public int Top;
    }
}

public enum ThemeChoice
{
    System = 0,
    Light = 1,
    Dark = 2
}

public sealed class UiPalette
{
    public bool IsDark;
    public Color Window;
    public Color Card;
    public Color Editor;
    public Color EditorBorder;
    public Color Border;
    public Color Text;
    public Color Muted;
    public Color SecondaryButton;
    public Color SecondaryButtonText;
    public Color SecondaryButtonBorder;
    public Color DangerButton;
    public Color DangerText;
    public Color DangerBorder;
    public Color ProgressTrack;
    public Color Blue;

    public static UiPalette Create(bool dark)
    {
        UiPalette p = new UiPalette();
        p.IsDark = dark;
        p.Blue = Color.FromArgb(10, 132, 255);
        if (dark)
        {
            p.Window = Color.FromArgb(20, 24, 30);
            p.Card = Color.FromArgb(29, 34, 43);
            p.Editor = Color.FromArgb(18, 22, 28);
            p.EditorBorder = Color.FromArgb(53, 61, 74);
            p.Border = Color.FromArgb(49, 57, 69);
            p.Text = Color.FromArgb(236, 240, 246);
            p.Muted = Color.FromArgb(158, 168, 184);
            p.SecondaryButton = Color.FromArgb(37, 43, 53);
            p.SecondaryButtonText = Color.FromArgb(225, 231, 240);
            p.SecondaryButtonBorder = Color.FromArgb(63, 72, 86);
            p.DangerButton = Color.FromArgb(57, 31, 34);
            p.DangerText = Color.FromArgb(255, 131, 131);
            p.DangerBorder = Color.FromArgb(104, 51, 56);
            p.ProgressTrack = Color.FromArgb(53, 61, 72);
        }
        else
        {
            p.Window = Color.FromArgb(244, 247, 251);
            p.Card = Color.White;
            p.Editor = Color.White;
            p.EditorBorder = Color.FromArgb(211, 219, 230);
            p.Border = Color.FromArgb(216, 223, 233);
            p.Text = Color.FromArgb(22, 38, 65);
            p.Muted = Color.FromArgb(100, 111, 130);
            p.SecondaryButton = Color.White;
            p.SecondaryButtonText = Color.FromArgb(42, 61, 91);
            p.SecondaryButtonBorder = Color.FromArgb(216, 223, 233);
            p.DangerButton = Color.FromArgb(255, 244, 244);
            p.DangerText = Color.FromArgb(220, 47, 47);
            p.DangerBorder = Color.FromArgb(255, 204, 204);
            p.ProgressTrack = Color.FromArgb(221, 227, 235);
        }
        return p;
    }
}

public static class ThemeManager
{
    private static string SettingsFolder
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DepoimentoLocal");
        }
    }

    private static string SettingsPath
    {
        get { return Path.Combine(SettingsFolder, "theme.txt"); }
    }

    public static ThemeChoice LoadChoice()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string s = File.ReadAllText(SettingsPath).Trim();
                ThemeChoice c;
                if (Enum.TryParse<ThemeChoice>(s, true, out c)) return c;
            }
        }
        catch { }
        return ThemeChoice.System;
    }

    public static void SaveChoice(ThemeChoice choice)
    {
        try
        {
            Directory.CreateDirectory(SettingsFolder);
            File.WriteAllText(SettingsPath, choice.ToString(), new UTF8Encoding(false));
        }
        catch { }
    }

    public static bool ResolveDark(ThemeChoice choice)
    {
        if (choice == ThemeChoice.Dark) return true;
        if (choice == ThemeChoice.Light) return false;
        return IsWindowsDark();
    }

    private static bool IsWindowsDark()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key != null)
                {
                    object v = key.GetValue("AppsUseLightTheme");
                    if (v is int) return ((int)v) == 0;
                }
            }
        }
        catch { }
        return false;
    }
}

public class RoundedPanel : Panel
{
    public int Radius = 14;
    public Color FillColor = Color.White;
    public Color BorderColor = Color.FromArgb(223, 229, 238);

    public RoundedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Padding = new Padding(1);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle r = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using (GraphicsPath gp = RoundedRect(r, Radius))
        using (SolidBrush b = new SolidBrush(FillColor))
        using (Pen p = new Pen(BorderColor))
        {
            e.Graphics.FillPath(b, gp);
            e.Graphics.DrawPath(p, gp);
        }
    }

    public static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        int d = Math.Max(2, radius * 2);
        GraphicsPath gp = new GraphicsPath();
        gp.AddArc(r.Left, r.Top, d, d, 180, 90);
        gp.AddArc(r.Right - d, r.Top, d, d, 270, 90);
        gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        gp.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
        gp.CloseFigure();
        return gp;
    }
}

public class ModernButton : Button
{
    private int radius = 8;
    private Color normalColor;
    private Color hoverColor;

    public ModernButton(string text, Color back, Color fore, Color border)
    {
        Text = text;
        Height = 38;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16, 0, 16, 0);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        FlatStyle = FlatStyle.Flat;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        Resize += delegate { UpdateRegion(); };
        SetPalette(back, fore, border);
    }

    public void SetPalette(Color back, Color fore, Color border)
    {
        normalColor = back;
        hoverColor = Shift(back, back.GetBrightness() > 0.72f ? -9 : 11);
        BackColor = back;
        ForeColor = fore;
        FlatAppearance.MouseDownBackColor = Shift(back, back.GetBrightness() > 0.72f ? -16 : 18);
        FlatAppearance.MouseOverBackColor = hoverColor;
        if (border.A == 0)
        {
            FlatAppearance.BorderSize = 0;
        }
        else
        {
            FlatAppearance.BorderSize = 1;
            FlatAppearance.BorderColor = border;
        }
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        BackColor = hoverColor;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        BackColor = normalColor;
    }

    private static Color Shift(Color c, int amount)
    {
        int r = Math.Max(0, Math.Min(255, c.R + amount));
        int g = Math.Max(0, Math.Min(255, c.G + amount));
        int b = Math.Max(0, Math.Min(255, c.B + amount));
        return Color.FromArgb(r, g, b);
    }

    private void UpdateRegion()
    {
        Rectangle r = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using (GraphicsPath gp = RoundedPanel.RoundedRect(r, radius))
        {
            Region old = Region;
            Region = new Region(gp);
            if (old != null) old.Dispose();
        }
    }
}

public class ThemedComboBox : ComboBox
{
    private Color surface = Color.White;
    private Color textColor = Color.Black;
    private Color selected = Color.FromArgb(10, 132, 255);
    private Color border = Color.FromArgb(216, 223, 233);
    private Color arrowColor = Color.Black;

    public ThemedComboBox()
    {
        DropDownStyle = ComboBoxStyle.DropDownList;
        DrawMode = DrawMode.OwnerDrawFixed;
        FlatStyle = FlatStyle.Flat;
        ItemHeight = 24;
        IntegralHeight = false;
        DropDownHeight = 120;
        Font = new Font("Segoe UI", 9.5F);
    }

    public void SetPalette(UiPalette p)
    {
        surface = p.SecondaryButton;
        textColor = p.SecondaryButtonText;
        selected = p.Blue;
        border = p.SecondaryButtonBorder;
        arrowColor = p.SecondaryButtonText;
        BackColor = surface;
        ForeColor = textColor;
        try
        {
            if (IsHandleCreated)
                NativeBridgeApi.SetWindowTheme(Handle, p.IsDark ? "DarkMode_Explorer" : "Explorer", null);
        }
        catch { }
        Invalidate();
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0)
        {
            using (SolidBrush b = new SolidBrush(surface)) e.Graphics.FillRectangle(b, e.Bounds);
            return;
        }

        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color bg = isSelected ? selected : surface;
        Color fg = isSelected ? Color.White : textColor;
        using (SolidBrush b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, e.Bounds);
        Rectangle textRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, Math.Max(1, e.Bounds.Width - 12), e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, textRect, fg,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0x000F && FlatStyle == FlatStyle.Flat)
        {
            DrawChrome();
        }
    }

    private void DrawChrome()
    {
        try
        {
            using (Graphics g = CreateGraphics())
            {
                int arrowWidth = 27;
                Rectangle arrowRect = new Rectangle(Math.Max(0, Width - arrowWidth - 1), 1, Math.Max(1, arrowWidth), Math.Max(1, Height - 2));
                using (SolidBrush b = new SolidBrush(surface)) g.FillRectangle(b, arrowRect);

                int cx = arrowRect.Left + arrowRect.Width / 2;
                int cy = arrowRect.Top + arrowRect.Height / 2 + 1;
                using (Pen chevron = new Pen(arrowColor, 1.6F))
                {
                    chevron.StartCap = LineCap.Round;
                    chevron.EndCap = LineCap.Round;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawLine(chevron, cx - 4, cy - 2, cx, cy + 2);
                    g.DrawLine(chevron, cx, cy + 2, cx + 4, cy - 2);
                }

                using (Pen p = new Pen(border))
                {
                    Rectangle r = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
                    g.DrawRectangle(p, r);
                }
            }
        }
        catch { }
    }
}

public class ModernProgressBar : Control
{
    private int value;
    private Color trackColor = Color.FromArgb(221, 227, 235);
    private Color fillColor = Color.FromArgb(10, 132, 255);

    public int Value
    {
        get { return value; }
        set { this.value = Math.Max(0, Math.Min(100, value)); Invalidate(); }
    }

    public ModernProgressBar()
    {
        Height = 8;
        DoubleBuffered = true;
        BackColor = Color.FromArgb(244, 247, 251);
    }

    public void SetPalette(Color surface, Color track, Color fill)
    {
        BackColor = surface;
        trackColor = track;
        fillColor = fill;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle bg = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using (GraphicsPath gp = RoundedPanel.RoundedRect(bg, 4))
        using (SolidBrush b = new SolidBrush(trackColor))
        {
            e.Graphics.FillPath(b, gp);
        }

        int w = (int)Math.Round((Width - 1) * value / 100.0);
        if (w > 3)
        {
            Rectangle fg = new Rectangle(0, 0, w, Math.Max(1, Height - 1));
            using (GraphicsPath gp = RoundedPanel.RoundedRect(fg, 4))
            using (SolidBrush b = new SolidBrush(fillColor))
            {
                e.Graphics.FillPath(b, gp);
            }
        }
    }
}

public sealed class EditorMenuColors : ProfessionalColorTable
{
    private readonly UiPalette palette;
    public EditorMenuColors(UiPalette palette) { this.palette = palette; UseSystemColors = false; }
    public override Color ToolStripDropDownBackground { get { return palette.Editor; } }
    public override Color MenuBorder { get { return palette.EditorBorder; } }
    public override Color MenuItemSelected { get { return palette.SecondaryButton; } }
    public override Color MenuItemBorder { get { return palette.EditorBorder; } }
}

public sealed class EditorMenuRenderer : ToolStripProfessionalRenderer
{
    private readonly UiPalette palette;
    public EditorMenuRenderer(UiPalette palette) : base(new EditorMenuColors(palette)) { this.palette = palette; }
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Enabled ? palette.Text : palette.Muted;
        base.OnRenderItemText(e);
    }
}

public class SectionCard : RoundedPanel
{
    public RichTextBox Editor;
    public ModernButton ClearButton;
    private Label title;
    private RoundedPanel editorHost;
    private TableLayoutPanel layout;
    private Label placeholder;
    private ContextMenuStrip editorMenu;
    private ToolStripMenuItem cutItem;
    private ToolStripMenuItem copyItem;
    private ToolStripMenuItem pasteItem;
    private ToolStripMenuItem selectAllItem;

    public SectionCard(string heading, bool readOnly) : this(heading, readOnly, false) { }

    public SectionCard(string heading, bool readOnly, bool allowClear)
    {
        Padding = new Padding(14, 9, 14, 12);
        Margin = new Padding(0, 6, 0, 6);

        layout = new TableLayoutPanel();
        layout.Dock = DockStyle.Fill;
        layout.ColumnCount = 1;
        layout.RowCount = 2;
        layout.BackColor = Color.Transparent;
        layout.Margin = new Padding(0);
        layout.Padding = new Padding(0);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        Controls.Add(layout);

        title = new Label();
        title.Text = heading;
        title.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        title.Dock = DockStyle.Fill;
        title.TextAlign = ContentAlignment.MiddleLeft;
        title.Margin = new Padding(4, 0, 0, 3);
        title.BackColor = Color.Transparent;
        TableLayoutPanel headingLayout = new TableLayoutPanel();
        headingLayout.Dock = DockStyle.Fill;
        headingLayout.Margin = new Padding(0);
        headingLayout.ColumnCount = 2;
        headingLayout.RowCount = 1;
        headingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        headingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        headingLayout.Controls.Add(title, 0, 0);
        if (allowClear)
        {
            ClearButton = new ModernButton("Limpar", Color.White, Color.Black, Color.Gray);
            ClearButton.AccessibleName = "Limpar " + heading;
            ClearButton.AutoSize = false;
            ClearButton.Size = new Size(85, 27);
            ClearButton.Dock = DockStyle.Fill;
            ClearButton.Margin = new Padding(8, 0, 0, 3);
            ClearButton.Font = new Font("Segoe UI", 9F);
            ClearButton.Padding = new Padding(8, 0, 8, 0);
            headingLayout.Controls.Add(ClearButton, 1, 0);
        }
        layout.Controls.Add(headingLayout, 0, 0);

        editorHost = new RoundedPanel();
        editorHost.Radius = 9;
        editorHost.Dock = DockStyle.Fill;
        editorHost.Margin = new Padding(0);
        editorHost.Padding = new Padding(13, 10, 11, 10);
        layout.Controls.Add(editorHost, 0, 1);

        Editor = new RichTextBox();
        Editor.BorderStyle = BorderStyle.None;
        Editor.DetectUrls = false;
        Editor.HideSelection = false;
        Editor.WordWrap = true;
        Editor.ScrollBars = RichTextBoxScrollBars.Vertical;
        Editor.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
        Editor.Dock = DockStyle.Fill;
        Editor.ReadOnly = readOnly;
        Editor.AcceptsTab = false;
        Editor.ShortcutsEnabled = true;
        editorHost.Controls.Add(Editor);

        editorMenu = new ContextMenuStrip();
        editorMenu.ShowImageMargin = false;
        cutItem = new ToolStripMenuItem("Recortar");
        copyItem = new ToolStripMenuItem("Copiar");
        pasteItem = new ToolStripMenuItem("Colar");
        selectAllItem = new ToolStripMenuItem("Selecionar tudo");
        // Display native shortcuts without registering form-wide menu shortcuts.
        cutItem.ShortcutKeyDisplayString = "Ctrl+X";
        copyItem.ShortcutKeyDisplayString = "Ctrl+C";
        pasteItem.ShortcutKeyDisplayString = "Ctrl+V";
        selectAllItem.ShortcutKeyDisplayString = "Ctrl+A";
        cutItem.Click += delegate { if (!Editor.ReadOnly && Editor.SelectionLength > 0) Editor.Cut(); };
        copyItem.Click += delegate { if (Editor.SelectionLength > 0) Editor.Copy(); };
        pasteItem.Click += delegate { if (!Editor.ReadOnly) Editor.Paste(); };
        selectAllItem.Click += delegate { Editor.Focus(); Editor.SelectAll(); };
        editorMenu.Items.AddRange(new ToolStripItem[] { cutItem, copyItem, pasteItem, selectAllItem });
        editorMenu.Opening += delegate
        {
            // Focusing preserves the existing selection, including a right-click on the placeholder.
            Editor.Focus();
            cutItem.Visible = pasteItem.Visible = !Editor.ReadOnly;
            cutItem.Enabled = !Editor.ReadOnly && Editor.SelectionLength > 0;
            copyItem.Enabled = Editor.SelectionLength > 0;
            bool canPaste = false;
            try { canPaste = Clipboard.ContainsText(); }
            catch (System.Runtime.InteropServices.ExternalException) { }
            pasteItem.Enabled = !Editor.ReadOnly && canPaste;
            selectAllItem.Enabled = Editor.TextLength > 0;
        };
        Editor.ContextMenuStrip = editorMenu;
        editorHost.ContextMenuStrip = editorMenu;

        placeholder = new Label();
        placeholder.AutoSize = true;
        placeholder.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        placeholder.BackColor = Color.Transparent;
        placeholder.Location = new Point(15, 13);
        placeholder.Visible = false;
        placeholder.Cursor = Cursors.IBeam;
        placeholder.ContextMenuStrip = editorMenu;
        placeholder.Click += delegate { Editor.Focus(); };
        editorHost.Controls.Add(placeholder);
        placeholder.BringToFront();

        Editor.TextChanged += delegate { UpdatePlaceholder(); };
        Editor.Enter += delegate { UpdatePlaceholder(); };
        Editor.Leave += delegate { UpdatePlaceholder(); };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && editorMenu != null) editorMenu.Dispose();
        base.Dispose(disposing);
    }

    public void SetPlaceholder(string text)
    {
        placeholder.Text = text ?? "";
        UpdatePlaceholder();
    }

    private void UpdatePlaceholder()
    {
        if (placeholder == null || Editor == null) return;
        placeholder.Visible = placeholder.Text.Length > 0 && Editor.TextLength == 0 && !Editor.Focused;
    }

    public void ApplyPalette(UiPalette p)
    {
        FillColor = p.Card;
        BorderColor = p.Border;
        title.ForeColor = p.Text;
        if (ClearButton != null) ClearButton.SetPalette(p.DangerButton, p.DangerText, p.DangerBorder);
        if (placeholder != null) placeholder.ForeColor = p.Muted;
        editorHost.FillColor = p.Editor;
        editorHost.BorderColor = p.EditorBorder;
        Editor.BackColor = p.Editor;
        Editor.ForeColor = p.Text;
        editorMenu.BackColor = p.Editor;
        editorMenu.ForeColor = p.Text;
        editorMenu.Renderer = new EditorMenuRenderer(p);
        try
        {
            if (Editor.IsHandleCreated)
                NativeBridgeApi.SetWindowTheme(Editor.Handle, p.IsDark ? "DarkMode_Explorer" : "Explorer", null);
        }
        catch { }
        Invalidate();
        editorHost.Invalidate();
        Editor.Invalidate();
    }
}

public sealed class ModernDepoimentoForm : Form
{
    private OriginalAppBridge bridge;
    private TextBox modelPath;
    private RoundedPanel modelPathHost;
    private RoundedPanel modelCard;
    private Label modelLabel;
    private Label appTitle;
    private Label subtitle;
    private Panel header;
    private TableLayoutPanel root;
    private SectionCard originalCard;
    private SectionCard reformulatedCard;
    private SectionCard combinedCard;
    private ModernButton selectModel;
    private ModernButton loadModel;
    private ModernButton reformulate;
    private ModernButton cancel;
    private ModernButton addButton;
    private ModernButton copyButton;
    private ModernButton saveButton;
    private ModernButton exportButton;
    private ModernButton clearButton;
    private ModernButton logButton;
    private Label status;
    private ModernProgressBar progress;
    private Timer syncTimer;
    private Stopwatch localTimer = new Stopwatch();
    private Stopwatch modelLoadTimer = new Stopwatch();
    private bool generationWasRunning;
    private bool generationStarting;
    private bool cancelRequested;
    private bool modelWasLoading;
    private bool modelStarting;
    private bool syncInProgress;
    private bool closing;
    private string lastOutput = "";
    private FlowLayoutPanel actions;
    private Panel generation;
    private ThemedComboBox themePicker;
    private Label themeLabel;
    private ThemeChoice themeChoice;
    private UiPalette palette;
    private bool applyingThemePicker;
    private bool lastResolvedDark;
    private int themePollTicks;
    private string backendDescription = "";
    private Timer autosaveTimer;
    private string lastSavedCombined = "";
    private bool autosaveReady;

    public ModernDepoimentoForm(OriginalAppBridge appBridge, string iconPath, string backendDescription)
    {
        bridge = appBridge;
        this.backendDescription = String.IsNullOrWhiteSpace(backendDescription) ? "" : backendDescription.Trim();
        Text = "Depoimento Local — Windows";
        try
        {
            if (!String.IsNullOrEmpty(iconPath) && File.Exists(iconPath)) Icon = new Icon(iconPath);
        }
        catch { }

        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 720);
        Size = new Size(1280, 860);
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;

        themeChoice = ThemeManager.LoadChoice();
        lastResolvedDark = ThemeManager.ResolveDark(themeChoice);
        palette = UiPalette.Create(lastResolvedDark);
        BackColor = palette.Window;

        BuildUi();
        ApplyTheme();

        Shown += delegate
        {
            InitialSync();
            ApplyTheme();
            ApplyWindowTitleBarTheme(lastResolvedDark);
            // Autosave starts only after the recovery decision, so the initial
            // sync cannot overwrite a copy that has not been offered yet.
            try { OfferRecovery(); }
            finally { autosaveReady = true; }
        };
        FormClosing += Form_Closing;
    }

    private void BuildUi()
    {
        root = new TableLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.Padding = new Padding(22, 14, 22, 14);
        root.ColumnCount = 1;
        root.RowCount = 7;
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
        Controls.Add(root);

        header = new Panel();
        header.Dock = DockStyle.Fill;

        appTitle = new Label();
        appTitle.Text = "Depoimento Local";
        appTitle.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
        appTitle.AutoSize = true;
        appTitle.Location = new Point(2, 0);

        subtitle = new Label();
        subtitle.Text = "Processamento local  •  Nenhum dado enviado à internet";
        subtitle.Font = new Font("Segoe UI", 9.5F);
        subtitle.AutoSize = true;
        subtitle.Location = new Point(4, 34);

        themeLabel = new Label();
        themeLabel.Text = "Tema";
        themeLabel.Font = new Font("Segoe UI", 9.5F);
        themeLabel.AutoSize = true;
        themeLabel.TextAlign = ContentAlignment.MiddleRight;
        themeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        themePicker = new ThemedComboBox();
        themePicker.Items.Add("Sistema");
        themePicker.Items.Add("Claro");
        themePicker.Items.Add("Escuro");
        themePicker.Width = 116;
        themePicker.Height = 29;
        themePicker.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        applyingThemePicker = true;
        themePicker.SelectedIndex = (int)themeChoice;
        applyingThemePicker = false;
        themePicker.SelectedIndexChanged += ThemePicker_SelectedIndexChanged;

        header.Controls.Add(appTitle);
        header.Controls.Add(subtitle);
        header.Controls.Add(themeLabel);
        header.Controls.Add(themePicker);
        header.Resize += delegate { LayoutHeaderTheme(); };
        root.Controls.Add(header, 0, 0);

        modelCard = new RoundedPanel();
        modelCard.Dock = DockStyle.Fill;
        modelCard.Margin = new Padding(0, 0, 0, 7);
        modelCard.Padding = new Padding(16, 12, 16, 11);

        TableLayoutPanel modelGrid = new TableLayoutPanel();
        modelGrid.Dock = DockStyle.Fill;
        modelGrid.ColumnCount = 4;
        modelGrid.RowCount = 1;
        modelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        modelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
        modelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        modelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        modelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        modelLabel = new Label();
        modelLabel.Text = "Modelo GGUF";
        modelLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        modelLabel.Dock = DockStyle.Fill;
        modelLabel.TextAlign = ContentAlignment.MiddleLeft;
        modelLabel.BackColor = Color.Transparent;
        modelLabel.Margin = new Padding(6, 0, 8, 0);

        Panel modelLabelHost = new Panel();
        modelLabelHost.Dock = DockStyle.Fill;
        modelLabelHost.BackColor = Color.Transparent;
        modelLabelHost.Margin = new Padding(0);
        modelLabelHost.Padding = new Padding(6, 0, 0, 0);
        modelLabelHost.Controls.Add(modelLabel);

        modelPathHost = new RoundedPanel();
        modelPathHost.Radius = 8;
        modelPathHost.Dock = DockStyle.Fill;
        modelPathHost.Margin = new Padding(0, 3, 12, 3);
        modelPathHost.Padding = new Padding(10, 8, 8, 7);

        modelPath = new TextBox();
        modelPath.ReadOnly = true;
        modelPath.BorderStyle = BorderStyle.None;
        modelPath.Dock = DockStyle.Fill;
        modelPath.Font = new Font("Segoe UI", 9.75F);
        modelPathHost.Controls.Add(modelPath);

        selectModel = SecondaryButton("Selecionar GGUF");
        selectModel.Margin = new Padding(0, 1, 10, 1);
        selectModel.Click += SelectModel_Click;

        loadModel = SecondaryButton("Carregar modelo");
        loadModel.Margin = new Padding(0, 1, 0, 1);
        loadModel.Click += LoadModel_Click;

        modelGrid.BackColor = Color.Transparent;
        modelGrid.Controls.Add(modelLabel, 0, 0);
        modelGrid.Controls.Add(modelPathHost, 1, 0);
        modelGrid.Controls.Add(selectModel, 2, 0);
        modelGrid.Controls.Add(loadModel, 3, 0);
        modelCard.Controls.Add(modelGrid);
        root.Controls.Add(modelCard, 0, 1);

        originalCard = new SectionCard("Resposta / transcrição original", false, true);
        originalCard.ClearButton.Click += ClearOriginal_Click;
        originalCard.Dock = DockStyle.Fill;
        root.Controls.Add(originalCard, 0, 2);

        generation = new Panel();
        generation.Dock = DockStyle.Fill;

        reformulate = new ModernButton("Reformular texto", palette.Blue, Color.White, Color.Transparent);
        reformulate.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        reformulate.Location = new Point(0, 7);
        reformulate.Click += Reformulate_Click;

        cancel = SecondaryButton("Cancelar");
        cancel.Location = new Point(170, 7);
        cancel.Click += Cancel_Click;

        progress = new ModernProgressBar();
        progress.Location = new Point(325, 21);
        progress.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        progress.Width = 360;

        status = new Label();
        status.Text = "Pronto para reformular.";
        status.Font = new Font("Segoe UI", 9.5F);
        status.AutoEllipsis = true;
        status.TextAlign = ContentAlignment.MiddleLeft;
        status.Location = new Point(710, 5);
        status.Height = 38;
        status.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        generation.Controls.Add(reformulate);
        generation.Controls.Add(cancel);
        generation.Controls.Add(progress);
        generation.Controls.Add(status);
        generation.Resize += delegate
        {
            int available = generation.ClientSize.Width;
            int progressLeft = Math.Max(315, reformulate.Right + cancel.Width + 35);
            int statusWidth = Math.Max(200, Math.Min(430, available / 3));
            int baseProgWidth = Math.Max(160, available - progressLeft - statusWidth - 28);
            int progWidth = Math.Max(160, (int)Math.Round(baseProgWidth * 0.86));
            progress.Left = progressLeft;
            progress.Width = progWidth;
            status.Left = progress.Right + 20;
            status.Width = Math.Max(120, available - status.Left);
        };
        root.Controls.Add(generation, 0, 3);

        reformulatedCard = new SectionCard("Texto reformulado", true, true);
        reformulatedCard.ClearButton.Click += ClearReformulated_Click;
        reformulatedCard.Dock = DockStyle.Fill;
        root.Controls.Add(reformulatedCard, 0, 4);

        actions = new FlowLayoutPanel();
        actions.Dock = DockStyle.Fill;
        actions.WrapContents = false;
        actions.FlowDirection = FlowDirection.LeftToRight;
        actions.Padding = new Padding(0, 5, 0, 3);

        addButton = SecondaryButton("Adicionar ao depoimento");
        copyButton = SecondaryButton("Copiar");
        saveButton = SecondaryButton("Salvar rascunho");
        exportButton = SecondaryButton("Exportar Word");
        clearButton = new ModernButton("Limpar consolidado", palette.DangerButton, palette.DangerText, palette.DangerBorder);
        logButton = SecondaryButton("Abrir log");

        addButton.Click += Add_Click;
        copyButton.Click += Copy_Click;
        saveButton.Click += Save_Click;
        exportButton.Click += Export_Click;
        clearButton.Click += Clear_Click;
        logButton.Click += async delegate { await UpdateBridgeAsync(delegate { bridge.Click("Abrir log"); }); };

        actions.Controls.Add(addButton);
        actions.Controls.Add(copyButton);
        actions.Controls.Add(saveButton);
        actions.Controls.Add(exportButton);
        actions.Controls.Add(clearButton);
        actions.Controls.Add(logButton);
        root.Controls.Add(actions, 0, 5);

        combinedCard = new SectionCard("Depoimento consolidado", false);
        combinedCard.Dock = DockStyle.Fill;
        combinedCard.SetPlaceholder("Os trechos adicionados aparecerão aqui.");
        combinedCard.Editor.TextChanged += async delegate
        {
            string text = combinedCard.Editor.Text;
            if (bridge != null && combinedCard.Editor.Focused)
                await UpdateBridgeAsync(delegate { bridge.CombinedText = text; });
        };
        combinedCard.Editor.TextChanged += delegate { ScheduleAutosave(); };
        root.Controls.Add(combinedCard, 0, 6);

        autosaveTimer = new Timer();
        autosaveTimer.Interval = 1500;
        autosaveTimer.Tick += delegate
        {
            autosaveTimer.Stop();
            WriteAutosave();
        };

        syncTimer = new Timer();
        syncTimer.Interval = 350;
        syncTimer.Tick += SyncTimer_Tick;
        syncTimer.Start();

        LayoutHeaderTheme();
    }

    private void LayoutHeaderTheme()
    {
        if (header == null || themePicker == null || themeLabel == null) return;
        themePicker.Left = Math.Max(0, header.ClientSize.Width - themePicker.Width - 4);
        themePicker.Top = 10;
        themeLabel.Left = Math.Max(0, themePicker.Left - themeLabel.Width - 9);
        themeLabel.Top = 15;
    }

    private ModernButton SecondaryButton(string text)
    {
        return new ModernButton(text, palette.SecondaryButton, palette.SecondaryButtonText, palette.SecondaryButtonBorder);
    }

    private void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (applyingThemePicker || themePicker.SelectedIndex < 0) return;
        themeChoice = (ThemeChoice)themePicker.SelectedIndex;
        ThemeManager.SaveChoice(themeChoice);
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        bool dark = ThemeManager.ResolveDark(themeChoice);
        lastResolvedDark = dark;
        palette = UiPalette.Create(dark);

        BackColor = palette.Window;
        if (root != null) root.BackColor = palette.Window;
        if (header != null) header.BackColor = palette.Window;
        if (generation != null) generation.BackColor = palette.Window;
        if (actions != null) actions.BackColor = palette.Window;

        if (appTitle != null) appTitle.ForeColor = palette.Text;
        if (subtitle != null) subtitle.ForeColor = palette.Muted;
        if (themeLabel != null) themeLabel.ForeColor = palette.Muted;

        if (themePicker != null)
        {
            themePicker.SetPalette(palette);
        }

        if (modelCard != null)
        {
            modelCard.FillColor = palette.Card;
            modelCard.BorderColor = palette.Border;
            modelCard.Invalidate();
        }

        if (modelLabel != null) modelLabel.ForeColor = palette.Text;

        if (modelPathHost != null)
        {
            modelPathHost.FillColor = palette.Editor;
            modelPathHost.BorderColor = palette.EditorBorder;
            modelPathHost.Invalidate();
        }

        if (modelPath != null)
        {
            modelPath.BackColor = palette.Editor;
            modelPath.ForeColor = palette.Text;
        }

        if (originalCard != null) originalCard.ApplyPalette(palette);
        if (reformulatedCard != null) reformulatedCard.ApplyPalette(palette);
        if (combinedCard != null) combinedCard.ApplyPalette(palette);

        if (status != null) status.ForeColor = palette.Muted;
        if (progress != null) progress.SetPalette(palette.Window, palette.ProgressTrack, palette.Blue);

        if (reformulate != null) reformulate.SetPalette(palette.Blue, Color.White, Color.Transparent);
        ApplySecondaryPalette(selectModel);
        ApplySecondaryPalette(loadModel);
        ApplySecondaryPalette(cancel);
        ApplySecondaryPalette(addButton);
        ApplySecondaryPalette(copyButton);
        ApplySecondaryPalette(saveButton);
        ApplySecondaryPalette(exportButton);
        ApplySecondaryPalette(logButton);

        if (clearButton != null) clearButton.SetPalette(palette.DangerButton, palette.DangerText, palette.DangerBorder);

        ApplyWindowTitleBarTheme(dark);
        Invalidate(true);
    }

    private void ApplySecondaryPalette(ModernButton b)
    {
        if (b == null) return;
        b.SetPalette(palette.SecondaryButton, palette.SecondaryButtonText, palette.SecondaryButtonBorder);
    }

    private void ApplyWindowTitleBarTheme(bool dark)
    {
        try
        {
            int useDark = dark ? 1 : 0;
            int result = NativeBridgeApi.DwmSetWindowAttribute(Handle, 20, ref useDark, 4);
            if (result != 0)
            {
                NativeBridgeApi.DwmSetWindowAttribute(Handle, 19, ref useDark, 4);
            }
        }
        catch { }
    }

    private void InitialSync()
    {
        modelPath.Text = bridge.ModelPath;
        originalCard.Editor.Text = bridge.OriginalText;
        reformulatedCard.Editor.Text = bridge.ReformulatedText;
        combinedCard.Editor.Text = bridge.CombinedText;
        cancel.Enabled = false;
        string s = bridge.StatusText;
        if (!String.IsNullOrWhiteSpace(s)) status.Text = BeautifyStatus(s, bridge.TimerText);
    }

    private async void SelectModel_Click(object sender, EventArgs e)
    {
        if (generationWasRunning || modelWasLoading) return;
        using (OpenFileDialog dlg = new OpenFileDialog())
        {
            dlg.Filter = "Modelo GGUF (*.gguf)|*.gguf|Todos os arquivos (*.*)|*.*";
            dlg.Title = "Selecione o modelo GGUF";
            if (!String.IsNullOrWhiteSpace(modelPath.Text))
            {
                try { dlg.InitialDirectory = Path.GetDirectoryName(modelPath.Text); } catch { }
            }
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                modelPath.Text = dlg.FileName;
                bridge.InvalidateModel();
                reformulate.Enabled = false;
                string selected = dlg.FileName;
                await UpdateBridgeAsync(delegate { bridge.ModelPath = selected; });
                if (closing) return;
                status.Text = "Modelo selecionado. Clique em Carregar modelo.";
            }
        }
    }

    private async void LoadModel_Click(object sender, EventArgs e)
    {
        if (generationWasRunning || modelWasLoading) return;
        bridge.InvalidateModel();
        reformulate.Enabled = false;
        if (String.IsNullOrWhiteSpace(modelPath.Text) || !File.Exists(modelPath.Text))
        {
            MessageBox.Show(this, "Selecione um arquivo GGUF válido.", "Depoimento Local", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string path = modelPath.Text;
        modelWasLoading = true;
        modelStarting = true;
        modelLoadTimer.Restart();
        loadModel.Enabled = false;
        selectModel.Enabled = false;
        status.Text = "Carregando modelo local...";
        try
        {
            bool dispatched = await bridge.InvokeAsync(delegate
            {
                bridge.ModelPath = path;
                return bridge.Click("Carregar modelo");
            });
            if (closing) return;
            if (!dispatched) throw new InvalidOperationException("O motor local não está pronto para carregar o modelo.");
        }
        catch (Exception ex) { if (!closing) HandleBridgeFailure(ex); }
        finally { modelStarting = false; }
    }

    private async void Reformulate_Click(object sender, EventArgs e)
    {
        // This guard must run before any IPC, state transition or LLM request.
        if (!bridge.ModelReady || modelWasLoading)
        {
            status.Text = "Carregue o modelo antes de reformular o texto.";
            return;
        }
        if (generationWasRunning) return;
        string src = originalCard.Editor.Text.Trim();
        if (src.Length == 0)
        {
            MessageBox.Show(this, "Cole ou digite a resposta/transcrição original.", "Depoimento Local", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string source = originalCard.Editor.Text;
        // Reserve the operation before yielding so double clicks cannot enqueue it twice.
        generationWasRunning = true;
        cancelRequested = false;
        originalCard.ClearButton.Enabled = false;
        reformulatedCard.ClearButton.Enabled = false;
        generationStarting = true;
        localTimer.Restart();
        reformulate.Enabled = false;
        cancel.Enabled = false;
        loadModel.Enabled = false;
        selectModel.Enabled = false;
        status.Text = "Processando localmente...";
        progress.Value = 2;
        try
        {
            string previousOutput = await bridge.InvokeAsync(delegate
            {
                if (!bridge.ModelReady || !bridge.ButtonEnabled("Reformular"))
                    throw new InvalidOperationException("O motor local ainda não está pronto para reformular.");
                bridge.OriginalText = source;
                string previous = bridge.ReformulatedText;
                if (!bridge.Click("Reformular"))
                    throw new InvalidOperationException("O motor local ainda não está pronto para reformular.");
                return previous;
            });
            if (closing) return;
            lastOutput = previousOutput;
            cancel.Enabled = true;
        }
        catch (Exception ex)
        {
            if (!closing) HandleBridgeFailure(ex);
        }
        finally
        {
            generationStarting = false;
        }
    }

    private void FinishGeneration()
    {
        generationWasRunning = false;
        cancelRequested = false;
        originalCard.ClearButton.Enabled = true;
        reformulatedCard.ClearButton.Enabled = true;
        localTimer.Stop();
        reformulate.Enabled = bridge.ModelReady && !modelWasLoading;
        cancel.Enabled = false;
        loadModel.Enabled = !modelWasLoading;
        selectModel.Enabled = !modelWasLoading;
    }

    private async void Cancel_Click(object sender, EventArgs e)
    {
        if (!bridge.ModelReady || !generationWasRunning || generationStarting) return;
        cancel.Enabled = false;
        cancelRequested = true;
        try
        {
            if (await bridge.InvokeAsync(delegate { return bridge.Click("Cancelar"); })) cancelRequested = false;
        }
        catch (Exception ex) { if (!closing) HandleBridgeFailure(ex); }
    }

    private async Task UpdateBridgeAsync(Action action)
    {
        try { await bridge.InvokeAsync(delegate { action(); return true; }); }
        catch (Exception ex) { if (!closing) HandleBridgeFailure(ex); }
    }

    private void HandleBridgeFailure(Exception ex)
    {
        bridge.InvalidateModel();
        modelWasLoading = false;
        modelLoadTimer.Stop();
        FinishGeneration();
        status.Text = ex.Message;
    }

    private sealed class EngineSnapshot
    {
        public string Status, Timer, Output, ModelPath;
        public int Progress;
        public bool LoadFinished, Ready, GenerationFinished, CancellationSent;
    }

    private async void SyncTimer_Tick(object sender, EventArgs e)
    {
        if (closing || syncInProgress || modelStarting || generationStarting) return;
        syncInProgress = true;
        try
        {
            themePollTicks++;
            if (themeChoice == ThemeChoice.System && themePollTicks >= 6)
            {
                themePollTicks = 0;
                bool nowDark = ThemeManager.ResolveDark(themeChoice);
                if (nowDark != lastResolvedDark) ApplyTheme();
            }

            if (modelWasLoading)
            {
                EngineSnapshot snapshot = await bridge.InvokeAsync(delegate
                {
                    EngineSnapshot s = new EngineSnapshot();
                    s.Status = bridge.StatusText;
                    s.Timer = bridge.TimerText;
                    if (s.Status.StartsWith("Falha ao carregar", StringComparison.OrdinalIgnoreCase))
                        bridge.AcknowledgeModelLoadError();
                    s.LoadFinished = modelLoadTimer.ElapsedMilliseconds > 700 && bridge.ButtonEnabled("Carregar modelo");
                    if (s.LoadFinished)
                    {
                        s.Ready = bridge.ConfirmModelLoaded();
                        s.ModelPath = bridge.ModelPath;
                    }
                    return s;
                });
                if (closing) return;
                if (!String.IsNullOrWhiteSpace(snapshot.Status)) status.Text = BeautifyStatus(snapshot.Status, snapshot.Timer);
                if (snapshot.LoadFinished)
                {
                    modelWasLoading = false;
                    modelLoadTimer.Stop();
                    loadModel.Enabled = true;
                    selectModel.Enabled = true;
                    modelPath.Text = snapshot.ModelPath;
                    reformulate.Enabled = snapshot.Ready;
                }
            }

            if (generationWasRunning && !generationStarting)
            {
                bool requestCancellation = cancelRequested;
                EngineSnapshot snapshot = await bridge.InvokeAsync(delegate
                {
                    EngineSnapshot s = new EngineSnapshot();
                    // A user can cancel before the posted generation command has
                    // enabled the engine's Cancel button. Retry that request here.
                    s.CancellationSent = requestCancellation && bridge.Click("Cancelar");
                    s.GenerationFinished = localTimer.ElapsedMilliseconds > 700 && bridge.ButtonEnabled("Reformular");
                    s.Progress = bridge.ProgressValue;
                    s.Output = bridge.ReformulatedText;
                    s.Status = bridge.StatusText;
                    s.Timer = bridge.TimerText;
                    return s;
                });
                if (closing) return;
                if (snapshot.CancellationSent) cancelRequested = false;
                if (snapshot.Progress > 0) progress.Value = snapshot.Progress;
                if (!String.IsNullOrEmpty(snapshot.Output) && snapshot.Output != lastOutput) reformulatedCard.Editor.Text = snapshot.Output;
                if (!String.IsNullOrWhiteSpace(snapshot.Status)) status.Text = BeautifyStatus(snapshot.Status, snapshot.Timer);
                if (snapshot.GenerationFinished)
                {
                    FinishGeneration();
                    progress.Value = 100;
                    reformulatedCard.Editor.Text = snapshot.Output;
                }
            }
        }
        catch (Exception ex)
        {
            if (!closing) HandleBridgeFailure(ex);
        }
        finally { syncInProgress = false; }
    }

    private string BeautifyStatus(string raw, string timer)
    {
        if (String.IsNullOrWhiteSpace(raw)) return "Pronto.";

        if (raw.StartsWith("Modelo carregado", StringComparison.OrdinalIgnoreCase))
        {
            Match th = Regex.Match(raw, @"(\d+)\s*threads?", RegexOptions.IgnoreCase);
            string threads = th.Success ? "  •  " + th.Groups[1].Value + " threads CPU" : "";
            if (!String.IsNullOrWhiteSpace(backendDescription))
                return "Modelo carregado  •  " + backendDescription + threads;
        }

        Match m = Regex.Match(raw, @"Conclu[ií]do em\s+(\d+)\s+bloco", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            string t = "";
            Match tm = Regex.Match(timer ?? "", @"(\d{2}:\d{2})");
            if (tm.Success) t = "  •  " + tm.Groups[1].Value;
            return "✓  Concluído  •  " + m.Groups[1].Value + (m.Groups[1].Value == "1" ? " bloco" : " blocos") + t;
        }
        if (raw.StartsWith("Selecione ", StringComparison.OrdinalIgnoreCase)) return "Pronto para carregar o modelo.";
        return raw;
    }

    private async void Add_Click(object sender, EventArgs e)
    {
        string text = reformulatedCard.Editor.Text.Trim();
        if (text.Length == 0) return;
        string current = combinedCard.Editor.Text;
        if (current.Length > 0 && !current.EndsWith(Environment.NewLine)) current += Environment.NewLine + Environment.NewLine;
        current += text;
        combinedCard.Editor.Text = current;
        await UpdateBridgeAsync(delegate { bridge.CombinedText = current; });
        if (!closing) status.Text = "Trecho adicionado ao depoimento consolidado.";
    }

    private void Copy_Click(object sender, EventArgs e)
    {
        string text = combinedCard.Editor.Text.Trim();
        if (text.Length > 0)
        {
            Clipboard.SetText(text);
            status.Text = "Depoimento consolidado copiado.";
        }
    }

    private async void ClearOriginal_Click(object sender, EventArgs e)
    {
        if (generationWasRunning) return;
        try
        {
            originalCard.Editor.Clear();
            originalCard.Editor.Focus();
            await bridge.InvokeAsync(delegate { bridge.OriginalText = ""; return true; });
            if (closing) return;
            status.Text = "Resposta / transcrição original limpa.";
        }
        catch (Exception ex) { if (!closing) HandleBridgeFailure(ex); }
    }

    private async void ClearReformulated_Click(object sender, EventArgs e)
    {
        if (generationWasRunning) return;
        try
        {
            lastOutput = "";
            reformulatedCard.Editor.Clear();
            progress.Value = 0;
            await bridge.InvokeAsync(delegate { bridge.ReformulatedText = ""; return true; });
            if (closing) return;
            status.Text = "Texto reformulado limpo.";
        }
        catch (Exception ex) { if (!closing) HandleBridgeFailure(ex); }
    }

    private async void Clear_Click(object sender, EventArgs e)
    {
        if (combinedCard.Editor.Text.Trim().Length > 0)
        {
            string msg = HasUnsavedChanges()
                ? "O depoimento consolidado ainda não foi salvo.\n\nDeseja realmente apagá-lo? Essa ação não pode ser desfeita."
                : "Deseja realmente limpar o depoimento consolidado?";
            DialogResult r = MessageBox.Show(this, msg, "Limpar consolidado",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (r != DialogResult.Yes) return;
        }
        combinedCard.Editor.Clear();
        lastSavedCombined = "";
        if (autosaveTimer != null) autosaveTimer.Stop();
        AutosaveStore.Delete();
        await UpdateBridgeAsync(delegate { bridge.CombinedText = ""; });
        if (!closing) status.Text = "Depoimento consolidado limpo.";
    }

    private void Save_Click(object sender, EventArgs e)
    {
        string text = combinedCard.Editor.Text.Trim();
        if (text.Length == 0)
        {
            MessageBox.Show(this, "O depoimento consolidado está vazio.", "Depoimento Local", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (SaveCombinedToFile(false)) status.Text = "Rascunho salvo.";
    }

    private void OfferRecovery()
    {
        try
        {
            string path = AutosaveStore.FilePath;
            if (!File.Exists(path)) return;
            string saved = File.ReadAllText(path, Encoding.UTF8);
            if (saved.Trim().Length == 0)
            {
                AutosaveStore.Delete();
                return;
            }
            if (saved.Trim() == combinedCard.Editor.Text.Trim()) return;

            DateTime when = File.GetLastWriteTime(path);
            DialogResult r = MessageBox.Show(this,
                "Foi encontrado um depoimento consolidado que não foi salvo na última sessão (" +
                when.ToString("dd/MM/yyyy 'às' HH:mm") + ").\n\nDeseja recuperá-lo?\n\n" +
                "Se escolher «Não», esse texto será descartado.",
                "Recuperar depoimento", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                combinedCard.Editor.Text = saved;
                var sync = UpdateBridgeAsync(delegate { bridge.CombinedText = saved; });
                status.Text = "Depoimento recuperado da última sessão. Lembre-se de salvá-lo.";
            }
            else
            {
                AutosaveStore.Delete();
            }
        }
        catch (Exception ex)
        {
            status.Text = "Não foi possível verificar a recuperação: " + ex.Message;
        }
    }

    private void ScheduleAutosave()
    {
        if (!autosaveReady || autosaveTimer == null) return;
        autosaveTimer.Stop();
        autosaveTimer.Start();
    }

    private void WriteAutosave()
    {
        try
        {
            string text = combinedCard.Editor.Text;
            string trimmed = text.Trim();
            // Nothing to protect: empty, or identical to what the user last saved.
            if (trimmed.Length == 0 || trimmed == lastSavedCombined) AutosaveStore.Delete();
            else AutosaveStore.Write(text);
        }
        catch (Exception ex)
        {
            if (!generationWasRunning && !closing) status.Text = "Falha no salvamento automático: " + ex.Message;
        }
    }

    private bool HasUnsavedChanges()
    {
        string t = combinedCard.Editor.Text.Trim();
        return t.Length > 0 && t != lastSavedCombined;
    }

    private void MarkCombinedSaved(string savedText)
    {
        lastSavedCombined = (savedText ?? "").Trim();
        if (autosaveTimer != null) autosaveTimer.Stop();
        WriteAutosave();
    }

    private void Form_Closing(object sender, FormClosingEventArgs e)
    {
        if (autosaveTimer != null) autosaveTimer.Stop();

        // Windows shutdown or forced end: no questions, keep the recovery copy.
        bool systemClosing = e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing;
        if (systemClosing)
        {
            if (HasUnsavedChanges()) WriteAutosave();
        }
        else if (HasUnsavedChanges())
        {
            DialogResult r = MessageBox.Show(this,
                "O depoimento consolidado não foi salvo.\n\nDeseja salvá-lo antes de sair?",
                "Depoimento Local", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (r == DialogResult.Cancel || (r == DialogResult.Yes && !SaveCombinedToFile(true)))
            {
                // The window stays open and the engine keeps running.
                e.Cancel = true;
                WriteAutosave();
                return;
            }
            if (r == DialogResult.No) AutosaveStore.Delete();
        }
        else
        {
            AutosaveStore.Delete();
        }

        // Only after the decision: stop syncing and end the engine.
        closing = true;
        syncTimer.Stop();
        bridge.Stop();
    }

    private bool SaveCombinedToFile(bool offerWord)
    {
        string text = combinedCard.Editor.Text.Trim();
        if (text.Length == 0) return true;

        using (SaveFileDialog dlg = new SaveFileDialog())
        {
            if (offerWord)
            {
                dlg.Filter = "Documento do Word (*.docx)|*.docx|Texto (*.txt)|*.txt";
                dlg.FileName = "depoimento.docx";
            }
            else
            {
                dlg.Filter = "Texto (*.txt)|*.txt";
                dlg.FileName = "depoimento-rascunho.txt";
            }
            if (dlg.ShowDialog(this) != DialogResult.OK) return false;

            try
            {
                if (dlg.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                    SimpleDocx.Write(dlg.FileName, text);
                else
                    File.WriteAllText(dlg.FileName, text, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Erro ao salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MarkCombinedSaved(text);
            return true;
        }
    }

    private void Export_Click(object sender, EventArgs e)
    {
        string text = combinedCard.Editor.Text.Trim();
        if (text.Length == 0)
        {
            MessageBox.Show(this, "O depoimento consolidado está vazio.", "Depoimento Local", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using (SaveFileDialog dlg = new SaveFileDialog())
        {
            dlg.Filter = "Documento do Word (*.docx)|*.docx";
            dlg.FileName = "depoimento.docx";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    SimpleDocx.Write(dlg.FileName, text);
                    MarkCombinedSaved(text);
                    status.Text = "Documento Word exportado.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Erro ao exportar Word", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

// Recovery copy of the consolidated text, outside the program folder.
public static class AutosaveStore
{
    public static string Folder
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DepoimentoLocal", "recuperacao");
        }
    }

    public static string FilePath
    {
        get { return Path.Combine(Folder, "consolidado.txt"); }
    }

    public static void Write(string text)
    {
        Directory.CreateDirectory(Folder);
        string tmp = FilePath + ".tmp";
        File.WriteAllText(tmp, text ?? "", new UTF8Encoding(false));
        if (File.Exists(FilePath))
        {
            try
            {
                File.Replace(tmp, FilePath, null);
                return;
            }
            catch (IOException) { }
            catch (PlatformNotSupportedException) { }
            File.Copy(tmp, FilePath, true);
            File.Delete(tmp);
        }
        else
        {
            File.Move(tmp, FilePath);
        }
    }

    public static void Delete()
    {
        try { if (File.Exists(FilePath)) File.Delete(FilePath); } catch { }
        try { if (File.Exists(FilePath + ".tmp")) File.Delete(FilePath + ".tmp"); } catch { }
    }
}

public static class SimpleDocx
{
    public static void Write(string path, string text)
    {
        if (File.Exists(path)) File.Delete(path);
        using (FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
        using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            Add(zip, "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>" +
                "</Types>");
            Add(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>" +
                "</Relationships>");
            StringBuilder body = new StringBuilder();
            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] lines = normalized.Split('\n');
            foreach (string line in lines)
            {
                body.Append("<w:p><w:r><w:t xml:space=\"preserve\">");
                body.Append(XmlEscape(line));
                body.Append("</w:t></w:r></w:p>");
            }
            Add(zip, "word/document.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>" +
                body.ToString() + "<w:sectPr/></w:body></w:document>");
        }
    }

    private static void Add(ZipArchive zip, string name, string content)
    {
        ZipArchiveEntry e = zip.CreateEntry(name, CompressionLevel.Optimal);
        using (Stream s = e.Open())
        using (StreamWriter w = new StreamWriter(s, new UTF8Encoding(false))) w.Write(content);
    }

    private static string XmlEscape(string s)
    {
        if (s == null) return "";
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
    }
}

public static class ModernShell
{
    public static void Run(Process originalProcess, string iconPath, string backendDescription)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        OriginalAppBridge bridge = new OriginalAppBridge(originalProcess);
        if (!bridge.Connect(12000))
        {
            MessageBox.Show("Não foi possível conectar a nova interface ao motor do Depoimento Local. Use INICIAR-ORIGINAL.bat e envie o arquivo de log se o problema persistir.", "Depoimento Local", MessageBoxButtons.OK, MessageBoxIcon.Error);
            bridge.Stop();
            return;
        }
        using (ModernDepoimentoForm f = new ModernDepoimentoForm(bridge, iconPath, backendDescription)) Application.Run(f);
    }
}
