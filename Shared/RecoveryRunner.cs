using System.Collections.Concurrent;
using System.Numerics;

namespace CertificatePasswordRecovery;

internal sealed record RecoveryOptions(int Length, string Symbols, string StartingString,
    string Prefix, string Suffix, string CertificatePath, string LogPath, int LogLevel,
    bool TrimPadding);

internal sealed record PasswordAttempt(string Password, BigInteger Number);

internal static class RecoveryRunner
{
    internal static string ParseSymbols(string input)
    {
        var entries = input.Split(',');
        if (entries.Any(entry => entry.Length != 1 && entry != "comma"))
            throw new ArgumentException("Use comma-separated single characters (or 'comma'). Empty entries are not allowed.");
        return new string(entries.Select(entry => entry == "comma" ? ',' : entry[0]).ToArray());
    }

    internal static void Validate(RecoveryOptions options)
    {
        if (options.Length < 1 || options.Symbols.Length == 0)
            throw new ArgumentException("Choose a positive password length and at least one symbol.");
        if (options.StartingString.Length > options.Length ||
            options.StartingString.Any(c => !options.Symbols.Contains(c)))
            throw new ArgumentException("The starting string must fit the selected length and use only selected symbols.");
    }

    internal static IEnumerable<PasswordAttempt> Candidates(RecoveryOptions options,
        CancellationToken cancellationToken)
    {
        Validate(options);
        var positions = new int[options.Length];
        for (int i = 0; i < options.StartingString.Length; i++)
            positions[positions.Length - options.StartingString.Length + i] =
                options.Symbols.IndexOf(options.StartingString[i]);

        var total = BigInteger.Pow(options.Symbols.Length, options.Length);
        for (var number = BigInteger.Zero; number < total; number++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Preserve the existing start-after-the-selected-string ordering, wrapping once.
            for (int i = positions.Length - 1; i >= 0; i--)
            {
                if (++positions[i] < options.Symbols.Length) break;
                positions[i] = 0;
            }
            var text = new string(positions.Select(i => options.Symbols[i]).ToArray());
            if (options.TrimPadding) text = text.Trim();
            yield return new PasswordAttempt(options.Prefix + text + options.Suffix, number);
        }
    }

    internal static Task<string?> RunAsync(RecoveryOptions options,
        Func<PasswordAttempt, bool> testPassword, Action<PasswordAttempt, bool> logAttempt,
        int concurrency, CancellationToken cancellationToken)
    {
        Validate(options);
        ArgumentOutOfRangeException.ThrowIfLessThan(concurrency, 1);
        return Task.Run(() =>
        {
            string? winner = null;
            var logLock = new object();
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = concurrency,
                CancellationToken = cancellationToken
            };
            // No detached tasks: Parallel.ForEach joins every in-flight attempt before returning.
            Parallel.ForEach(Partitioner.Create(Candidates(options, cancellationToken),
                EnumerablePartitionerOptions.NoBuffering), parallelOptions, (attempt, loop) =>
            {
                if (Volatile.Read(ref winner) != null || loop.IsStopped) return;
                cancellationToken.ThrowIfCancellationRequested();
                bool found = testPassword(attempt);
                if (found)
                {
                    if (Interlocked.CompareExchange(ref winner, attempt.Password, null) == null)
                    {
                        lock (logLock) logAttempt(attempt, true);
                    }
                    loop.Stop();
                }
                else
                {
                    lock (logLock) logAttempt(attempt, false);
                }
            });
            return winner;
        }, cancellationToken);
    }
}
