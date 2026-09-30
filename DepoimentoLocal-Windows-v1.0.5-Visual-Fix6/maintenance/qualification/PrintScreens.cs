// Screenshots for approval: qualification panel closed and open, light and
// dark themes, and the narrow window. Fictitious data only.
public static class PrintScreens
{
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);

    private static void Pump(int ms) { var t = Stopwatch.StartNew(); while (t.ElapsedMilliseconds < ms) { Application.DoEvents(); System.Threading.Thread.Sleep(15); } }
    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
    private static object Call(object target, string name, params object[] args)
    {
        return target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, args);
    }

    private static void Shot(Form form, string path)
    {
        form.TopMost = true; form.Location = new Point(0, 0); form.Activate(); Pump(900);
        using (var bmp = new Bitmap(form.Width, form.Height))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr dc = g.GetHdc();
                PrintWindow(form.Handle, dc, 2);
                g.ReleaseHdc(dc);
            }
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
        form.TopMost = false; form.Location = new Point(-4000, -4000); Pump(200);
        Console.WriteLine("print: " + path);
    }

    public static void Run(string root, string dir)
    {
        Application.EnableVisualStyles();
        ThemeChoice savedTheme = ThemeManager.LoadChoice();
        string realCopy = AutosaveStore.FilePath + ".antes-do-teste";
        bool hadReal = File.Exists(AutosaveStore.FilePath);
        if (hadReal) { if (File.Exists(realCopy)) File.Delete(realCopy); File.Move(AutosaveStore.FilePath, realCopy); }
        Process engine = null;
        ModernDepoimentoForm form = null;
        try
        {
            ThemeManager.SaveChoice(ThemeChoice.Light);
            var start = new ProcessStartInfo(Path.Combine(root, "engine", "DepoimentoLocal.exe"));
            start.WorkingDirectory = Path.Combine(root, "engine"); start.UseShellExecute = false; start.CreateNoWindow = true;
            engine = Process.Start(start);
            var bridge = new OriginalAppBridge(engine);
            if (!bridge.Connect(12000)) throw new Exception("Motor não conectou");
            form = new ModernDepoimentoForm(bridge, "", "Vulkan");
            form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-4000, -4000); form.Size = new Size(1280, 860);
            form.Show(); Pump(800);

            var q = new Qualification();
            q[Qualification.Procedimento] = "IP 0457/2026";
            q[Qualification.Unidade] = "1ª DP de Exemplópolis";
            q[Qualification.Local] = "Sala de oitivas 2";
            q[Qualification.DataHora] = "30/09/2026 14:30";
            q[Qualification.Depoente] = "Maria Exemplo da Silva";
            q[Qualification.Documento] = "CPF 529.982.247-25";
            q[Qualification.Condicao] = "testemunha";
            q[Qualification.Endereco] = "Rua das Amostras, 100, Centro, Exemplópolis";
            q[Qualification.Telefone] = "(31) 99999-0000";
            q[Qualification.Autoridade] = "Dr. João Fictício Pereira";
            q[Qualification.Escrivao] = "Ana Modelo Souza";
            Call(form, "ApplyQualification", q);
            Field<SectionCard>(form, "originalCard").Editor.Text =
                "Eu estava na portaria do prédio no dia 12/08/2026, lá pelas 14h30. Um homem de boné azul entrou e pediu para falar com o síndico. " +
                "Eu disse que ele não estava. Ele ficou nervoso, bateu no balcão e saiu num carro prata, placa ABC1D23.";
            Field<SectionCard>(form, "reformulatedCard").Editor.Text =
                "A depoente declarou que, no dia 12 de agosto de 2026, por volta das 14h30, estava na portaria do prédio, quando um homem de boné azul entrou e pediu para falar com o síndico. " +
                "Informou que o síndico não estava. Relatou que o homem ficou nervoso, bateu no balcão e saiu em um veículo prata, placa ABC1D23.";
            Field<SectionCard>(form, "combinedCard").Editor.Text =
                "A depoente declarou que, no dia 12 de agosto de 2026, por volta das 14h30, estava na portaria do prédio, quando um homem de boné azul entrou e pediu para falar com o síndico.";
            Pump(1200);
            Field<Label>(form, "status").Text = "Rascunho aberto: depoimento-rascunho.txt.";
            var toggle = Field<ModernButton>(form, "qualToggle");
            var picker = Field<ThemedComboBox>(form, "themePicker");

            picker.SelectedIndex = 1; Pump(400);
            Shot(form, Path.Combine(dir, "print-fechado-claro.png"));
            toggle.PerformClick(); Pump(400);
            Shot(form, Path.Combine(dir, "print-aberto-claro.png"));
            picker.SelectedIndex = 2; Pump(500);
            Shot(form, Path.Combine(dir, "print-aberto-escuro.png"));
            toggle.PerformClick(); Pump(400);
            Shot(form, Path.Combine(dir, "print-fechado-escuro.png"));
            picker.SelectedIndex = 1; Pump(400);
            form.Size = new Size(1060, 860); Pump(500);
            toggle.PerformClick(); Pump(400);
            Shot(form, Path.Combine(dir, "print-estreito-aberto-claro.png"));
        }
        finally
        {
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
        }
    }
}
