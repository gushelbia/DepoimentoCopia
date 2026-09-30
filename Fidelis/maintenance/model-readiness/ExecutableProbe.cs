using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public static class ExecutableProbe
{
    private delegate bool EnumProc(IntPtr h, IntPtr data);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr window, EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr window);
    [DllImport("user32.dll")] private static extern int GetDlgCtrlID(IntPtr window);
    [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr window);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr w, IntPtr l);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessageTimeout(IntPtr window, uint message, IntPtr w, StringBuilder l, uint flags, uint timeout, out IntPtr result);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessageTimeout(IntPtr window, uint message, IntPtr w, string l, uint flags, uint timeout, out IntPtr result);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, StringBuilder value, int count);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out Rect rect);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    private static string report;
    private static string Text(IntPtr window)
    {
        var value = new StringBuilder(32768); IntPtr result;
        if (SendMessageTimeout(window, 0xD, new IntPtr(value.Capacity), value, 2, 1000, out result) == IntPtr.Zero)
            throw new Exception("Unresponsive window " + window);
        return value.ToString();
    }
    private static void Log(string message) { Console.WriteLine(message); File.AppendAllText(report, message + Environment.NewLine, Encoding.UTF8); }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static IntPtr Find(IntPtr parent, string text)
    {
        IntPtr found = IntPtr.Zero;
        EnumChildWindows(parent, delegate(IntPtr h, IntPtr d) { if (Text(h).StartsWith(text, StringComparison.Ordinal)) found = h; return true; }, IntPtr.Zero);
        return found;
    }
    private static void Click(IntPtr button)
    {
        Check(button != IntPtr.Zero, "Button missing");
        Check(PostMessage(GetParent(button), 0x111, new IntPtr(GetDlgCtrlID(button) & 0xffff), button), "Post failed");
    }
    private static void Wait(Func<bool> done, int seconds, string action)
    {
        var timer = Stopwatch.StartNew();
        while (!done()) { if (timer.Elapsed.TotalSeconds > seconds) throw new Exception("Timeout " + action); Thread.Sleep(80); }
    }
    public static void Run(string root)
    {
        report = Path.Combine(root, "maintenance", "model-readiness", "executable-results.txt");
        File.WriteAllText(report, "START " + DateTime.Now.ToString("s") + Environment.NewLine);
        var start = new ProcessStartInfo(Path.Combine(root, "Fidelis.exe"));
        start.WorkingDirectory = Path.GetTempPath(); start.UseShellExecute = false; start.CreateNoWindow = true;
        Process process = Process.Start(start); IntPtr window = IntPtr.Zero;
        try
        {
            Wait(delegate
            {
                Check(!process.HasExited, "Executable exited");
                EnumWindows(delegate(IntPtr h, IntPtr d)
                {
                    uint pid; GetWindowThreadProcessId(h, out pid);
                    if (pid == process.Id && Text(h) == "Fidelis") window = h;
                    return true;
                }, IntPtr.Zero);
                return window != IntPtr.Zero;
            }, 30, "startup");
            IntPtr reformulate = Find(window, "Reformular texto");
            IntPtr load = Find(window, "Carregar modelo");
            var editors = new List<IntPtr>();
            EnumChildWindows(window, delegate(IntPtr h, IntPtr d)
            {
                var name = new StringBuilder(256); GetClassName(h, name, name.Capacity);
                if (name.ToString().ToUpperInvariant().Contains("RICHEDIT")) editors.Add(h);
                return true;
            }, IntPtr.Zero);
            editors.Sort(delegate(IntPtr a, IntPtr b) { Rect x, y; GetWindowRect(a, out x); GetWindowRect(b, out y); return x.Top.CompareTo(y.Top); });
            Check(editors.Count == 3, "Expected three text editors");
            IntPtr result;
            Check(SendMessageTimeout(editors[0], 0xC, IntPtr.Zero, "Eu fui ao mercado ontem. Comprei arroz e voltei para casa.", 2, 1000, out result) != IntPtr.Zero, "Input failed");
            Log("PASS executável instalado abriu a partir de outro diretório");
            Click(reformulate);
            Wait(delegate { return Find(window, "Carregue o modelo antes de reformular o texto.") != IntPtr.Zero; }, 5, "unloaded guard");
            Check(Text(window) == "Fidelis" && Text(editors[1]) == "", "Unloaded action did not return safely");
            Log("PASS clique real sem modelo: aviso local, saída vazia, janela responsiva");
            Click(load);
            Wait(delegate { return !IsWindowEnabled(reformulate); }, 3, "loading lock");
            Wait(delegate { Text(window); return Find(window, "Modelo carregado") != IntPtr.Zero && IsWindowEnabled(reformulate); }, 120, "load");
            Log("PASS carga real do GGUF e habilitação do botão");
            Click(reformulate);
            Wait(delegate { return !IsWindowEnabled(reformulate); }, 3, "generation start");
            int checks = 0;
            Wait(delegate { Text(window); checks++; return IsWindowEnabled(reformulate); }, 180, "generation");
            string output = Text(editors[1]);
            Check(!String.IsNullOrWhiteSpace(output), "Empty generation");
            IntPtr completed = Find(window, "✓  Concluído");
            if (completed == IntPtr.Zero) completed = Find(window, "Concluído");
            Check(completed != IntPtr.Zero, "No successful completion status; output=" + output);
            Log("PASS geração real concluída; " + checks + " verificações de resposta da janela; status=" + Text(completed) + "; saída=" + output);
            PostMessage(window, 0x10, IntPtr.Zero, IntPtr.Zero);
            Check(process.WaitForExit(10000), "Close failed");
            Log("ALL PASS executável instalado");
        }
        catch (Exception ex) { Log("FAIL " + ex); throw; }
        finally { if (!process.HasExited) { process.CloseMainWindow(); if (!process.WaitForExit(3000)) process.Kill(); } process.Dispose(); }
    }
}
