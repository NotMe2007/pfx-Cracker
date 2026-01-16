using System;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace CertificatePasswordRecovery;

public partial class CertPasswordRecoveryForm : Form
{
    private int[] password = Array.Empty<int>();
    private char[] sequence = Array.Empty<char>();
    private int logLevel; // 0: Off, 1: Success Only, 2: Every 10,000 + Success, 3: Everything
    private X509Certificate2? certificate;

    public CertPasswordRecoveryForm()
    {
        InitializeComponent();

        logLevelBx.SelectedIndex = 2;
        logLevel = logLevelBx.SelectedIndex;

        startStringBx.MaxLength = (int)maxGenBx.Value;

        // Set a safe default log path in the user's Documents folder
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        logPathBx.Text = Path.Combine(documentsPath, "CertificatePasswordRecoveryLog.txt");
    }

    private void maxGenBx_ValueChanged(object? sender, EventArgs e)
    {
        // TODO: Add logic for when maxGenBx value changes
    }

    private void helpAboutLinkLbl_Click(object? sender, EventArgs e)
    {
        // TODO: Show help/about dialog
    }

    private void recoverPasswordBtn_Click(object? sender, EventArgs e)
    {
        RecoverPassword();
    }

    private void logLevelBx_SelectedIndexChanged(object? sender, EventArgs e)
    {
        logLevel = logLevelBx.SelectedIndex;
    }

    private void certBrowseBtn_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Certificate Files (*.pfx;*.p12;*.cer)|*.pfx;*.p12;*.cer|PFX Files (*.pfx)|*.pfx|P12 Files (*.p12)|*.p12|CER Files (*.cer)|*.cer|All Files (*.*)|*.*",
            FilterIndex = 1,
            Title = "Select Certificate File"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            pathToCertBx.Text = ofd.FileName;
        }
    }

    private void RecoverPassword()
    {
        if (!ValidateInputs())
            return;

        DisableControls();

        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.Visible = true;

        var passwordRecoveryWorker = new BackgroundWorker();
        passwordRecoveryWorker.DoWork += PasswordRecoveryWorker_DoWork;
        passwordRecoveryWorker.RunWorkerCompleted += PasswordRecoveryWorker_RunWorkerCompleted;
        passwordRecoveryWorker.RunWorkerAsync();
    }

    private void PasswordRecoveryWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        password = new int[(int)maxGenBx.Value];
        sequence = ParseSymbolSequence(symbolSequenceBx.Text);

        SetStartingString();
        PerformBruteForce();
    }

    private void PasswordRecoveryWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        EnableControls();

        progressBar.Style = ProgressBarStyle.Blocks;
        progressBar.Visible = false;
    }

    private void SetStartingString()
    {
        var startingChars = startStringBx.Text.ToCharArray();
        Array.Reverse(startingChars);

        var startingCharsConverted = new int[startingChars.Length];
        for (var i = 0; i < startingChars.Length; i++)
        {
            startingCharsConverted[i] = Array.IndexOf(sequence, startingChars[i]);
        }

        for (int i = password.Length - 1, pos = 0; i >= 0 && pos < startingCharsConverted.Length; i--, pos++)
        {
            password[i] = startingCharsConverted[pos];
        }
    }

    private void PerformBruteForce()
    {
        var repeatUntil = BigInteger.Pow(sequence.Length, password.Length);
        var i = BigInteger.Zero;

        do
        {
            IncrementPassword();

            var pw = BuildPasswordString();
            if (TestPassword(pathToCertBx.Text, pw, logPathBx.Text, i))
            {
                MessageBox.Show($"Password found: {pw}", "Success");
                return;
            }

            i++;
        } while (i < repeatUntil);

        MessageBox.Show("Password not found.", "Info");
    }

    private void IncrementPassword()
    {
        var depth = password.Length - 1;
        password[depth]++;

        for (var i = depth; i >= 0; i--)
        {
            if (password[i] >= sequence.Length)
            {
                password[i] = 0;
                if (i > 0)
                    password[i - 1]++;
            }
        }
    }

    private string BuildPasswordString()
    {
        var passwordSequenced = new char[password.Length];
        for (var i = 0; i < password.Length; i++)
        {
            passwordSequenced[i] = sequence[password[i]];
        }

        var pw = new string(passwordSequenced);
        if (!string.IsNullOrEmpty(prefixBx.Text))
            pw = prefixBx.Text + pw;
        if (!string.IsNullOrEmpty(suffixBx.Text))
            pw += suffixBx.Text;

        return pw;
    }

    private bool TestPassword(string certificatePath, string certificatePassword, string logFileLocation, BigInteger passwordNumber)
    {
        try
        {
            certificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword);

            if (logLevel != 0)
                Log($"Found Password: {certificatePassword}", logFileLocation, passwordNumber);

            return true;
        }
        catch
        {
            if (logLevel == 2 && passwordNumber % 10000 == 0 || logLevel == 3)
                Log($"Password Failed: {certificatePassword}", logFileLocation, passwordNumber);

            return false;
        }
    }

    private void Log(string logMessage, string logFile, BigInteger passwordNumber)
    {
        try
        {
            // Ensure the directory exists
            string? directory = Path.GetDirectoryName(logFile);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var logger = File.AppendText(logFile);
            logger.WriteLine($"{DateTime.Now}: {logMessage}");
        }
        catch (UnauthorizedAccessException)
        {
            // If we can't write to the specified location, try the user's temp folder
            try
            {
                string tempLogFile = Path.Combine(Path.GetTempPath(), "CertificatePasswordRecoveryLog.txt");
                using var logger = File.AppendText(tempLogFile);
                logger.WriteLine($"{DateTime.Now}: {logMessage}");
                logger.WriteLine($"{DateTime.Now}: Original log path was inaccessible: {logFile}");
                
                // Update the UI to show the new log path (invoke required for thread safety)
                if (logPathBx.InvokeRequired)
                {
                    logPathBx.Invoke(() => logPathBx.Text = tempLogFile);
                }
                else
                {
                    logPathBx.Text = tempLogFile;
                }
            }
            catch
            {
                // If all else fails, just ignore logging to prevent crashes
            }
        }
        catch
        {
            // For any other exception, just ignore logging to prevent crashes
        }
    }

    private char[] ParseSymbolSequence(string input)
    {
        var candidates = input.Split(',');
        var sequence = new char[candidates.Length];

        for (var i = 0; i < candidates.Length; i++)
        {
            sequence[i] = candidates[i] == "comma" ? ',' : candidates[i][0];
        }

        return sequence;
    }

    private bool ValidateInputs()
    {
        if (!File.Exists(pathToCertBx.Text))
        {
            MessageBox.Show("No certificate file selected.", "Error");
            return false;
        }

        if (logLevel != 0 && string.IsNullOrEmpty(logPathBx.Text))
        {
            MessageBox.Show("No log file path selected.", "Error");
            return false;
        }

        return true;
    }

    private void DisableControls()
    {
        maxGenBx.ReadOnly = true;
        prefixBx.ReadOnly = true;
        suffixBx.ReadOnly = true;
        pathToCertBx.ReadOnly = true;
        logPathBx.ReadOnly = true;
        logLevelBx.Enabled = false;
        recoverPasswordBtn.Enabled = false;
    }

    private void EnableControls()
    {
        maxGenBx.ReadOnly = false;
        prefixBx.ReadOnly = false;
        suffixBx.ReadOnly = false;
        pathToCertBx.ReadOnly = false;
        logPathBx.ReadOnly = false;
        logLevelBx.Enabled = true;
        recoverPasswordBtn.Enabled = true;
    }
}
