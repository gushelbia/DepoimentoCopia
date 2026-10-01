// Gender of the deponent in the real window, with the real model: conversion on
// screen, in the consolidated text, the draft, the recovery copy and the Word;
// identity for Masculino and Não informado; suggestion only with a clear clue.
public static class GenderUiTest
{
    private static string reportPath;
    private static int failures;

    private static void Log(string m) { Console.WriteLine(m); File.AppendAllText(reportPath, m + Environment.NewLine, Encoding.UTF8); }
    private static void Check(bool ok, string name, string detail)
    {
        if (!ok) failures++;
        Log((ok ? "PASS " : "FAIL ") + name + (String.IsNullOrEmpty(detail) ? "" : " | " + detail));
    }
    private static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }
    private static bool WaitFor(Func<bool> done, int ms) { var t = Stopwatch.StartNew(); while (!done()) { if (t.ElapsedMilliseconds > ms) return false; Application.DoEvents(); System.Threading.Thread.Sleep(20); } return true; }
    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
    private static object Call(object target, string name, params object[] args)
    {
        return target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, args);
    }
    private static void SetGender(Form form, string gender)
    {
        var combo = (ComboBox)Field<Control[]>(form, "qualInputs")[Qualification.Genero];
        combo.SelectedIndex = gender.Length == 0 ? 0 : combo.FindStringExact(gender);
        Pump(150);
    }
    private static string Generate(Form form, OriginalAppBridge bridge, string input)
    {
        Field<SectionCard>(form, "originalCard").Editor.Text = input;
        Pump(300);
        Call(form, "Reformulate_Click", null, EventArgs.Empty);
        WaitFor(delegate { return Field<bool>(form, "generationWasRunning"); }, 5000);
        WaitFor(delegate { return !Field<bool>(form, "generationWasRunning"); }, 300000);
        Pump(1500);
        return bridge.ReformulatedText;
    }
    private static string DocxText(string path)
    {
        using (ZipArchive zip = ZipFile.OpenRead(path))
        using (var reader = new StreamReader(zip.GetEntry("word/document.xml").Open(), Encoding.UTF8))
            return Regex.Replace(reader.ReadToEnd(), "<[^>]+>", "");
    }

    public static int Run(string root, string work)
    {
        reportPath = Path.Combine(root, "maintenance", "gender", "ui-results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Directory.CreateDirectory(work);
        Application.EnableVisualStyles();
        ThemeChoice savedTheme = ThemeManager.LoadChoice();
        string realCopy = AutosaveStore.FilePath + ".antes-do-teste";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        DialogKit.Start();
        Process engine = null;
        ModernDepoimentoForm form = null;
        try
        {
            var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
            start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
            engine = Process.Start(start);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
            form.Show(); Pump(800);
            var original = Field<SectionCard>(form, "originalCard");
            var reform = Field<SectionCard>(form, "reformulatedCard");
            var combined = Field<SectionCard>(form, "combinedCard");
            var suggest = Field<ModernButton>(form, "qualSuggest");
            Control[] inputs = Field<Control[]>(form, "qualInputs");

            // Field.
            var gcombo = (ComboBox)inputs[Qualification.Genero];
            var opts = new List<string>(); foreach (object o in gcombo.Items) opts.Add(Convert.ToString(o));
            Check(String.Join("|", opts.ToArray()) == "Não informado|Masculino|Feminino" && gcombo.SelectedIndex == 0, "campo «Gênero do depoente»: Não informado, Masculino, Feminino (padrão: Não informado)", String.Join(", ", opts.ToArray()));

            // Suggestion: only with a clear clue; never changes the field by itself.
            original.Editor.Text = "Eu fiquei nervosa e liguei para o meu irmão."; Pump(1300);
            Check(suggest.Visible && suggest.Text == "Usar Feminino" && gcombo.SelectedIndex == 0, "pista clara («fiquei nervosa»): sugestão «Usar Feminino» aparece e o campo continua Não informado", suggest.Text);
            original.Editor.Text = "A porta estava fechada quando cheguei."; Pump(1300);
            Check(!suggest.Visible, "sem pista clara: sem sugestão", null);
            original.Editor.Text = "Acho que foi a Maria, mas posso estar enganado."; Pump(1300);
            Check(suggest.Visible && suggest.Text == "Usar Masculino" && gcombo.SelectedIndex == 0, "pista de homem: «Usar Masculino», campo inalterado", suggest.Text);
            original.Editor.Text = "Eu fiquei nervosa."; Pump(1300);
            suggest.PerformClick(); Pump(300);
            string said = Field<Control>(form, "status").Text;
            Check(Convert.ToString(gcombo.SelectedItem) == "Feminino" && !suggest.Visible && said == "Gênero do depoente preenchido: Feminino.", "clicar na sugestão preenche o campo (ação do usuário) e o status diz qual", said);
            SetGender(form, "");

            // Model.
            bridge.ModelPath = @"..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf";
            Field<TextBox>(form, "modelPath").Text = bridge.ModelPath;
            Call(form, "LoadModel_Click", null, EventArgs.Empty);
            Check(WaitFor(delegate { return bridge.ModelReady; }, 180000), "modelo carregado", null);
            Pump(800);

            // Feminino: conversion on screen; the engine still has «o depoente».
            SetGender(form, "Feminino");
            string engineText = Generate(form, bridge, "Eu fiquei nervosa e liguei para o meu irmão.");
            string shown = reform.Editor.Text;
            Check(engineText.Contains("depoente") && !engineText.Contains("a depoente"), "motor continua gerando no masculino (motor não mudou)", engineText);
            Check(shown.StartsWith("A depoente relatou que") && shown.Contains("nervosa") && !Regex.IsMatch(shown, @"\b(?:o|do|ao|no|pelo) depoente\b") && !shown.Contains("nervoso"), "Feminino: texto da tela no feminino", shown);
            Check(!Field<string>(form, "reviewFull").Contains("gênero trocado"), "Feminino: nenhum alerta de gênero trocado no texto convertido", Field<string>(form, "reviewFull"));

            // Consolidated, draft, recovery, Word use the converted text.
            Call(form, "Add_Click", null, EventArgs.Empty); Pump(500);
            Check(combined.Editor.Text.Contains("A depoente relatou que") && combined.Editor.Text.Contains("nervosa"), "consolidado recebe o texto convertido", combined.Editor.Text);
            bool auto = WaitFor(delegate { return File.Exists(AutosaveStore.FilePath) && File.ReadAllText(AutosaveStore.FilePath, Encoding.UTF8).Contains("A depoente relatou que"); }, 5000);
            string rec = File.Exists(AutosaveStore.FilePath) ? File.ReadAllText(AutosaveStore.FilePath, Encoding.UTF8) : "";
            Check(auto && rec.Contains("genero: Feminino"), "cópia de recuperação guarda o texto convertido e o gênero", null);
            string draft = Path.Combine(work, "rascunho-genero.txt");
            DialogKit.ArmFile("Salvar", draft);
            Call(form, "Save_Click", null, EventArgs.Empty); Pump(500);
            string dtext = File.Exists(draft) ? File.ReadAllText(draft, Encoding.UTF8) : "";
            Check(dtext.Contains("genero: Feminino") && dtext.Contains("A depoente relatou que") && dtext.Contains("nervosa"), "rascunho guarda o texto convertido e o gênero", null);
            string docx = Path.Combine(work, "genero.docx");
            DialogKit.ArmFile("Salvar", docx);
            Call(form, "Export_Click", null, EventArgs.Empty); Pump(500);
            string word = File.Exists(docx) ? DocxText(docx) : "";
            Check(word.Contains("A depoente relatou que") && word.Contains("nervosa") && !word.Contains("nervoso"), "Word usa o texto convertido", null);

            // Switching the field: the reformulated text follows (not edited by hand); the consolidated never changes.
            string consolidatedBefore = combined.Editor.Text;
            SetGender(form, "Masculino"); Pump(300);
            Check(reform.Editor.Text == engineText && combined.Editor.Text == consolidatedBefore, "trocar para Masculino: reformulado volta ao texto do motor; consolidado não muda", reform.Editor.Text);
            SetGender(form, "Feminino"); Pump(300);
            Check(reform.Editor.Text == shown, "voltar para Feminino: converte de novo", reform.Editor.Text);
            reform.Editor.Text = shown + " Editado."; Pump(100);
            SetGender(form, "Masculino"); Pump(300);
            Check(reform.Editor.Text == shown + " Editado.", "texto editado à mão não é reescrito ao trocar o gênero", null);

            // Masculino and Não informado: the screen shows exactly the engine's text.
            foreach (string g in new string[] { "Masculino", "" })
            {
                SetGender(form, g);
                string e = Generate(form, bridge, "Ele me ligou duas vezes e eu não atendi.");
                Check(reform.Editor.Text == e, "«" + (g.Length == 0 ? "Não informado" : g) + "»: texto da tela idêntico ao do motor", reform.Editor.Text);
            }
            Check(DialogKit.Unexpected.Count == 0, "nenhuma janela inesperada", String.Join(", ", DialogKit.Unexpected.ToArray()));
        }
        catch (Exception ex) { failures++; Log("FAIL execução: " + ex); }
        finally
        {
            DialogKit.Stop();
            if (form != null)
            {
                try
                {
                    Field<SectionCard>(form, "combinedCard").Editor.Text = "";
                    Call(form, "ApplyQualification", new Qualification());
                    form.Close(); form.Dispose();
                }
                catch { }
            }
            try { if (engine != null && !engine.HasExited) engine.Kill(); } catch { }
            ThemeManager.SaveChoice(savedTheme);
            AutosaveStore.Delete();
            if (hadReal) File.Move(realCopy, AutosaveStore.FilePath);
            Log(failures == 0 ? "RESULT PASS" : "RESULT FAIL (" + failures + ")");
        }
        return failures;
    }
}
