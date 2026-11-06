using System.Drawing;
using System.Windows.Forms;

namespace CertificatePasswordRecovery;

public partial class CertPasswordRecoveryForm
{
    private readonly Color ink = Color.FromArgb(25, 43, 67);
    private readonly Color muted = Color.FromArgb(87, 105, 124);
    private readonly Color accent = Color.FromArgb(32, 94, 215);
    private TextBox pathToCertBx = null!, rememberedBx = null!, symbolSequenceBx = null!,
        prefixBx = null!, suffixBx = null!, logPathBx = null!, startStringBx = null!, resultBx = null!;
    private NumericUpDown maxGenBx = null!, minGenBx = null!, workersBx = null!;
    private ComboBox modeBx = null!, charactersBx = null!, logLevelBx = null!;
    private CheckBox variationsBx = null!, advancedBx = null!, revealBx = null!;
    private Button recoverPasswordBtn = null!, certBrowseBtn = null!, stopBtn = null!, copyBtn = null!;
    private Label statusLbl = null!, detailLbl = null!, countLbl = null!, speedLbl = null!,
        elapsedLbl = null!, estimateLbl = null!, fileHintLbl = null!, searchHintLbl = null!;
    private ProgressBar progressBar = null!;
    private TableLayoutPanel wordsPanel = null!, rangePanel = null!, advancedPanel = null!, resultPanel = null!;
    private System.Windows.Forms.Timer progressTimer = null!;

    private Label TextLabel(string text, float size = 10, bool bold = false)
        => new() { Text = text, AutoSize = true, ForeColor = ink,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(0, 0, 0, 8), Dock = DockStyle.Top };

    private static TableLayoutPanel Stack() => new()
    {
        AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1,
        Dock = DockStyle.Top, Margin = new Padding(0), Padding = new Padding(0)
    };

    private TableLayoutPanel Card(string title)
    {
        var card = Stack();
        card.BackColor = Color.White;
        card.Padding = new Padding(20);
        card.Margin = new Padding(0, 0, 0, 16);
        card.Controls.Add(TextLabel(title, 12, true));
        return card;
    }

