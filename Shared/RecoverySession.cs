using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CertificatePasswordRecovery;

internal sealed class RecoverySession : IDisposable
{
    private readonly RecoveryOptions options;
    private readonly byte[] certificateData;
    private StreamWriter? writer;
    private bool loggingDisabled;
    private readonly Stopwatch flushTimer = Stopwatch.StartNew();
    private long attempts;
    internal long Attempts => Interlocked.Read(ref attempts);
    internal string LogPath { get; private set; }
    internal string? LogWarning { get; private set; }

    internal RecoverySession(RecoveryOptions options)
    {
        this.options = options;
        LogPath = options.LogPath;
        certificateData = File.ReadAllBytes(options.CertificatePath);
        if (X509Certificate2.GetCertContentType(certificateData) != X509ContentType.Pkcs12)
            throw new InvalidDataException("This file is not a PFX/P12 certificate. Choose the original certificate file.");
    }

    internal bool TestPassword(PasswordAttempt attempt)
    {
        try
        {
            // Share immutable input bytes, never certificate objects or private-key handles.
            using var certificate = X509CertificateLoader.LoadPkcs12(
                certificateData, attempt.Password, X509KeyStorageFlags.EphemeralKeySet);
            return true;
        }
        catch (Pkcs12LoadLimitExceededException) { throw; }
        catch (CryptographicException) { return false; }
        finally { Interlocked.Increment(ref attempts); }
    }

    // The runner serializes this callback. Memory is bounded by StreamWriter's buffer.
    internal void LogAttempt(PasswordAttempt attempt, bool found)
    {
        if (loggingDisabled || options.LogLevel == 0 || (!found && options.LogLevel != 3 &&
            (options.LogLevel != 2 || attempt.Number % 10000 != 0))) return;
        try
        {
            if (writer == null)
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                writer = new StreamWriter(LogPath, append: true, Encoding.UTF8, bufferSize: 16 * 1024);
            }
            writer.WriteLine($"{DateTime.Now}: Log Entry #{attempt.Number}: " +
                (found ? "Found Password: " : "Password Failed: ") + attempt.Password);
            if (found || flushTimer.Elapsed >= TimeSpan.FromSeconds(1))
            {
                writer.Flush();
                flushTimer.Restart();
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            loggingDisabled = true;
            LogWarning = "The log could not be written. Recovery continued without logging.";
        }
    }

    // Call only after the runner has joined its workers, including cancellation/error paths.
    public void Dispose()
    {
        try { writer?.Dispose(); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        { LogWarning = "The last log entries could not be saved."; }
        finally { CryptographicOperations.ZeroMemory(certificateData); }
    }
}
