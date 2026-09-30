using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

// Isolated test of the incomplete-output marker through the real engine UI
// (temporary engine copy) and the production ModernDepoimentoForm.
public static class MarkerTest
{
    const string Head = "[SAÍDA INCOMPLETA E NÃO VALIDADA - NÃO UTILIZAR COMO REFORMULAÇÃO]";
    const string Foot = "[FIM DO TRECHO PARCIAL - a transcrição não foi reformulada por completo]";
    static List<string> results = new List<string>();

    static string ReadShared(string path)
    {
        if (!File.Exists(path)) return "";
        using (var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var r = new StreamReader(s, Encoding.UTF8)) return r.ReadToEnd();
    }

    static string N(string s) { return (s ?? "").Replace("\r\n", "\n"); }

    static int Count(string text, string part)
    {
        int c = 0, i = 0;
        while (text != null && (i = text.IndexOf(part, i, StringComparison.Ordinal)) >= 0) { c++; i += part.Length; }
        return c;
    }

    static void Check(bool ok, string name, string detail)
    {
        results.Add((ok ? "PASS " : "FAIL ") + name + (String.IsNullOrEmpty(detail) ? "" : " | " + detail));
    }

    static void Pump(int ms)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(20); }
    }

    static string WaitFor(Func<string> log, string marker, int before, int seconds)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < seconds)
        {
            string current = log();
            if (Count(current, marker) > before) return current;
            System.Threading.Thread.Sleep(150);
        }
        throw new TimeoutException("Esperando " + marker);
    }

    static string WaitAny(Func<string> log, string before, int seconds)
    {
        string[] ends = { "[GENERATION_CANCELLED]", "[GENERATION_FAILED]", "[GENERATION_OK]" };
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < seconds)
        {
            string current = log();
            foreach (string e in ends) if (Count(current, e) > Count(before, e)) return e;
            System.Threading.Thread.Sleep(150);
        }
        throw new TimeoutException("Esperando fim da geração");
    }

    static void CloseDialogs(int pid)
    {
        NativeBridgeApi.EnumWindows(delegate (IntPtr h, IntPtr l)
        {
            uint owner; NativeBridgeApi.GetWindowThreadProcessId(h, out owner);
            if (owner == pid)
            {
                var cls = new StringBuilder(64); NativeBridgeApi.GetClassName(h, cls, 64);
                if (cls.ToString() == "#32770") NativeBridgeApi.PostMessage(h, 0x0010, IntPtr.Zero, IntPtr.Zero);
            }
            return true;
        }, IntPtr.Zero);
    }

    static Control Editor(object form, string card)
    {
        object c = form.GetType().GetField(card, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
        PropertyInfo p = c.GetType().GetProperty("Editor");
        if (p != null) return (Control)p.GetValue(c, null);
        return (Control)c.GetType().GetField("Editor").GetValue(c);
    }

    public static List<string> Run(string root, string rejectText, string cancelText, string pngPath)
    {
        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DepoimentoLocal", "logs", "depoimento-local.log");
        string prefix = ReadShared(logPath);
        Func<string> log = delegate { string c = ReadShared(logPath); return c.StartsWith(prefix, StringComparison.Ordinal) ? c.Substring(prefix.Length) : c; };
        var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
        start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
        Process process = Process.Start(start);
        var bridge = new OriginalAppBridge(process);
        Form form = null;
        try
        {
            if (!bridge.Connect(15000)) throw new Exception("Sem conexão com o motor de teste.");
            bridge.ModelPath = @"..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf";
            bridge.Click("Carregar modelo");
            var load = Stopwatch.StartNew();
            while (!bridge.ConfirmModelLoaded()) { if (load.Elapsed.TotalSeconds > 180) throw new TimeoutException("modelo"); System.Threading.Thread.Sleep(100); }

            // A) Simulated rejection in block 2 of a multi-block text.
            string before = log();
            bridge.OriginalText = rejectText; bridge.ReformulatedText = "";
            bridge.Click("Reformular");
            string runLog = WaitFor(log, "[GENERATION_FAILED]", Count(before, "[GENERATION_FAILED]"), 600);
            System.Threading.Thread.Sleep(700);
            string rejected = bridge.ReformulatedText;
            CloseDialogs(process.Id); System.Threading.Thread.Sleep(500);
            string tail = runLog.Substring(before.Length);
            Check(tail.Contains("blocks=2") && tail.Contains("[BLOCK_REJECTED] block=2/2"), "A: rejeição simulada no bloco 2 de 2", null);
            Check(rejected.StartsWith(Head) && Count(rejected, Head) == 1, "A: cabeçalho presente uma única vez", null);
            Check(rejected.TrimEnd().EndsWith(Foot), "A: rodapé de trecho parcial presente", null);
            Check(rejected.Contains("Motivo: O bloco 2 ainda precisa de revisão") && rejected.Contains("blocos seguintes não foram processados"), "A: motivo explícito no texto", null);
            Check(rejected.Contains("Ruth"), "A: texto do bloco 1 aceito continua visível dentro do marcador", null);
            File.WriteAllText(Path.ChangeExtension(pngPath, ".rejeicao.txt"), rejected, new UTF8Encoding(false));

            // Copy and Adicionar through the production modern UI.
            var shell = typeof(ModernDepoimentoForm);
            form = (Form)Activator.CreateInstance(shell, new object[] { bridge, null, "teste" });
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.ShowInTaskbar = false;
            form.Show();
            Control reform = Editor(form, "reformulatedCard"), combined = Editor(form, "combinedCard");
            var sync = Stopwatch.StartNew();
            while (N(reform.Text) != N(rejected) && sync.Elapsed.TotalSeconds < 15) Pump(200);
            Check(N(reform.Text) == N(rejected), "UI: editor da interface moderna recebe o texto marcado sem alteração (quebras de linha normalizadas)", null);
            shell.GetMethod("Add_Click", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(form, new object[] { null, EventArgs.Empty });
            Pump(1500);
            Check(N(combined.Text).Contains(N(rejected.Trim())) && Count(combined.Text, Head) == 1 && combined.Text.Contains(Foot), "UI: Adicionar leva cabeçalho, trecho e rodapé ao consolidado", null);
            shell.GetMethod("Copy_Click", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(form, new object[] { null, EventArgs.Empty });
            Pump(500);
            string clip = Clipboard.GetText();
            Check(N(clip) == N(combined.Text.Trim()) && clip.Contains(Head) && clip.Contains(Foot), "UI: Copiar leva o marcador byte a byte à área de transferência", null);
            Check(clip.IndexOf('\uFFFD') < 0 && !System.Text.RegularExpressions.Regex.IsMatch(clip, "Ã[\u0080-\u00BF]") && clip.Contains("SAÍDA") && clip.Contains("REFORMULAÇÃO"), "UI: acentos do marcador preservados na cópia (sem '?' ou caractere de substituição)", null);
            object card = shell.GetField("reformulatedCard", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            var cardControl = (Control)card;
            using (var bmp = new Bitmap(cardControl.Width, cardControl.Height))
            {
                cardControl.DrawToBitmap(bmp, new Rectangle(0, 0, cardControl.Width, cardControl.Height));
                bmp.Save(pngPath, ImageFormat.Png);
            }
            results.Add("INFO tela renderizada em " + pngPath);

            // B) Cancellation after block 1 was accepted.
            before = log();
            bridge.OriginalText = cancelText;
            bridge.Click("Reformular");
            WaitFor(log, "[BLOCK_START] block=2/", Count(before, "[BLOCK_START] block=2/"), 300);
            string duringB = bridge.ReformulatedText;
            Check(Count(duringB, Head) == 0, "B: novo Reformular começa sem o marcador anterior (não acumula)", null);
            bridge.Click("Cancelar");
            string endB = WaitAny(log, before, 300);
            System.Threading.Thread.Sleep(700);
            string cancelled = bridge.ReformulatedText;
            CloseDialogs(process.Id); System.Threading.Thread.Sleep(500);
            results.Add("INFO B: desfecho registrado pelo motor = " + endB + (endB == "[GENERATION_CANCELLED]" ? " (tratador de cancelamento)" : " (tratador de erro)"));
            Check(cancelled.StartsWith(Head) && Count(cancelled, Head) == 1 && cancelled.TrimEnd().EndsWith(Foot), "B: cancelamento com bloco 1 aceito marca o trecho parcial uma única vez", null);
            Check(cancelled.Contains("Ruth"), "B: texto do bloco 1 aceito continua visível dentro do marcador", null);
            Check(endB == "[GENERATION_CANCELLED]" && cancelled.Contains("Motivo: Geração cancelada pelo usuário"), "B: cancelamento no meio do bloco é registrado como cancelamento, não como erro", endB);
            File.WriteAllText(Path.ChangeExtension(pngPath, ".cancelamento.txt"), cancelled, new UTF8Encoding(false));

            // C) Cancellation right after Reformular, before any text: the screen stays empty.
            before = log();
            bridge.Click("Reformular");
            System.Threading.Thread.Sleep(300);
            bridge.Click("Cancelar");
            string endC = WaitAny(log, before, 300);
            System.Threading.Thread.Sleep(700);
            string empty = bridge.ReformulatedText;
            results.Add("INFO C: desfecho registrado pelo motor = " + endC);
            Check(endC == "[GENERATION_CANCELLED]", "C: cancelamento antes de qualquer texto é registrado como cancelamento", endC);
            Check(String.IsNullOrWhiteSpace(empty), "C: tela vazia continua vazia (sem marcador)", "texto='" + empty + "'");
        }        catch (Exception ex) { results.Add("FAIL execução: " + ex); }
        finally
        {
            // The consolidated text holds only test data: empty it so closing does not ask to save.
            if (form != null) { try { Editor(form, "combinedCard").Text = ""; form.Close(); form.Dispose(); } catch { } }
            try { if (!process.HasExited) process.Kill(); } catch { }
        }
        return results;
    }
}
