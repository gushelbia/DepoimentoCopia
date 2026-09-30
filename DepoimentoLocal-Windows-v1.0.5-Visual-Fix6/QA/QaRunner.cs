// Compiled together with the unmodified production ModernShell.cs.
// Calls the same OriginalAppBridge used by Reformulate_Click. No LLM implementation here.
public sealed class QaGeneration
{
    public string Output = "";
    public string Log = "";
    public string Exception = "";
    public double GenerationSeconds;
    public double TotalSeconds;
    public bool CompletedNormally;
}

public static class QaRunner
{
    private static string ReadSharedLog(string path)
    {
        if (!File.Exists(path)) return "";
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var reader = new StreamReader(stream, Encoding.UTF8)) return reader.ReadToEnd();
    }

    public static QaGeneration Run(string root, string input, int timeoutSeconds)
    {
        var result = new QaGeneration();
        var total = Stopwatch.StartNew();
        var generation = new Stopwatch();
        Process process = null;
        OriginalAppBridge bridge = null;
        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DepoimentoLocal", "logs", "depoimento-local.log");
        string prefix = ReadSharedLog(logPath);
        Func<string> readRun = delegate {
            string current = ReadSharedLog(logPath);
            if (!current.StartsWith(prefix, StringComparison.Ordinal))
            {
                // AppLog rotates at startup; preserve only this run's newly created file.
                string previous = Path.Combine(Path.GetDirectoryName(logPath), "depoimento-local.previous.log");
                if (File.Exists(previous) && ReadSharedLog(previous) == prefix) return current;
                throw new IOException("Log compartilhado alterado/rotacionado durante o teste; atribuição não confiável.");
            }
            return current.Substring(prefix.Length);
        };
        try
        {
            string engine = Path.Combine(root, "engine");
            var start = new ProcessStartInfo(Path.Combine(engine, "DepoimentoLocal.exe"));
            start.WorkingDirectory = engine;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            process = Process.Start(start);
            bridge = new OriginalAppBridge(process);
            if (!bridge.Connect(15000)) throw new Exception("Não foi possível conectar ao motor do teste.");
            // Same portable GGUF selected by DesktopProgram; no model/config mutations.
            bridge.ModelPath = @"..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf";
            if (!bridge.Click("Carregar modelo")) throw new Exception("Carregamento não enviado.");
            var loading = Stopwatch.StartNew();
            while (!bridge.ConfirmModelLoaded())
            {
                result.Log = readRun();
                if (result.Log.Contains("[MODEL_LOAD_FAILED]")) throw new Exception("Falha no carregamento; consulte o log.");
                if (process.HasExited) throw new Exception("Motor encerrou durante carregamento.");
                if (loading.Elapsed.TotalSeconds > 180) throw new TimeoutException("Carregamento excedeu 180 segundos.");
                System.Threading.Thread.Sleep(100);
            }
            bridge.OriginalText = input.Trim();
            if (bridge.OriginalText.Replace("\r\n", "\n") != input.Trim().Replace("\r\n", "\n"))
                throw new Exception("Entrada no motor difere do caso de teste.");
            bridge.ReformulatedText = "";
            generation.Start();
            if (!bridge.Click("Reformular")) throw new Exception("Reformulação não enviada.");
            while (true)
            {
                result.Log = readRun();
                if (result.Log.Contains("[GENERATION_OK]") || result.Log.Contains("[GENERATION_FAILED]") || result.Log.Contains("[GENERATION_CANCELLED]")) break;
                if (process.HasExited) throw new Exception("Motor encerrou durante geração.");
                if (generation.Elapsed.TotalSeconds > timeoutSeconds) throw new TimeoutException("Geração excedeu " + timeoutSeconds + " segundos.");
                System.Threading.Thread.Sleep(150);
            }
            generation.Stop();
            result.Output = bridge.ReformulatedText;
            result.Log = readRun();
            // Logs have no PID: refuse ambiguous evidence if another app generated concurrently.
            if (System.Text.RegularExpressions.Regex.Matches(result.Log, @"\[GENERATION_START\]").Count != 1 ||
                System.Text.RegularExpressions.Regex.Matches(result.Log, @"\[APP_START\]").Count != 1)
                throw new Exception("Atividade concorrente no log compartilhado; resultado não confiável. Reexecute sem gerar em outra instância.");
            result.CompletedNormally = result.Log.Contains("[GENERATION_OK]");
            if (!result.CompletedNormally) result.Exception = "Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.";
        }
        catch (Exception ex)
        {
            result.Exception = ex.ToString();
            try { result.Log = readRun(); } catch (Exception logError) { result.Exception += "\n" + logError; }
            try { if (bridge != null) result.Output = bridge.ReformulatedText; } catch { }
        }
        finally
        {
            generation.Stop();
            result.GenerationSeconds = generation.Elapsed.TotalSeconds;
            if (bridge != null) bridge.Stop(); // Only the process created by this test.
            if (process != null) { if (!process.HasExited) { process.Kill(); } process.WaitForExit(10000); process.Dispose(); }
            result.TotalSeconds = total.Elapsed.TotalSeconds;
        }
        return result;
    }
}
