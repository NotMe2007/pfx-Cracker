using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms;

namespace CertificatePasswordRecovery;

public partial class CertPasswordRecoveryForm
{
    private CancellationTokenSource? recoveryCancellation;
    private bool closeRequested;
    private RecoverySession? activeSession;
    private readonly Stopwatch elapsed = new();
    private BigInteger totalCandidates;
    private long completedAttempts;

    private void InitializeRecovery(int workers)
    {
        workersBx.Value = Math.Clamp(workers, 1, Environment.ProcessorCount);
        UpdateSearchSize();
    }

    private RecoveryOptions CaptureOptions() => new(
        (int)maxGenBx.Value, new string(symbolSequenceBx.Text.Distinct().ToArray()), "",
        prefixBx.Text, suffixBx.Text, pathToCertBx.Text.Trim(), logPathBx.Text.Trim(),
        logLevelBx.SelectedIndex, TrimPadding: false, MinimumLength: (int)minGenBx.Value,
        WordMode: modeBx.SelectedIndex == 0, RememberedPasswords: rememberedBx.Text,
        CommonEndings: variationsBx.Checked);

    private void UpdateSearchSize()
    {
        if (searchHintLbl == null || modeBx == null) return;
        if (modeBx.SelectedIndex == 0)
        {
            searchHintLbl.Text = "Try company names, part names, or old passwords. These clues are not saved.";
            return;
        }
        var options = CaptureOptions();
        if (options.MinimumLength > options.Length || options.Symbols.Length == 0)
        {
            searchHintLbl.Text = "Choose a valid length range and at least one character.";
            return;
        }
        var total = RecoveryRunner.TotalCandidates(options);
        searchHintLbl.Text = total > BigInteger.Pow(10, 18)
            ? $"{total:E2} combinations. This search is too large to finish in practice. Try remembered words or a much smaller range."
            : total > 1_000_000_000
            ? $"{total:E2} combinations. This could take years. Narrow the range or try remembered passwords first."
            : $"{total:N0} combinations. Shorter lengths are checked first. Spaces count as characters.";
    }

