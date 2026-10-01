using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Runs the engine's own steps one by one for each sentence and records the text
// after each step: raw model output, lead + repairs, first-person repair, anchors.
// Read-only: loads the engine by reflection; nothing in the engine is changed.
public static class GenderProbe
{
    private static Assembly app;
    private static object Call(string typeName, string name, params object[] args)
    {
        return app.GetType("DepoimentoLocal.Windows." + typeName, true)
            .GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, args);
    }
    public static int Main(string[] args)
    {
        try { Run(args).GetAwaiter().GetResult(); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    private static async Task Run(string[] args)
    {
        string inputs = Path.GetFullPath(args[0]), output = Path.GetFullPath(args[1]);
        string mode = args.Length > 2 ? args[2] : "atual";
        string model = @"..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf";
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        app = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "DepoimentoLocal.dll"));
        object service = Activator.CreateInstance(app.GetType("DepoimentoLocal.Windows.LlmService", true), true);
        var sb = new StringBuilder();
        try
        {
            await (Task)service.GetType().GetMethod("LoadAsync").Invoke(service, new object[] { model, null });
            foreach (string line in File.ReadAllLines(inputs, Encoding.UTF8))
            {
                if (line.Trim().Length == 0) continue;
                string[] parts = line.Split('\t');
                string id = parts[0], source = parts[1].Trim();
                string lead = "Relatou que ";
                int maxTokens = (int)Call("TextProcessing", "EstimateMaxTokens", source);
                string prompt = (string)Call("PromptFactory", "Build", source, lead);
                // Probe only (nothing in the engine changes): feminine variant of the prompt.
                if (mode == "feminino")
                {
                    prompt = prompt.Replace("\"o depoente\"", "\"a depoente\"").Replace("O depoente", "A depoente").Replace("o depoente", "a depoente");
                    prompt = prompt.Replace("Reescreva a transcrição em português,", "A depoente é uma mulher: escreva sempre «a depoente» e ponha no feminino os adjetivos e particípios que se referem a ela (nervosa, agredida, sozinha). Reescreva a transcrição em português,");
                }
                var task = (Task<string>)service.GetType().GetMethod("GenerateAsync").Invoke(service, new object[] { prompt, maxTokens, CancellationToken.None, null });
                string raw = await task;
                string led = (string)Call("TextProcessing", "ApplyLeadAndRepair", lead, raw, source);
                string v1 = (string)Call("TextProcessing", "Validate", source, led);
                string residual = (string)Call("TextProcessing", "RepairResidualFirstPerson", led);
                string anchors = (string)Call("TextProcessing", "RepairSemanticAnchors", source, residual);
                string v2 = (string)Call("TextProcessing", "Validate", source, anchors);
                sb.AppendLine("=== " + id + ": " + source);
                sb.AppendLine("  1 BRUTO DO MODELO   : " + raw.Trim());
                sb.AppendLine("  2 ABERTURA+REPAROS  : " + led + "   [validação: " + (v1 ?? "ok") + "]");
                sb.AppendLine("  3 1ª PESSOA (motor) : " + residual);
                sb.AppendLine("  4 ÂNCORAS (final)   : " + anchors + "   [validação: " + (v2 ?? "ok") + "]");
                Console.WriteLine("=== " + id + " ok");
            }
        }
        finally { ((IDisposable)service).Dispose(); }
        File.WriteAllText(output, sb.ToString(), Encoding.UTF8);
    }
}
