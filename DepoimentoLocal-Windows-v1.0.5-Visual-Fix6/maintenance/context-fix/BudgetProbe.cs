using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LLama;

public static class BudgetProbe
{
    public static int Main(string[] args)
    {
        try { Run(args[0]).GetAwaiter().GetResult(); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    private static async Task Run(string outputPath)
    {
        outputPath = Path.GetFullPath(outputPath);
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        var app = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "DepoimentoLocal.dll"));
        var type = app.GetType("DepoimentoLocal.Windows.LlmService", true);
        object service = Activator.CreateInstance(type, true);
        try
        {
            string model = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DepoimentoLocal", "last-model.txt")).Trim();
            await (Task)type.GetMethod("LoadAsync").Invoke(service, new object[] { model, null });
            var weights = (LLamaWeights)type.GetField("_weights", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(service);
            var factory = app.GetType("DepoimentoLocal.Windows.PromptFactory", true);
            var guard = app.GetType("DepoimentoLocal.Windows.ContextBudget", true).GetMethod("Validate");
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            string seed = "Não sei quem abriu a porta. Carlos me disse que Paulo gritou, mas eu não ouvi esse grito. Não lembro a hora. ";
            string draftSeed = "Não se lembra quem abriu a porta. Carlos lhe disse que Paulo gritou, mas não ouviu esse grito. Não lembra a hora. ";
            var source = new StringBuilder();
            var draft = new StringBuilder();
            while (source.Length < 1155) source.Append(seed);
            while (draft.Length < 1076) draft.Append(draftSeed);
            string original = source.ToString(0, 1155);
            string firstResponse = draft.ToString(0, 1076);
            string first = (string)factory.GetMethod("Build", flags).Invoke(null, new object[] { original, "Relatou que " });
            string retry = (string)factory.GetMethod("BuildRetry", flags).Invoke(null, new object[] { original, firstResponse, "a saída alterou o sentido de 'não sei'/'não sabe'", "Relatou que " });
            string report = "SYNTHETIC token-budget probe. Matches logged character counts, NOT the original case.\n" +
                "originalChars=1155; firstResponseChars=1076; maxTokens=465\n" +
                "FIRST " + guard.Invoke(null, new object[] { weights, first, 465, 4096 }) + "\n" +
                "RETRY " + guard.Invoke(null, new object[] { weights, retry, 465, 4096 }) + "\n";
            File.WriteAllText(outputPath, report, Encoding.UTF8);
            Console.WriteLine(report);
        }
        finally { ((IDisposable)service).Dispose(); }
    }
}
