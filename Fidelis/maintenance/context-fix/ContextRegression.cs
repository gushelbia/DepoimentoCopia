using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;

public static class ContextRegression
{
    private static Assembly app;
    private static string reportPath;
    private static object Call(string typeName, string name, params object[] args)
    {
        return app.GetType("DepoimentoLocal.Windows." + typeName, true)
            .GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, args);
    }
    private static void Log(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(reportPath, message + Environment.NewLine, Encoding.UTF8);
    }
    private static async Task<string> Generate(object service, string prompt, int maxTokens)
    {
        var task = (Task<string>)service.GetType().GetMethod("GenerateAsync").Invoke(service,
            new object[] { prompt, maxTokens, CancellationToken.None, null });
        return await task;
    }
    public static int Main(string[] args)
    {
        try { Run(args).GetAwaiter().GetResult(); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    private static async Task Run(string[] args)
    {
        string work = Path.GetFullPath(args[0]);
        string fixture = args.Length > 1 ? Path.GetFullPath(args[1]) : null;
        reportPath = Path.Combine(work, fixture == null ? "regression-results.txt" : "exact-text-results.txt");
        File.WriteAllText(reportPath, DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        app = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "DepoimentoLocal.dll"));
        object service = Activator.CreateInstance(app.GetType("DepoimentoLocal.Windows.LlmService", true), true);
        try
        {
            string modelPath = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DepoimentoLocal", "last-model.txt")).Trim();
            await (Task)service.GetType().GetMethod("LoadAsync").Invoke(service, new object[] { modelPath, null });
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var parameters = (ModelParams)service.GetType().GetField("_parameters", flags).GetValue(service);
            var weights = (LLamaWeights)service.GetType().GetField("_weights", flags).GetValue(service);
            var executor = (StatelessExecutor)service.GetType().GetField("_executor", flags).GetValue(service);
            if (parameters.ContextSize != 4096 || parameters.GpuLayerCount != 8) throw new Exception("Unexpected model parameters");
            Log("PARAMETERS PASS context=" + parameters.ContextSize + "; gpuLayers=" + parameters.GpuLayerCount);

            // Match MainForm: trim only surrounding whitespace before splitting/estimating.
            string source = fixture == null ? "Não sei quem abriu a porta. Carlos me disse que Paulo gritou, mas eu não ouvi esse grito. Não lembro o horário." : File.ReadAllText(fixture).Trim();
            Log("FIXTURE=" + (fixture == null ? "synthetic regression, NOT the missing 1155-character original" : fixture) + "; chars=" + source.Length);
            string lead = "Relatou que ";
            int maxTokens = (int)Call("TextProcessing", "EstimateMaxTokens", source);
            string firstPrompt = (string)Call("PromptFactory", "Build", source, lead);
            Log("FIRST " + Call("ContextBudget", "Validate", weights, firstPrompt, maxTokens, 4096));
            string firstRaw = await Generate(service, firstPrompt, maxTokens);
            object firstContext = executor.Context;
            if (!executor.Context.NativeHandle.IsClosed) throw new Exception("First context was not disposed");
            string firstOutput = (string)Call("TextProcessing", "ApplyLead", lead, firstRaw);
            Log("FIRST OUTPUT=" + firstOutput);
            string issue = (string)Call("TextProcessing", "Validate", source, firstOutput);
            Log("FIRST VALIDATION=" + (issue ?? "ok"));

            string retrySource = source;
            string draft = firstOutput;
            if (fixture == null)
            {
                retrySource = "Não sei quem abriu a porta.";
                draft = "Não se lembra quem abriu a porta.";
                issue = (string)Call("TextProcessing", "Validate", retrySource, draft);
                if (issue == null || !issue.Contains("não sei")) throw new Exception("Existing validator missed meaning change");
                Log("VALIDATOR PASS: " + issue);
                maxTokens = (int)Call("TextProcessing", "EstimateMaxTokens", retrySource);
            }
            if (String.IsNullOrEmpty(issue)) issue = "Confira fidelidade sem alterar o sentido";
            string retryPrompt = (string)Call("PromptFactory", "BuildRetry", retrySource, draft, issue, lead);
            if (!retryPrompt.Contains(retrySource.Trim()) || !retryPrompt.Contains(draft.Trim()) || !retryPrompt.Contains(issue)) throw new Exception("Retry lost original, draft or detected issue");
            Log("RETRY " + Call("ContextBudget", "Validate", weights, retryPrompt, maxTokens, 4096));
            string retryRaw = await Generate(service, retryPrompt, maxTokens);
            object retryContext = executor.Context;
            if (Object.ReferenceEquals(firstContext, retryContext) || !executor.Context.NativeHandle.IsClosed) throw new Exception("Retry reused first context or left it open");
            Log("FRESH CONTEXT PASS");
            string retryOutput = (string)Call("TextProcessing", "ApplyLead", lead, retryRaw);
            Log("RETRY OUTPUT=" + retryOutput);
            Log("RETRY VALIDATION=" + ((string)Call("TextProcessing", "Validate", retrySource, retryOutput) ?? "ok"));

            if (fixture != null) { Log("EXACT TEXT CONTEXT PASS (semantic validation reported above)"); return; }

            string again = await Generate(service, retryPrompt, maxTokens);
            if (Object.ReferenceEquals(retryContext, executor.Context) || !executor.Context.NativeHandle.IsClosed) throw new Exception("Repeated retry context was not fresh");
            if (again != retryRaw) throw new Exception("Repeated deterministic retry changed output");
            Log("REPEATED RETRY PASS: fresh context and identical output");

            string hugePrompt = firstPrompt + new string('X', 60000);
            try { await Generate(service, hugePrompt, 465); throw new Exception("Oversize request was silently accepted"); }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Contains("Nenhum conteúdo foi cortado")) throw;
                Log("OVERSIZE REJECTED WITHOUT TRUNCATION: " + ex.Message);
            }
            string afterError = await Generate(service, retryPrompt, maxTokens);
            if (afterError != retryRaw) throw new Exception("Failure contaminated subsequent generation");
            Log("RECOVERY AFTER BUDGET ERROR PASS");
            Log("CONTEXT REGRESSION PASS");
        }
        finally { ((IDisposable)service).Dispose(); }
    }
}
