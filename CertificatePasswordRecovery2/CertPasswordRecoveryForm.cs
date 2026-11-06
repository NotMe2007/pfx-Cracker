/*
Copyright (c) 2012 Rion Carter

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
using System.Windows.Forms;

namespace CertificatePasswordRecovery;

public partial class CertPasswordRecoveryForm : Form
{
    private CancellationTokenSource? recoveryCancellation;
    private bool closeRequested;

    public CertPasswordRecoveryForm()
    {
        InitializeComponent();
        logLevelBx.SelectedIndex = 2;
        startStringBx.MaxLength = (int)maxGenBx.Value;
        logPathBx.Text = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments), "CertificatePasswordRecoveryLog.txt");
    }

    private async void recoverPasswordBtn_Click(object? sender, EventArgs e)
    {
        if (recoveryCancellation != null) return;
        RecoveryOptions options;
        try
        {
            // Snapshot every input on the UI thread before starting background work.
            options = new RecoveryOptions((int)maxGenBx.Value,
                RecoveryRunner.ParseSymbols(symbolSequenceBx.Text), startStringBx.Text,
                prefixBx.Text, suffixBx.Text, pathToCertBx.Text, logPathBx.Text,
                logLevelBx.SelectedIndex, TrimPadding: true);
            RecoveryRunner.Validate(options);
            if (!File.Exists(options.CertificatePath))
                throw new ArgumentException("Select an existing certificate file.");
            if (options.LogLevel != 0 && string.IsNullOrWhiteSpace(options.LogPath))
                throw new ArgumentException("Enter a log file path.");
        }
        catch (ArgumentException error)
        {
            MessageBox.Show(this, error.Message, "Invalid input");
            return;
        }

        using var cancellation = new CancellationTokenSource();
        recoveryCancellation = cancellation;
        var session = new RecoverySession(options);
        SetRunning(true);
        try
        {
            var result = await RecoveryRunner.RunAsync(options, session.TestPassword,
                session.LogAttempt, Environment.ProcessorCount, cancellation.Token);
            if (!closeRequested && !IsDisposed)
                MessageBox.Show(this, result == null ? "Password not found." :
                    $"Password found: {result}", result == null ? "Info" : "Success");
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // Closing cancels the run without showing a failure dialog.
        }
        catch (Exception error)
        {
            if (!closeRequested && !IsDisposed)
                MessageBox.Show(this, error.GetBaseException().Message, "Recovery failed");
        }
        finally
        {
            recoveryCancellation = null;
            if (!IsDisposed)
            {
                logPathBx.Text = session.LogPath;
                SetRunning(false);
                if (closeRequested) Close();
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel) return;
        if (recoveryCancellation != null)
        {
            // Keep the UI context alive until all workers have stopped.
            e.Cancel = true;
            closeRequested = true;
            recoveryCancellation.Cancel();
        }
    }

    private void SetRunning(bool running)
    {
        maxGenBx.Enabled = !running;
        startStringBx.ReadOnly = running;
        prefixBx.ReadOnly = running;
        suffixBx.ReadOnly = running;
        symbolSequenceBx.ReadOnly = running;
        pathToCertBx.ReadOnly = running;
        logPathBx.ReadOnly = running;
        logLevelBx.Enabled = !running;
        certBrowseBtn.Enabled = !running;
        recoverPasswordBtn.Enabled = !running;
        progressBar.Style = running ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        progressBar.Visible = running;
    }

    private void maxGenBx_ValueChanged(object? sender, EventArgs e)
    {
        startStringBx.MaxLength = (int)maxGenBx.Value;
    }

    private void logLevelBx_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // The selected value is captured when a run starts.
    }

    private void certBrowseBtn_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Certificate Files (*.pfx;*.p12;*.cer)|*.pfx;*.p12;*.cer|All Files (*.*)|*.*",
            Title = "Select Certificate File"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) pathToCertBx.Text = dialog.FileName;
    }

    private void helpAboutLinkLbl_Click(object? sender, EventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
            "https://7thzero.com/blog/certificate-password-recovery-tool") { UseShellExecute = true });
    }
}