    private async void recoverPasswordBtn_Click(object? sender, EventArgs e)
    {
        if (recoveryCancellation != null) return;
        RecoveryOptions options;
        try
        {
            options = CaptureOptions();
            RecoveryRunner.Validate(options);
            if (!File.Exists(options.CertificatePath))
                throw new ArgumentException("Choose an existing .pfx or .p12 file using Browse files.");
            if (options.LogLevel != 0 && string.IsNullOrWhiteSpace(options.LogPath))
                throw new ArgumentException("Enter a log file path, or switch logging off.");
        }
        catch (ArgumentException error)
        {
            statusLbl.Text = "Check your settings";
            detailLbl.Text = error.Message;
            return;
        }

        using var cancellation = new CancellationTokenSource();
        recoveryCancellation = cancellation;
        int workers = (int)workersBx.Value;
        completedAttempts = 0;
        totalCandidates = 0;
        resultBx.Clear();
        resultPanel.Visible = false;
        revealBx.Checked = false;
        elapsed.Restart();
        SetRunning(true);
        statusLbl.Text = "Preparing your search";
        detailLbl.Text = "Reading the certificate into memory…";
        progressTimer.Start();
        try
        {
            // File reads and candidate counting never block the UI thread.
            activeSession = await Task.Run(() => new RecoverySession(options), cancellation.Token);
            totalCandidates = await Task.Run(() => RecoveryRunner.TotalCandidates(options, cancellation.Token), cancellation.Token);
            if (!IsDisposed)
            {
                statusLbl.Text = "Searching";
                string count = totalCandidates > 1_000_000_000 ? totalCandidates.ToString("E2") : totalCandidates.ToString("N0");
                detailLbl.Text = $"Checking {count} guesses with {workers} CPU workers. You can stop at any time.";
            }
            var result = await RecoveryRunner.RunAsync(options, activeSession.TestPassword,
                options.LogLevel == 0 ? null : activeSession.LogAttempt, workers, cancellation.Token);
            if (!closeRequested && !IsDisposed)
            {
                statusLbl.Text = result == null ? "No match in this search" : "Password found";
                detailLbl.Text = result == null
                    ? "Your file is unchanged. Try other remembered words or adjust the search settings."
                    : "Your certificate password is ready below. It is hidden until you choose to show it.";
                if (result != null)
                {
                    resultBx.Text = result;
                    resultPanel.Visible = true;
                }
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            if (!IsDisposed && !closeRequested)
            {
                statusLbl.Text = "Stopped";
                detailLbl.Text = "All workers have stopped. Starting again begins a new search.";
            }
        }
        catch (Exception error)
        {
            if (!closeRequested && !IsDisposed)
            {
                statusLbl.Text = "Could not complete the search";
                detailLbl.Text = error.GetBaseException().Message;
            }
        }
        finally
        {
            elapsed.Stop();
            completedAttempts = activeSession?.Attempts ?? 0;
            // Flush the log only after all workers have stopped.
            activeSession?.Dispose();
            string? warning = activeSession?.LogWarning;
            activeSession = null;
            recoveryCancellation = null;
            if (!IsDisposed)
            {
                progressTimer.Stop();
                RefreshProgress();
                SetRunning(false);
                if (warning != null) detailLbl.Text += " " + warning;
                if (closeRequested) Close();
            }
        }
    }

    private void StopRecovery()
    {
        if (recoveryCancellation == null) return;
        recoveryCancellation.Cancel();
        stopBtn.Enabled = false;
        statusLbl.Text = "Stopping…";
        detailLbl.Text = "Waiting for active checks to finish and flushing the log.";
    }

    private void RefreshProgress()
    {
        long attempts = activeSession?.Attempts ?? completedAttempts;
        double seconds = elapsed.Elapsed.TotalSeconds;
        double rate = seconds > 0 ? attempts / seconds : 0;
        countLbl.Text = $"{attempts:N0} guesses checked";
        speedLbl.Text = rate > 0 ? $"Speed: {rate:N0} guesses / second" : "Speed: measuring…";
        elapsedLbl.Text = $"Elapsed: {(int)elapsed.Elapsed.TotalHours:00}:{elapsed.Elapsed.Minutes:00}:{elapsed.Elapsed.Seconds:00}";
        progressBar.Value = totalCandidates > 0
            ? (int)Math.Clamp((double)attempts / (double)totalCandidates * progressBar.Maximum, 0, progressBar.Maximum) : 0;
        if (!elapsed.IsRunning)
            estimateLbl.Text = "Search is not running.";
        else if (rate > 0 && seconds >= 1)
        {
            double remaining = Math.Max(0, (double)totalCandidates - attempts) / rate;
            estimateLbl.Text = "To check all remaining guesses: " + FormatDuration(remaining) + ". A match may be found sooner.";
        }
        else estimateLbl.Text = "Remaining time: measuring…";
    }

    private static string FormatDuration(double seconds) => seconds switch
    {
        < 60 => $"{Math.Ceiling(seconds):N0} seconds",
        < 3600 => $"{seconds / 60:N1} minutes",
        < 86400 => $"{seconds / 3600:N1} hours",
        < 31557600 => $"{seconds / 86400:N1} days",
        < 31557600d * 1_000_000 => $"{seconds / 31557600:N1} years",
        _ => "more than a million years"
    };

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel) return;
        if (recoveryCancellation != null)
        {
            e.Cancel = true;
            closeRequested = true;
            StopRecovery();
        }
    }

    private void SetRunning(bool running)
    {
        maxGenBx.Enabled = minGenBx.Enabled = workersBx.Enabled = !running;
        modeBx.Enabled = charactersBx.Enabled = variationsBx.Enabled = !running;
        rememberedBx.ReadOnly = prefixBx.ReadOnly = suffixBx.ReadOnly = symbolSequenceBx.ReadOnly = running;
        startStringBx.ReadOnly = pathToCertBx.ReadOnly = logPathBx.ReadOnly = running;
        logLevelBx.Enabled = certBrowseBtn.Enabled = recoverPasswordBtn.Enabled = !running;
        stopBtn.Enabled = running;
    }

    private void certBrowseBtn_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "PFX / P12 certificates (*.pfx;*.p12)|*.pfx;*.p12", Title = "Choose your certificate" };
        if (dialog.ShowDialog(this) == DialogResult.OK) pathToCertBx.Text = dialog.FileName;
    }

    private void CopyPassword()
    {
        try
        {
            if (resultBx.Text.Length == 0) Clipboard.Clear();
            else Clipboard.SetText(resultBx.Text);
            detailLbl.Text = "Password copied to the clipboard.";
        }
        catch (System.Runtime.InteropServices.ExternalException)
        { detailLbl.Text = "The clipboard is busy. Try copying again."; }
    }
}