    private static TextBox Input(string name, string placeholder = "") => new()
    {
        Name = name, AccessibleName = placeholder, PlaceholderText = placeholder,
        Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 10),
        BorderStyle = BorderStyle.FixedSingle
    };

    private static ComboBox Choice(string name, params string[] items)
    {
        var box = new ComboBox { Name = name, DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 10) };
        box.Items.AddRange(items);
        box.SelectedIndex = 0;
        return box;
    }

    private Button ActionButton(string text, bool primary = false) => new()
    {
        Text = text, AutoSize = true, Height = 42, MinimumSize = new Size(110, 42),
        FlatStyle = FlatStyle.Flat, BackColor = primary ? accent : Color.White,
        ForeColor = primary ? Color.White : ink, Padding = new Padding(14, 5, 14, 5),
        Margin = new Padding(0, 0, 10, 0), UseVisualStyleBackColor = false,
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
    };

    private void BuildLayout()
    {
        SuspendLayout();
        Text = "PFX Password Recovery";
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(239, 244, 250);
        ForeColor = ink;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96, 96);
        ClientSize = new Size(1010, 810);
        MinimumSize = new Size(820, 640);
        StartPosition = FormStartPosition.CenterScreen;

        var page = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(24) };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(page);
        var header = Stack();
        header.Padding = new Padding(0, 0, 0, 16);
        header.Controls.Add(TextLabel("Find your certificate password", 23, true));
        var subtitle = TextLabel("Start with what you remember. Everything stays on this computer.");
        subtitle.ForeColor = muted;
        header.Controls.Add(subtitle);
        page.Controls.Add(header, 0, 0);

        var columns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        columns.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.Controls.Add(columns, 0, 1);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(0, 0, 18, 0), Margin = new Padding(0) };
        columns.Controls.Add(scroll, 0, 0);
        var setup = Stack();
        scroll.Controls.Add(setup);

        var file = Card("1   Choose your certificate");
        setup.Controls.Add(file);
        pathToCertBx = Input("pathToCertBx", "Select a .pfx or .p12 file");
        file.Controls.Add(pathToCertBx);
        certBrowseBtn = ActionButton("Browse files…");
        certBrowseBtn.Click += certBrowseBtn_Click;
        file.Controls.Add(certBrowseBtn);
        fileHintLbl = TextLabel("Your original file will not be changed.", 9);
        fileHintLbl.Margin = new Padding(0, 10, 0, 0);
        fileHintLbl.ForeColor = muted;
        file.Controls.Add(fileHintLbl);

        var search = Card("2   Choose how to search");
        setup.Controls.Add(search);
        modeBx = Choice("modeBx", "Try passwords I remember", "Search by length and characters");
        search.Controls.Add(modeBx);
        wordsPanel = Stack();
        wordsPanel.Controls.Add(TextLabel("Words or possible passwords — one per line", 9, true));
        rememberedBx = Input("rememberedBx", "Enter a company name, part name, or an old password");
        rememberedBx.Multiline = true;
        rememberedBx.AcceptsReturn = true;
        rememberedBx.ScrollBars = ScrollBars.Vertical;
        rememberedBx.Height = 100;
        rememberedBx.MaxLength = 20000;
        wordsPanel.Controls.Add(rememberedBx);
        variationsBx = new CheckBox { Text = "Also try capitals, numbers (0–99), years (1990–now), !, 123, 1234",
            AutoSize = true, Dock = DockStyle.Top, Checked = true, Margin = new Padding(0, 0, 0, 10) };
        wordsPanel.Controls.Add(variationsBx);
        search.Controls.Add(wordsPanel);

        rangePanel = Stack();
        rangePanel.Visible = false;
        rangePanel.Controls.Add(TextLabel("Characters the unknown part may contain", 9, true));
        charactersBx = Choice("charactersBx", "Numbers only (0–9)", "Lowercase letters (a–z)",
            "Letters and numbers (a–z, A–Z, 0–9)", "Custom characters");
        rangePanel.Controls.Add(charactersBx);
        symbolSequenceBx = Input("symbolSequenceBx", "Type characters directly, without separating commas");
        symbolSequenceBx.Text = "0123456789";
        symbolSequenceBx.Visible = false;
        rangePanel.Controls.Add(symbolSequenceBx);
        var lengths = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 8) };
        minGenBx = new NumericUpDown { Name = "minGenBx", Minimum = 1, Maximum = 64, Value = 1, Width = 64 };
        maxGenBx = new NumericUpDown { Name = "maxGenBx", Minimum = 1, Maximum = 64, Value = 4, Width = 64 };
        lengths.Controls.Add(TextLabel("From"));
        lengths.Controls.Add(minGenBx);
        lengths.Controls.Add(TextLabel("to"));
        lengths.Controls.Add(maxGenBx);
        lengths.Controls.Add(TextLabel("unknown characters"));
        rangePanel.Controls.Add(lengths);
        search.Controls.Add(rangePanel);
        searchHintLbl = TextLabel("Long passwords are best approached using remembered words.", 9);
        searchHintLbl.ForeColor = muted;
        search.Controls.Add(searchHintLbl);

        advancedBx = new CheckBox { Text = "More options", AutoSize = true, Margin = new Padding(0, 2, 0, 12) };
        search.Controls.Add(advancedBx);
        advancedPanel = Stack();
        advancedPanel.Visible = false;
        advancedPanel.Controls.Add(TextLabel("Known beginning / ending (optional)", 9, true));
        prefixBx = Input("prefixBx", "Beginning — added before each guess");
        suffixBx = Input("suffixBx", "Ending — added after each guess");
        advancedPanel.Controls.Add(prefixBx);
        advancedPanel.Controls.Add(suffixBx);
        // Retained internally for compatibility with the legacy form's lifecycle checks.
        startStringBx = new TextBox { Visible = false };
        search.Controls.Add(startStringBx);
        advancedPanel.Controls.Add(TextLabel("CPU workers (more can make your PC less responsive)", 9, true));
        workersBx = new NumericUpDown { Name = "workersBx", Minimum = 1,
            Maximum = Math.Max(1, Environment.ProcessorCount), Width = 80, Margin = new Padding(0, 0, 0, 10) };
        advancedPanel.Controls.Add(workersBx);
        advancedPanel.Controls.Add(TextLabel("Save a log", 9, true));
        logLevelBx = Choice("logLevelBx", "Off — recommended", "Save recovered password",
            "Every 10,000 guesses + password", "Every guess + password (slower)");
        advancedPanel.Controls.Add(logLevelBx);
        logPathBx = Input("logPathBx", "Log file path");
        logPathBx.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CertificatePasswordRecoveryLog.txt");
        logPathBx.Visible = false;
        advancedPanel.Controls.Add(logPathBx);
        var privacy = TextLabel("Logs can contain passwords. Logging is off unless you choose it.", 9);
        privacy.ForeColor = muted;
        advancedPanel.Controls.Add(privacy);
        search.Controls.Add(advancedPanel);

        var statusScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = new Padding(0) };
        columns.Controls.Add(statusScroll, 1, 0);
        var status = Card("3   Follow your progress");
        statusScroll.Controls.Add(status);
        statusLbl = TextLabel("Ready when you are", 17, true);
        status.Controls.Add(statusLbl);
        detailLbl = TextLabel("Choose a file and add a few password clues to get started.", 10);
        detailLbl.ForeColor = muted;
        status.Controls.Add(detailLbl);
        progressBar = new ProgressBar { Dock = DockStyle.Top, Height = 8, Margin = new Padding(0, 8, 0, 20), Maximum = 1000 };
        status.Controls.Add(progressBar);
        countLbl = TextLabel("0 guesses checked", 14, true);
        speedLbl = TextLabel("Speed: —");
        elapsedLbl = TextLabel("Elapsed: 00:00:00");
        estimateLbl = TextLabel("Remaining time: measured after starting", 9);
        status.Controls.Add(countLbl);
        status.Controls.Add(speedLbl);
        status.Controls.Add(elapsedLbl);
        status.Controls.Add(estimateLbl);
        var notes = TextLabel("CPU processing • certificate cached in RAM\nGPU acceleration is not enabled.", 9);
        notes.ForeColor = muted;
        notes.Margin = new Padding(0, 16, 0, 12);
        status.Controls.Add(notes);
        resultPanel = Stack();
        resultPanel.Visible = false;
        resultPanel.Controls.Add(TextLabel("Recovered password", 10, true));
        resultBx = Input("resultBx");
        resultBx.ReadOnly = true;
        resultBx.UseSystemPasswordChar = true;
        resultPanel.Controls.Add(resultBx);
        revealBx = new CheckBox { Text = "Show password", AutoSize = true };
        revealBx.CheckedChanged += (_, _) => resultBx.UseSystemPasswordChar = !revealBx.Checked;
        resultPanel.Controls.Add(revealBx);
        copyBtn = ActionButton("Copy password");
        copyBtn.Click += (_, _) => CopyPassword();
        resultPanel.Controls.Add(copyBtn);
        status.Controls.Add(resultPanel);

        var footer = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 16, 0, 0), Margin = new Padding(0) };
        recoverPasswordBtn = ActionButton("Start recovery", true);
        recoverPasswordBtn.Name = "recoverPasswordBtn";
        recoverPasswordBtn.Click += recoverPasswordBtn_Click;
        stopBtn = ActionButton("Stop");
        stopBtn.Enabled = false;
        stopBtn.Click += (_, _) => StopRecovery();
        footer.Controls.Add(recoverPasswordBtn);
        footer.Controls.Add(stopBtn);
        footer.Controls.Add(TextLabel("You can stop at any time. Stopping does not save a resume point.", 9));
        page.Controls.Add(footer, 0, 2);

        modeBx.SelectedIndexChanged += (_, _) => { wordsPanel.Visible = modeBx.SelectedIndex == 0; rangePanel.Visible = !wordsPanel.Visible; UpdateSearchSize(); };
        charactersBx.SelectedIndexChanged += (_, _) =>
        {
            symbolSequenceBx.Visible = charactersBx.SelectedIndex == 3;
            if (charactersBx.SelectedIndex < 3)
                symbolSequenceBx.Text = new[] { "0123456789", "abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" }[charactersBx.SelectedIndex];
            UpdateSearchSize();
        };
        advancedBx.CheckedChanged += (_, _) => advancedPanel.Visible = advancedBx.Checked;
        logLevelBx.SelectedIndexChanged += (_, _) => logPathBx.Visible = logLevelBx.SelectedIndex != 0;
        minGenBx.ValueChanged += (_, _) => UpdateSearchSize();
        maxGenBx.ValueChanged += (_, _) => UpdateSearchSize();
        symbolSequenceBx.TextChanged += (_, _) => UpdateSearchSize();
        pathToCertBx.TextChanged += (_, _) => fileHintLbl.Text = "Your original file will not be changed.";
        progressTimer = new System.Windows.Forms.Timer { Interval = 250 };
        progressTimer.Tick += (_, _) => RefreshProgress();
        // Bound label widths to their cards so longer help text wraps at any window size.
        status.SizeChanged += (_, _) => WrapLabels(status);
        search.SizeChanged += (_, _) => WrapLabels(search);
        ResumeLayout(true);
    }

    private static void WrapLabels(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if (child is Label label) label.MaximumSize = new Size(Math.Max(100, parent.ClientSize.Width - parent.Padding.Horizontal - 8), 0);
            else if (child is TableLayoutPanel) WrapLabels(child);
        }
    }
}
