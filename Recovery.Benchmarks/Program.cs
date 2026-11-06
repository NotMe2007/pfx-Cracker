using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificatePasswordRecovery;

// A bounded throughput test, not a password search. Never prints certificate data or passwords.
string temporary = Path.Combine(Path.GetTempPath(), "recovery-benchmark-" + Guid.NewGuid());
Directory.CreateDirectory(temporary);
try
{
    string path;
    if (args.Length == 0)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=Temporary benchmark", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
        path = Path.Combine(temporary, "fixture.pfx");
        File.WriteAllBytes(path, certificate.Export(X509ContentType.Pfx, Guid.NewGuid().ToString("N")));
        Console.WriteLine("Fixture: generated certificate");
    }
    else
    {
        path = Path.GetFullPath(args[0]);
        Console.WriteLine("Fixture: user-selected local file (contents and path omitted)");
    }
    string wrongPassword = "benchmark-" + Guid.NewGuid().ToString("N");
    var options = new RecoveryOptions(1, "a", "", "", "", path, "", 0, false);
    using var session = new RecoverySession(options);
    bool FromFile(PasswordAttempt attempt)
    {
        try
        {
            using var cert = X509CertificateLoader.LoadPkcs12FromFile(path, attempt.Password, X509KeyStorageFlags.EphemeralKeySet);
            return true;
        }
        catch (Pkcs12LoadLimitExceededException) { throw; }
        catch (CryptographicException) { return false; }
    }
    var attempt = new PasswordAttempt(wrongPassword, 0);
    // Warm both paths; a random match is handled without printing the password.
    if (FromFile(attempt) || session.TestPassword(attempt)) throw new InvalidOperationException("Benchmark candidate was accepted; benchmark stopped.");
    Console.WriteLine("mode,workers,sample,attempts,seconds,attempts_per_second");
    foreach (int workers in new[] { 1, 4, Math.Max(1, Environment.ProcessorCount / 2), Environment.ProcessorCount }.Distinct())
    {
        for (int sample = 1; sample <= 2; sample++)
        {
            foreach (var mode in sample == 1 ? new[] { "file", "ram" } : new[] { "ram", "file" })
            {
                Func<PasswordAttempt, bool> test = mode == "file" ? FromFile : session.TestPassword;
                long count = 0;
                var watch = Stopwatch.StartNew();
                using var timeout = new CancellationTokenSource();
                try
                {
                    await RecoveryRunner.RunAsync(options with { Length = 10, Symbols = "0123456789" }, _ =>
                    {
                        // Check the deadline on workers: a saturated pool can delay timer callbacks.
                        if (watch.Elapsed >= TimeSpan.FromSeconds(1.5)) timeout.Cancel();
                        timeout.Token.ThrowIfCancellationRequested();
                        if (test(attempt)) throw new InvalidOperationException("Benchmark candidate accepted; stopped.");
                        Interlocked.Increment(ref count);
                        return false;
                    }, null, workers, timeout.Token);
                }
                catch (OperationCanceledException) when (timeout.IsCancellationRequested) { }
                watch.Stop();
                Console.WriteLine(FormattableString.Invariant($"{mode},{workers},{sample},{count},{watch.Elapsed.TotalSeconds:F3},{count / watch.Elapsed.TotalSeconds:F1}"));
            }
        }
    }
    // Isolate log I/O cost; buffered entries also include normal progress metadata.
    const int entries = 10000;
    string message = "Benchmark progress entry";
    var timer = Stopwatch.StartNew();
    string baselineLog = Path.Combine(temporary, "baseline.log");
    for (int i = 0; i < entries; i++) File.AppendAllText(baselineLog, message + Environment.NewLine);
    timer.Stop();
    double baselineMs = timer.Elapsed.TotalMilliseconds;
    timer.Restart();
    using (var log = new RecoverySession(options with { LogLevel = 3, LogPath = Path.Combine(temporary, "buffered.log") }))
        for (int i = 0; i < entries; i++) log.LogAttempt(new PasswordAttempt(message, i), false);
    timer.Stop();
    Console.WriteLine(FormattableString.Invariant($"Logging {entries} entries: append-per-entry={baselineMs:F1}ms; buffered={timer.Elapsed.TotalMilliseconds:F1}ms"));
}
finally { Directory.Delete(temporary, true); }
