using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertificatePasswordRecovery;

internal sealed class RecoverySession(RecoveryOptions options)
{
    // Only read by the UI after RunAsync has joined all workers.
    internal string LogPath { get; private set; } = options.LogPath;

    internal bool TestPassword(PasswordAttempt attempt)
    {
        try
        {
            using var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                options.CertificatePath, attempt.Password, X509KeyStorageFlags.EphemeralKeySet);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    // The runner serializes this callback, including fallback-path changes.
    internal void LogAttempt(PasswordAttempt attempt, bool found)
    {
        if (options.LogLevel == 0 || (!found && options.LogLevel != 3 &&
            (options.LogLevel != 2 || attempt.Number % 10000 != 0))) return;
        var message = $"{DateTime.Now}: Log Entry #{attempt.Number}: " +
            (found ? "Found Password: " : "Password Failed: ") + attempt.Password;
        try
        {
            WriteLog(message);
        }
        catch (UnauthorizedAccessException)
        {
            LogPath = Path.Combine(Path.GetTempPath(), "CertificatePasswordRecoveryLog.txt");
            try { WriteLog(message); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        catch (IOException) { }
    }

    private void WriteLog(string message)
    {
        var directory = Path.GetDirectoryName(LogPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        File.AppendAllText(LogPath, message + Environment.NewLine);
    }
}
