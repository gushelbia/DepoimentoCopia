public static class ContextMenuTest
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    private static ToolStripMenuItem Item(RichTextBox editor, string text)
    {
        foreach (ToolStripMenuItem item in editor.ContextMenuStrip.Items) if (item.Text == text) return item;
        throw new Exception("Missing menu item: " + text);
    }
    private static void OpenMenu(RichTextBox editor)
    {
        // Right-button down/up delivered to the real RichEdit window.
        IntPtr point = new IntPtr((12 << 16) | 12);
        SendMessage(editor.Handle, 0x0204, new IntPtr(2), point);
        SendMessage(editor.Handle, 0x0205, IntPtr.Zero, point);
        Application.DoEvents();
        Check(editor.ContextMenuStrip.Visible, "Right-click context menu did not open");
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] state);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetKeyboardState(byte[] state);
    private static void Key(RichTextBox editor, int character)
    {
        editor.Focus();
        byte[] previous = new byte[256];
        Check(GetKeyboardState(previous), "Keyboard state unavailable");
        byte[] pressed = (byte[])previous.Clone();
        pressed[0x11] = 0x80; // Ctrl on this test thread; no global input injection.
        int virtualKey = character == 3 ? 0x43 : character == 24 ? 0x58 : character == 22 ? 0x56 : 0x41;
        try
        {
            Check(SetKeyboardState(pressed), "Cannot set test thread keyboard state");
            Message message = Message.Create(editor.Handle, 0x0100, new IntPtr(virtualKey), new IntPtr(1));
            if (!editor.PreProcessMessage(ref message)) SendMessage(editor.Handle, message.Msg, message.WParam, message.LParam);
            SendMessage(editor.Handle, 0x0101, new IntPtr(virtualKey), new IntPtr(0xC0000001L));
        }
        finally { SetKeyboardState(previous); }
        Application.DoEvents();
    }
    public static void Run(string outputDirectory)
    {
        var report = new System.Collections.Generic.List<string>();
        IDataObject previousClipboard = Clipboard.GetDataObject();
        try
        {
            using (Form host = new Form())
            {
                host.Text = "DepoimentoLocal — teste de menus";
                host.Size = new Size(760, 640);
                var layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill; layout.RowCount = 3; layout.ColumnCount = 1;
                host.Controls.Add(layout);
                string[] headings = { "Resposta / transcrição original", "Texto reformulado", "Depoimento consolidado" };
                bool[] readOnly = { false, true, false };
                var cards = new SectionCard[3];
                for (int i = 0; i < 3; i++)
                {
                    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
                    cards[i] = new SectionCard(headings[i], readOnly[i]);
                    cards[i].Dock = DockStyle.Fill;
                    layout.Controls.Add(cards[i], 0, i);
                }
                host.Show(); Application.DoEvents();
                foreach (bool dark in new bool[] { false, true })
                {
                    for (int i = 0; i < cards.Length; i++)
                    {
                        Console.WriteLine("TEST " + headings[i] + " dark=" + dark);
                        SectionCard card = cards[i]; RichTextBox editor = card.Editor;
                        card.ApplyPalette(UiPalette.Create(dark));
                        editor.Text = "alfa beta gama"; editor.Select(5, 4);
                        Clipboard.SetText("novo");
                        OpenMenu(editor);
                        Check(editor.SelectionStart == 5 && editor.SelectionLength == 4, "Opening changed selection");
                        Check(Item(editor,"Copiar").Enabled, "Copy disabled with selection");
                        Check(Item(editor,"Recortar").Available == !readOnly[i], "Cut visibility incorrect");
                        Check(Item(editor,"Colar").Available == !readOnly[i], "Paste visibility incorrect");
                        Check(editor.ContextMenuStrip.BackColor == UiPalette.Create(dark).Editor, "Menu background theme");
                        Check(editor.ContextMenuStrip.Renderer is EditorMenuRenderer, "Menu renderer theme");
                        Item(editor,"Copiar").PerformClick(); editor.ContextMenuStrip.Close();
                        Check(Clipboard.GetText() == "beta", "Copy did not respect selection");
                        Check(editor.Text == "alfa beta gama", "Copy changed text");
                        if (!readOnly[i])
                        {
                            OpenMenu(editor); Item(editor,"Recortar").PerformClick(); editor.ContextMenuStrip.Close();
                            Check(editor.Text == "alfa  gama" && Clipboard.GetText() == "beta", "Cut did not respect selection");
                            editor.Select(5, 0); Clipboard.SetText("novo");
                            OpenMenu(editor); Item(editor,"Colar").PerformClick(); editor.ContextMenuStrip.Close();
                            Check(editor.Text == "alfa novo gama", "Paste at caret failed: " + headings[i] + "; dark=" + dark + "; text=" + editor.Text + "; clipboard=" + Clipboard.GetText());
                            editor.Select(5, 4); Clipboard.SetText("outro");
                            OpenMenu(editor); Item(editor,"Colar").PerformClick(); editor.ContextMenuStrip.Close();
                            Check(editor.Text == "alfa outro gama", "Paste did not replace selection");
                        }
                        OpenMenu(editor); Item(editor,"Selecionar tudo").PerformClick(); editor.ContextMenuStrip.Close();
                        Check(editor.SelectionLength == editor.TextLength, "Select all failed");
                        editor.Select(0, 0); OpenMenu(editor);
                        Check(!Item(editor,"Copiar").Enabled, "Copy enabled without selection");
                        Check(!Item(editor,"Recortar").Enabled, "Cut enabled without selection");
                        editor.ContextMenuStrip.Close();
                        editor.Text = "alfa beta gama"; editor.Select(5, 4);
                        Check(editor.ShortcutsEnabled, "Native shortcuts disabled");
                        Key(editor, 3); Check(Clipboard.GetText() == "beta", "Ctrl+C failed");
                        Key(editor, 24);
                        Check(editor.Text == (readOnly[i] ? "alfa beta gama" : "alfa  gama"), "Ctrl+X or read-only protection failed");
                        Clipboard.SetText("novo"); Key(editor, 22);
                        Check(editor.Text == (readOnly[i] ? "alfa beta gama" : "alfa novo gama"), "Ctrl+V or read-only protection failed");
                        Key(editor, 1); Check(editor.SelectionLength == editor.TextLength, "Ctrl+A failed");
                        editor.Text = ""; Clipboard.Clear(); OpenMenu(editor);
                        Check(!Item(editor,"Copiar").Enabled && !Item(editor,"Colar").Enabled && !Item(editor,"Selecionar tudo").Enabled, "Empty states incorrect");
                        editor.ContextMenuStrip.Close();
                        report.Add("PASS " + headings[i] + " / " + (dark ? "escuro" : "claro") + ": clique direito, seleção, recortar/copiar/colar/selecionar tudo, atalhos e estados vazios");
                    }
                }
                host.Close();
            }
        }
        finally
        {
            if (previousClipboard != null) Clipboard.SetDataObject(previousClipboard, true);
            else Clipboard.Clear();
        }
        File.WriteAllLines(Path.Combine(outputDirectory,"results.txt"), report.ToArray(), Encoding.UTF8);
        foreach (string line in report) Console.WriteLine(line);
    }
}






