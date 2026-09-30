using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

internal static class DesktopProgram
{
    private static bool SameFile(string first, string second)
    {
        if (!File.Exists(second)) return false;
        using (var hash = SHA256.Create())
        using (var a = File.OpenRead(first))
        using (var b = File.OpenRead(second))
            return Convert.ToHexString(hash.ComputeHash(a)) == Convert.ToHexString(hash.ComputeHash(b));
    }

    private static string LocateModel(string root, string remembered)
    {
        string models = Path.Combine(root, "modelo");
        // A portable copy of the last selected model takes precedence over its old absolute path.
        if (!String.IsNullOrWhiteSpace(remembered))
        {
            string portable = Path.Combine(models, Path.GetFileName(remembered));
            if (File.Exists(portable)) return portable;
            if (File.Exists(remembered)) return remembered;
        }
        string expected = Path.Combine(models, "qwen2.5-3b-instruct-q4_k_m.gguf");
        if (File.Exists(expected)) return expected;
        if (Directory.Exists(models))
        {
            string[] candidates = Directory.GetFiles(models, "*.gguf", SearchOption.TopDirectoryOnly);
            if (candidates.Length == 1) return candidates[0];
        }
        return remembered; // Keep the existing model picker for a missing/ambiguous model.
    }

    [STAThread]
    private static void Main()
    {
        Process engineProcess = null;
        OriginalAppBridge bridge = null;
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            // The managed executable and the self-contained runtime reside together in engine.
            string engine = AppContext.BaseDirectory;
            string root = Directory.GetParent(engine.TrimEnd(Path.DirectorySeparatorChar)).FullName;
            Directory.SetCurrentDirectory(engine);
            string executable = Path.Combine(engine, "DepoimentoLocal.exe");
            string vulkan = Path.Combine(engine, "DepoimentoLocal.Vulkan8.dll");
            string active = Path.Combine(engine, "DepoimentoLocal.dll");
            string[] required = {
                executable, vulkan, Path.Combine(engine, "hostfxr.dll"), Path.Combine(engine, "coreclr.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "vulkan-1.dll")
            };
            foreach (string file in required)
                if (!File.Exists(file)) throw new FileNotFoundException("Arquivo necessário não encontrado. Mantenha a pasta completa da aplicação.", file);
            foreach (string backend in new[] { "vulkan", "avx2" })
            {
                string[] nativeFiles = { "ggml.dll", "ggml-base.dll", backend == "vulkan" ? "ggml-vulkan.dll" : "ggml-cpu.dll", "llama.dll", "mtmd.dll" };
                foreach (string name in nativeFiles)
                {
                    string file = Path.Combine(engine, "runtimes", "win-x64", "native", backend, name);
                    if (!File.Exists(file)) throw new FileNotFoundException("Backend local incompleto; restaure os arquivos da aplicação. Não é necessário baixar arquivos durante a abertura.", file);
                }
            }
            // Preserve the same Vulkan8 profile used by INICIAR, without rewriting an identical DLL.
            if (!SameFile(vulkan, active)) File.Copy(vulkan, active, true);
            var start = new ProcessStartInfo(executable);
            start.WorkingDirectory = engine;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.WindowStyle = ProcessWindowStyle.Minimized;
            engineProcess = Process.Start(start);
            bridge = new OriginalAppBridge(engineProcess);
            if (!bridge.Connect(12000)) throw new InvalidOperationException("Não foi possível iniciar o motor local da aplicação.");
            string model = LocateModel(root, bridge.ModelPath);
            if (!String.IsNullOrWhiteSpace(model) && File.Exists(model))
                // Native GGUF fopen on Windows may reject accented absolute ancestor paths.
                // Both UI and engine use engine/ as cwd, so this remains portable and unambiguous.
                bridge.ModelPath = Path.GetRelativePath(engine, Path.GetFullPath(model));
            using (var form = new ModernDepoimentoForm(bridge, Path.Combine(root, "ui", "DepoimentoLocal.ico"), "Vulkan • 8 camadas GPU"))
                Application.Run(form);
        }
        catch (Exception error)
        {
            MessageBox.Show(error.Message + (error is FileNotFoundException ? "\n\n" + ((FileNotFoundException)error).FileName : ""),
                "Fidelis — erro ao iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (bridge != null) bridge.Stop();
            else if (engineProcess != null && !engineProcess.HasExited) engineProcess.Kill();
            if (engineProcess != null) engineProcess.Dispose();
        }
    }
}


