using System.Collections.Concurrent;
using System.Numerics;

namespace CertificatePasswordRecovery;

internal sealed record RecoveryOptions(int Length, string Symbols, string StartingString,
    string Prefix, string Suffix, string CertificatePath, string LogPath, int LogLevel,
    bool TrimPadding, int MinimumLength = 0, bool WordMode = false,
    string RememberedPasswords = "", bool CommonEndings = false);

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
        if (options.WordMode)
        {
            if (string.IsNullOrWhiteSpace(options.RememberedPasswords))
                throw new ArgumentException("Enter at least one word or password you remember, one per line.");
            return;
        }
        if (options.Length < 1 || options.Symbols.Length == 0)
            throw new ArgumentException("Choose a positive password length and at least one symbol.");
        if (options.StartingString.Length > options.Length ||
            options.StartingString.Any(c => !options.Symbols.Contains(c)))
            throw new ArgumentException("The starting string must fit the selected length and use only selected symbols.");
        if (options.MinimumLength < 0 || options.MinimumLength > options.Length)
            throw new ArgumentException("The shortest length cannot exceed the longest length.");
        if (options.MinimumLength > 0 && options.StartingString.Length > 0)
            throw new ArgumentException("A starting string can only be used with a single fixed length.");
    }

    internal static IEnumerable<string> RememberedCandidates(RecoveryOptions options)
    {
        foreach (var line in options.RememberedPasswords.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            // Deduplicate each word's variations without storing the entire search space.
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var word in options.CommonEndings
                ? new[] { line, line.ToLowerInvariant(), line.ToUpperInvariant(), char.ToUpperInvariant(line[0]) + line[1..] }
                : new[] { line })
            {
                if (seen.Add(word)) yield return options.Prefix + word + options.Suffix;
                if (!options.CommonEndings) continue;
                foreach (var ending in Enumerable.Range(0, 100).Select(n => n.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Concat(Enumerable.Range(1990, DateTime.Now.Year - 1990 + 1).Select(n => n.ToString(System.Globalization.CultureInfo.InvariantCulture)))
                    .Concat(new[] { "!", "123", "1234" }))
                {
                    if (seen.Add(word + ending)) yield return options.Prefix + word + ending + options.Suffix;
                }
            }
        }
    }

    internal static BigInteger TotalCandidates(RecoveryOptions options, CancellationToken cancellationToken = default)
    {
        if (options.WordMode)
        {
            BigInteger count = 0;
            foreach (var _ in RememberedCandidates(options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                count++;
            }
            return count;
        }
        if (options.MinimumLength == 0) return BigInteger.Pow(options.Symbols.Length, options.Length);
        BigInteger total = 0;
        for (int length = options.MinimumLength; length <= options.Length; length++)
            total += BigInteger.Pow(options.Symbols.Length, length);
        return total;
    }

    internal static IEnumerable<PasswordAttempt> Candidates(RecoveryOptions options,
        CancellationToken cancellationToken)
    {
        Validate(options);
        if (options.WordMode)
        {
            BigInteger index = 0;
            foreach (var word in RememberedCandidates(options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new PasswordAttempt(word, index++);
            }
            yield break;
        }
        if (options.MinimumLength > 0)
        {
            BigInteger offset = 0;
            for (int length = options.MinimumLength; length <= options.Length; length++)
            {
                foreach (var attempt in Candidates(options with { Length = length, MinimumLength = 0 }, cancellationToken))
                    yield return attempt with { Number = attempt.Number + offset };
                offset += BigInteger.Pow(options.Symbols.Length, length);
            }
            yield break;
        }
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
            var characters = new char[positions.Length];
            for (int i = 0; i < positions.Length; i++) characters[i] = options.Symbols[positions[i]];
            var text = new string(characters);
            if (options.TrimPadding) text = text.Trim();
            yield return new PasswordAttempt(options.Prefix + text + options.Suffix, number);
        }
    }

    internal static Task<string?> RunAsync(RecoveryOptions options,
        Func<PasswordAttempt, bool> testPassword, Action<PasswordAttempt, bool>? logAttempt,
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
                        if (logAttempt != null) lock (logLock) logAttempt(attempt, true);
                    }
                    loop.Stop();
                }
                else
                {
                    if (logAttempt != null) lock (logLock) logAttempt(attempt, false);
                }
            });
            return winner;
        }, cancellationToken);
    }
}
