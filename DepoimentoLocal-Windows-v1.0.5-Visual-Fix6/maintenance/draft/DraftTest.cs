// «Salvar rascunho» / «Abrir rascunho»: text and qualification together, old
// .txt drafts, question before replacing unsaved work, autosave and recovery
// of the fields.
public static class DraftTest
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
    private static Rectangle InForm(Form form, Control c) { return form.RectangleToClient(c.RectangleToScreen(c.ClientRectangle)); }

    private static Qualification Sample()
    {
        var q = new Qualification();
        q[Qualification.Procedimento] = "IP 0457/2026";
        q[Qualification.Unidade] = "1ª DP de Exemplópolis";
        q[Qualification.Local] = "Sala 2";
        q[Qualification.DataHora] = "30/09/2026 14:30";
        q[Qualification.Depoente] = "Maria Exemplo da Silva";
        q[Qualification.Documento] = "CPF 529.982.247-25";
        q[Qualification.Condicao] = "testemunha";
        q[Qualification.Endereco] = "Rua das Amostras, 100";
        q[Qualification.Telefone] = "(31) 99999-0000";
        q[Qualification.Autoridade] = "Dr. João Fictício";
        q[Qualification.Escrivao] = "Ana Modelo";
        return q;
    }

    private static string Describe(Qualification q)
    {
        if (q == null) return "(nulo)";
        var parts = new List<string>();
        for (int i = 0; i < Qualification.Count; i++) parts.Add(Qualification.Keys[i] + "=" + q[i]);
        return String.Join("; ", parts.ToArray());
    }

    private static string Saved()
    {
        if (!File.Exists(AutosaveStore.FilePath)) return null;
        return File.ReadAllText(AutosaveStore.FilePath, Encoding.UTF8);
    }

    private static Process StartEngine(string root)
    {
        var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
        start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
        return Process.Start(start);
    }

    private static ModernDepoimentoForm NewForm(OriginalAppBridge bridge)
    {
        var form = new ModernDepoimentoForm(bridge, "", "Vulkan");
        form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
        return form;
    }

    public static int Run(string root, string work)
    {
        reportPath = Path.Combine(root, "maintenance", "draft", "results.txt");
        File.WriteAllText(reportPath, "START " + DateTime.Now.ToString("s") + Environment.NewLine, Encoding.UTF8);
        Directory.CreateDirectory(work);

        // ---- File format (no window) ----
        try
        {
            Qualification q = Sample();
            q[Qualification.Endereco] = "Rua A: nº 5 \\ bloco B";
            string text = "Primeira linha.\r\n\r\n[TEXTO]\r\ncampo: valor que parece cabeçalho\r\nÚltima linha com acentos: ação, fé.";
            string content = DraftFile.Serialize(text, q);
            string back; Qualification qb;
            DraftFile.Parse(content, out back, out qb);
            Check(back == text && qb != null && qb.SameAs(q), "2: rascunho guarda texto e campos juntos e reabre igual", Describe(qb));
            Check(content.StartsWith(DraftFile.Header + "\r\n") && content.Contains("\r\ndepoente: Maria Exemplo da Silva\r\n") && content.Contains("\r\n[TEXTO]\r\nPrimeira linha."),
                "2: formato legível no Bloco de Notas (cabeçalho, um campo por linha, depois o texto)", null);

            var multi = new Qualification(); multi[Qualification.Endereco] = "linha 1\nlinha 2";
            DraftFile.Parse(DraftFile.Serialize("x", multi), out back, out qb);
            Check(qb[Qualification.Endereco] == "linha 1\nlinha 2" && back == "x", "2: quebra de linha e barra invertida num campo não quebram o arquivo", null);

            string legacy = "Texto antigo salvo só como texto.\r\nSegunda linha.";
            DraftFile.Parse(legacy, out back, out qb);
            Check(back == legacy && qb == null, "2: .txt antigo abre como texto, sem campos", null);
            DraftFile.Parse("﻿" + legacy, out back, out qb);
            Check(back == legacy && qb == null, "2: .txt antigo com BOM", null);
            DraftFile.Parse(DraftFile.Serialize("", new Qualification()), out back, out qb);
            Check(back == "" && qb != null && !qb.HasContentBesidesDate(), "2: rascunho vazio", null);
            DraftFile.Parse(DraftFile.Header + "\r\nversao: 2\r\nnovo_campo: x\r\ndepoente: Zé\r\n[TEXTO]\r\nT", out back, out qb);
            Check(back == "T" && qb[Qualification.Depoente] == "Zé", "2: campos desconhecidos (versões futuras) são ignorados", null);
            bool threw = false;
            try { DraftFile.Parse(DraftFile.Header + "\r\ndepoente: Zé\r\n", out back, out qb); } catch (InvalidDataException) { threw = true; }
            Check(threw, "2: rascunho cortado (sem a parte do texto) é recusado", null);
        }
        catch (Exception ex) { failures++; Log("FAIL execução (formato): " + ex); }

        // ---- Window ----
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
            engine = StartEngine(root);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = NewForm(bridge);
            form.Show(); Pump(800);
            var combined = Field<SectionCard>(form, "combinedCard");
            var status = Field<Label>(form, "status");
            var actions = Field<FlowLayoutPanel>(form, "actions");
            var saveButton = Field<ModernButton>(form, "saveButton");
            var openButton = Field<ModernButton>(form, "openButton");

            // Button next to «Salvar rascunho», visible at every width.
            Check(openButton.Text == "Abrir rascunho" && actions.Controls.GetChildIndex(openButton) == actions.Controls.GetChildIndex(saveButton) + 1,
                "2: botão «Abrir rascunho» ao lado de «Salvar rascunho»", null);
            foreach (int width in new int[] { 1280, 1060, 1050 })
            {
                form.Size = new Size(width, 860); Pump(300);
                var hidden = new List<string>();
                foreach (Control b in actions.Controls)
                {
                    Rectangle r = InForm(form, b);
                    if (!b.Visible || !form.ClientRectangle.Contains(r) || r.Right > InForm(form, actions).Right) hidden.Add(b.Text + " " + r);
                }
                string add = Field<ModernButton>(form, "addButton").Text;
                Check(hidden.Count == 0 && (width < 1200 || add == "Adicionar ao depoimento"), "2: todos os botões da barra visíveis com " + width + " px", "«" + add + "» " + String.Join("; ", hidden.ToArray()));
            }
            form.Size = new Size(1280, 860); Pump(300);

            // Save: text and fields in one file; the recovery copy goes away.
            Call(form, "ApplyQualification", Sample());
            string text = "A depoente declarou que chegou às 14h.\n\nDisse que não viu o carro.";
            combined.Editor.Text = text;
            Check(WaitFor(delegate { return Saved() != null; }, 3000), "2: salvamento automático antes de salvar", null);
            string draft = Path.Combine(work, "rascunho-teste.txt");
            DialogKit.Expect save = DialogKit.ArmFile("Salvar", draft);
            Call(form, "Save_Click", null, EventArgs.Empty); Pump(400);
            string back; Qualification qb;
            bool written = File.Exists(draft);
            if (written) DraftFile.Parse(File.ReadAllText(draft, Encoding.UTF8), out back, out qb); else { back = null; qb = null; }
            Check(save.Done && written && back == text && qb != null && qb.SameAs(Sample()), "2: «Salvar rascunho» grava texto e campos juntos", Describe(qb));
            Check(Saved() == null && !(bool)Call(form, "HasUnsavedChanges"), "2: depois de salvar, nada pendente e sem cópia de recuperação", null);

            // Replace everything, then open the draft: no question (nothing unsaved).
            DialogKit.Expect clear = DialogKit.Arm("Limpar consolidado", 6);
            Call(form, "Clear_Click", null, EventArgs.Empty); Pump(300);
            var fresh = new Qualification(); fresh[Qualification.DataHora] = "01/10/2026 08:00";
            Call(form, "ApplyQualification", fresh); Pump(100);
            Check(clear.Done && combined.Editor.Text.Length == 0, "2: consolidado e campos limpos para o teste", null);
            DialogKit.Expect open = DialogKit.ArmFile("Abrir rascunho", draft);
            Call(form, "Open_Click", null, EventArgs.Empty);
            WaitFor(delegate { return combined.Editor.Text == text; }, 4000); Pump(500);
            Qualification shown = (Qualification)Call(form, "ReadQualification");
            Check(open.Done && combined.Editor.Text == text && shown.SameAs(Sample()), "2: «Abrir rascunho» restaura texto e campos", Describe(shown));
            Check(bridge.CombinedText.Replace("\r\n", "\n") == text && !bridge.CombinedText.Contains("Maria Exemplo") && bridge.OriginalText.Length == 0,
                "2: só o texto vai para o motor; os campos não", null);
            Check(status.Text.StartsWith("Rascunho aberto") && !(bool)Call(form, "HasUnsavedChanges") && Saved() == null, "2: rascunho aberto conta como salvo", status.Text);
            Check(DialogKit.Unexpected.Count == 0, "2: sem pergunta quando não há nada a perder", String.Join(", ", DialogKit.Unexpected.ToArray()));

            // Unsaved text: asks first, «Não» is the default and keeps everything.
            combined.Editor.Text = text + "\n\nParágrafo que ainda não foi salvo.";
            string unsaved = combined.Editor.Text;
            DialogKit.ArmFile("Abrir rascunho", draft);
            DialogKit.Expect keep = DialogKit.Arm("Abrir rascunho", 7);
            Call(form, "Open_Click", null, EventArgs.Empty); Pump(600);
            Check(keep.Done && keep.DefaultId == 7 && keep.Text.Contains("não foram salvas") && combined.Editor.Text == unsaved,
                "2: texto não salvo: pergunta antes de substituir, «Não» é o padrão e mantém o texto", keep.Text.Replace("\n", " "));
            DialogKit.ArmFile("Abrir rascunho", draft);
            DialogKit.Expect replace = DialogKit.Arm("Abrir rascunho", 6);
            Call(form, "Open_Click", null, EventArgs.Empty);
            WaitFor(delegate { return combined.Editor.Text == text; }, 4000); Pump(300);
            Check(replace.Done && combined.Editor.Text == text, "2: «Sim» substitui pelo rascunho", null);

            // Unsaved fields only also count.
            SetText(form, Qualification.Telefone, "(31) 90000-1111"); Pump(100);
            Check((bool)Call(form, "HasUnsavedChanges"), "2: campo alterado conta como alteração não salva", null);
            DialogKit.ArmFile("Abrir rascunho", draft);
            DialogKit.Expect keepFields = DialogKit.Arm("Abrir rascunho", 7);
            Call(form, "Open_Click", null, EventArgs.Empty); Pump(600);
            Check(keepFields.Done && ((Qualification)Call(form, "ReadQualification"))[Qualification.Telefone] == "(31) 90000-1111", "2: campo não salvo também gera a pergunta", null);

            // Cancelling the file dialog changes nothing.
            DialogKit.ArmFile("Abrir rascunho", null);
            Call(form, "Open_Click", null, EventArgs.Empty); Pump(400);
            Check(combined.Editor.Text == text && ((Qualification)Call(form, "ReadQualification"))[Qualification.Telefone] == "(31) 90000-1111", "2: cancelar a janela de abrir não muda nada", null);

            // An old .txt (text only): asks whether to clear the fields, «Sim» by default.
            string old = Path.Combine(work, "rascunho-antigo.txt");
            File.WriteAllText(old, "Rascunho antigo, só texto.\r\nSegunda linha.", new UTF8Encoding(false));
            DialogKit.ArmFile("Abrir rascunho", old);
            DialogKit.Arm("Abrir rascunho", 6);                              // unsaved changes: replace
            DialogKit.Expect keepOld = DialogKit.Arm("Abrir rascunho", 7);  // old draft: keep the fields
            Call(form, "Open_Click", null, EventArgs.Empty);
            WaitFor(delegate { return combined.Editor.Text.StartsWith("Rascunho antigo"); }, 4000); Pump(300);
            Qualification afterOld = (Qualification)Call(form, "ReadQualification");
            Check(keepOld.Done && keepOld.DefaultId == 6 && keepOld.Text.Contains("não tem campos de qualificação") && keepOld.Text.Contains("limpar os campos"),
                "ajuste 1: .txt antigo pergunta se limpa os campos, com «Sim» como padrão", "padrão=" + keepOld.DefaultId + " | " + keepOld.Text.Replace("\n", " "));
            Check(combined.Editor.Text == "Rascunho antigo, só texto.\nSegunda linha." && afterOld[Qualification.Depoente] == "Maria Exemplo da Silva" && status.Text.Contains("não foram alterados"),
                "ajuste 1: «Não» abre o texto e mantém os campos", status.Text);
            combined.Editor.Text = "Texto qualquer ainda não salvo.";
            DialogKit.ArmFile("Abrir rascunho", old);
            DialogKit.Arm("Abrir rascunho", 6);                              // unsaved changes: replace
            DialogKit.Expect clearOld = DialogKit.Arm("Abrir rascunho", 6); // old draft: clear the fields
            Call(form, "Open_Click", null, EventArgs.Empty);
            WaitFor(delegate { return combined.Editor.Text.StartsWith("Rascunho antigo"); }, 4000); Pump(300);
            Qualification cleared = (Qualification)Call(form, "ReadQualification");
            DateTime clearedAt;
            bool nowDate = DateTime.TryParseExact(cleared[Qualification.DataHora], "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out clearedAt) && Math.Abs((DateTime.Now - clearedAt).TotalMinutes) < 3;
            Check(clearOld.Done && combined.Editor.Text == "Rascunho antigo, só texto.\nSegunda linha." && !cleared.HasContentBesidesDate() && nowDate && status.Text.Contains("limpos"),
                "ajuste 1: «Sim» abre o texto e limpa os campos (data e hora renovadas)", Describe(cleared));
            Check(!(bool)Call(form, "HasUnsavedChanges") && Saved() == null, "ajuste 1: depois de abrir e limpar, nada pendente (arquivo com quebras de linha do Windows)", null);
            DialogKit.ArmFile("Abrir rascunho", old);
            Call(form, "Open_Click", null, EventArgs.Empty); Pump(600);
            Check(DialogKit.Unexpected.Count == 0, "ajuste 1: sem campos preenchidos, abre o .txt antigo sem perguntar", String.Join(", ", DialogKit.Unexpected.ToArray()));
            Call(form, "ApplyQualification", Sample()); Pump(100);
            // A file that is not text is refused without touching anything.
            string binary = Path.Combine(work, "nao-e-texto.txt");
            File.WriteAllBytes(binary, new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x08, 0x00 });
            string beforeBinary = combined.Editor.Text;
            DialogKit.ArmFile("Abrir rascunho", binary);
            DialogKit.Expect error = DialogKit.Arm("Abrir rascunho", 1);
            Call(form, "Open_Click", null, EventArgs.Empty); Pump(600);
            Check(error.Done && error.Text.Contains("Não foi possível abrir") && combined.Editor.Text == beforeBinary, "2: arquivo que não é texto é recusado sem apagar nada", error.Text.Replace("\n", " "));

            // Save also works with fields only.
            DialogKit.Arm("Limpar consolidado", 6); Call(form, "Clear_Click", null, EventArgs.Empty); Pump(300);
            string fieldsOnly = Path.Combine(work, "so-campos.txt");
            DialogKit.ArmFile("Salvar", fieldsOnly);
            Call(form, "Save_Click", null, EventArgs.Empty); Pump(400);
            bool fo = File.Exists(fieldsOnly);
            if (fo) DraftFile.Parse(File.ReadAllText(fieldsOnly, Encoding.UTF8), out back, out qb);
            Check(fo && back == "" && qb[Qualification.Depoente] == "Maria Exemplo da Silva", "2: rascunho só com os campos também é salvo", null);

            // Autosave covers the fields; recovery brings them back.
            SetText(form, Qualification.Depoente, "Nome Alterado Depois");
            combined.Editor.Text = "Texto ainda não salvo.";
            var sw = Stopwatch.StartNew();
            bool auto = WaitFor(delegate { string s = Saved(); return s != null && s.Contains("Nome Alterado Depois") && s.Contains("Texto ainda não salvo."); }, 4000);
            Check(auto && sw.ElapsedMilliseconds >= 1300, "2: salvamento automático guarda texto e campos", sw.ElapsedMilliseconds + " ms");
            SetText(form, Qualification.Local, "Sala 9");
            Check(WaitFor(delegate { string s = Saved(); return s != null && s.Contains("local: Sala 9"); }, 4000), "2: mudar só um campo também dispara o salvamento automático", null);
            var shutdown = new FormClosingEventArgs(CloseReason.WindowsShutDown, false);
            Call(form, "Form_Closing", form, shutdown); Pump(400);
            form.Dispose(); form = null;
            Check(Saved() != null, "2: desligamento do Windows mantém a cópia com os campos", null);
            WaitFor(delegate { return engine.HasExited; }, 5000);

            engine = StartEngine(root);
            bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = NewForm(bridge);
            DialogKit.Expect recover = DialogKit.Arm("Recuperar depoimento", 6);
            form.Show(); WaitFor(delegate { return recover.Done; }, 8000); Pump(600);
            combined = Field<SectionCard>(form, "combinedCard");
            Qualification recovered = (Qualification)Call(form, "ReadQualification");
            Check(recover.Done && recover.Text.Contains("campos de qualificação") && combined.Editor.Text == "Texto ainda não salvo." && recovered[Qualification.Depoente] == "Nome Alterado Depois" && recovered[Qualification.Local] == "Sala 9" && recovered[Qualification.DataHora] == "30/09/2026 14:30",
                "2: recuperação devolve texto e campos (inclusive data e hora)", recover.Text.Replace("\n", " "));
            Check((bool)Call(form, "HasUnsavedChanges"), "2: recuperado continua marcado como não salvo", null);

            // Recovery of fields only.
            DialogKit.Expect clear2 = DialogKit.Arm("Limpar consolidado", 6);
            Call(form, "Clear_Click", null, EventArgs.Empty); Pump(200);
            Check(WaitFor(delegate { string s = Saved(); return s != null && s.Contains("Nome Alterado Depois"); }, 3000), "2: limpar o consolidado mantém a cópia dos campos não salvos", null);
            var shutdown2 = new FormClosingEventArgs(CloseReason.WindowsShutDown, false);
            Call(form, "Form_Closing", form, shutdown2); Pump(300);
            form.Dispose(); form = null;
            WaitFor(delegate { return engine.HasExited; }, 5000);
            engine = StartEngine(root);
            bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = NewForm(bridge);
            DialogKit.Expect recoverFields = DialogKit.Arm("Recuperar depoimento", 6);
            form.Show(); WaitFor(delegate { return recoverFields.Done; }, 8000); Pump(500);
            Check(recoverFields.Done && recoverFields.Text.StartsWith("Foram encontrados campos de qualificação") && ((Qualification)Call(form, "ReadQualification"))[Qualification.Depoente] == "Nome Alterado Depois",
                "2: recuperação só dos campos, com aviso próprio", recoverFields.Text.Replace("\n", " "));

            // Close prompt mentions the fields.
            DialogKit.Expect closeAsk = DialogKit.Arm("Fidelis", 2);
            form.Close(); Pump(500);
            Check(closeAsk.Done && closeAsk.Text.Contains("campos de qualificação") && form.Visible, "2: ao fechar com campos não salvos, pergunta (e «Cancelar» mantém aberto)", closeAsk.Text.Replace("\n", " "));
            Check(DialogKit.Unexpected.Count == 0, "2: nenhuma janela inesperada", String.Join(", ", DialogKit.Unexpected.ToArray()));
        }
        catch (Exception ex) { failures++; Log("FAIL execução (interface): " + ex); }
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

    private static void SetText(Form form, int field, string value)
    {
        Field<Control[]>(form, "qualInputs")[field].Text = value;
    }
}
