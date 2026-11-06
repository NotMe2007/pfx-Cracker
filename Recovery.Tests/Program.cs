extern alias Original;
extern alias Modern;

using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificatePasswordRecovery;

internal static class Program
{
    private static RecoveryOptions Options => new(3, "ab", "", "", "", "", "", 0, false);

    [STAThread]
    private static void Main()
    {
        ThreadPool.SetMinThreads(8, 8);
        Run("exact candidate coverage and stable numbering", Coverage);
        Run("waits for in-flight workers and publishes one winner", Completion);
        Run("cancellation joins workers", Cancellation);
        Run("worker errors propagate", Errors);
        Run("rejects invalid inputs", Validation);
        Run("length ranges and remembered-password variations", SearchModes);
        Run("synthetic certificate and serialized file logging", Certificate);
        Console.WriteLine("All regression checks passed.");
    }

    private static void Run(string name, Func<Task> test)
    {
        test().GetAwaiter().GetResult();
        Console.WriteLine($"PASS: {name}");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static async Task Coverage()
    {
        var attempts = new ConcurrentBag<PasswordAttempt>();
        var logged = new List<PasswordAttempt>(); // Deliberately not thread-safe: runner must serialize.
        var result = await RecoveryRunner.RunAsync(Options, attempt =>
        {
            attempts.Add(attempt);
            return false;
        }, (attempt, _) => logged.Add(attempt), 4, default);
        Check(result == null && attempts.Count == 8 && logged.Count == 8, "Incorrect exhaustion.");
        Check(attempts.Select(a => a.Password).Distinct().Count() == 8, "Duplicate or missing candidate.");
        Check(attempts.OrderBy(a => a.Number).Select(a => a.Password)
            .SequenceEqual(new[] { "aab", "aba", "abb", "baa", "bab", "bba", "bbb", "aaa" }),
            "Attempt numbers drifted or candidate ordering changed.");
        var resume = Options with { StartingString = "bab", Prefix = "pre", Suffix = "post" };
        Check(RecoveryRunner.Candidates(resume, default).First().Password == "prebbapost", "Resume/affixes changed.");
        var padding = Options with { Length = 1, Symbols = " a", TrimPadding = true };
        Check(RecoveryRunner.Candidates(padding, default).Last().Password == "", "Modern padding behavior changed.");
    }

    private static async Task Completion()
    {
        using var bothEntered = new CountdownEvent(2);
        using var release = new ManualResetEventSlim();
        var firstWin = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int successes = 0, active = 0, calls = 0;
        var run = RecoveryRunner.RunAsync(Options, attempt =>
        {
            Interlocked.Increment(ref calls);
            Interlocked.Increment(ref active);
            try
            {
                bothEntered.Signal();
                Check(bothEntered.Wait(TimeSpan.FromSeconds(10)), "Workers did not start.");
                if (attempt.Number != 0)
                    Check(release.Wait(TimeSpan.FromSeconds(10)), "Worker release timed out.");
                return true;
            }
            finally { Interlocked.Decrement(ref active); }
        }, (_, found) =>
        {
            if (found) { successes++; firstWin.TrySetResult(); }
        }, 2, default);
        try
        {
            await firstWin.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Check(!run.IsCompleted && active == 1, "Returned before other worker finished.");
        }
        finally { release.Set(); }
        Check(await run == "aab", "Wrong winning password.");
        Check(successes == 1 && active == 0 && calls == 2, "Duplicate result or detached work.");
    }

    private static async Task Cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        int active = 0;
        var run = RecoveryRunner.RunAsync(Options, _ =>
        {
            Interlocked.Increment(ref active);
            entered.Set();
            try { Check(release.Wait(TimeSpan.FromSeconds(10)), "Release timeout."); return false; }
            finally { Interlocked.Decrement(ref active); }
        }, (_, _) => { }, 1, cancellation.Token);
        try
        {
            Check(entered.Wait(TimeSpan.FromSeconds(10)), "Worker never started.");
            cancellation.Cancel();
            Check(!run.IsCompleted, "Cancellation abandoned an active worker.");
        }
        finally { release.Set(); }
        try { await run; throw new Exception("Cancellation was ignored."); }
        catch (OperationCanceledException) { }
        Check(active == 0, "Worker survived cancellation.");
        Check(await RecoveryRunner.RunAsync(Options, _ => false, (_, _) => { }, 2, default) == null,
            "A cancelled run affected the next run.");
    }

    private static async Task Errors()
    {
        try
        {
            await RecoveryRunner.RunAsync(Options, _ => throw new IOException("test failure"),
                (_, _) => { }, 2, default);
            throw new Exception("Worker error swallowed.");
        }
        catch (AggregateException error)
        {
            Check(error.Flatten().InnerExceptions.All(e => e is IOException), "Wrong worker error.");
        }
    }

    private static Task Validation()
    {
        foreach (var symbols in new[] { "", "a,", "a,,b", "ab" })
        {
            try { RecoveryRunner.ParseSymbols(symbols); throw new Exception("Invalid symbols accepted."); }
            catch (ArgumentException) { }
        }
        Check(RecoveryRunner.ParseSymbols(" ,comma,a") == " ,a", "Special symbols changed.");
        try { RecoveryRunner.Validate(Options with { StartingString = "z" }); throw new Exception("Invalid start accepted."); }
        catch (ArgumentException) { }
        return Task.CompletedTask;
    }

    private static Task SearchModes()
    {
        var range = Options with { MinimumLength = 1, Length = 3 };
        var candidates = RecoveryRunner.Candidates(range, default).ToArray();
        Check(candidates.Length == 14 && candidates.Select(c => c.Password).Distinct().Count() == 14,
            "Length range skipped or duplicated guesses.");
        Check(candidates.Select(c => c.Number).SequenceEqual(Enumerable.Range(0, 14).Select(i => (BigInteger)i)),
            "Range numbering reset between lengths.");
        Check(RecoveryRunner.TotalCandidates(range) == candidates.Length, "Wrong range estimate.");
        var words = Options with { WordMode = true, RememberedPasswords = "gasket\r\nOil Filter\n", CommonEndings = true };
        var variants = RecoveryRunner.Candidates(words, default).Select(a => a.Password).ToArray();
        Check(variants.Contains("Gasket2021") && variants.Contains("Oil Filter") && variants.Contains("GASKET99"),
            "Remembered word variations are missing.");
        Check(variants.Distinct().Count() == variants.Length, "Duplicate capitalization variants.");
        Check(RecoveryRunner.TotalCandidates(words) == variants.Length, "Wrong word search estimate.");
        var exact = words with { CommonEndings = false, RememberedPasswords = "  keep spaces  \nMixedCase" };
        Check(RecoveryRunner.Candidates(exact, default).Select(a => a.Password).SequenceEqual(new[] { "  keep spaces  ", "MixedCase" }),
            "Exact remembered passwords were changed.");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        try { RecoveryRunner.Candidates(words, cancellation.Token).First(); throw new Exception("Word search ignored cancellation."); }
        catch (OperationCanceledException) { }
        return Task.CompletedTask;
    }

    private static async Task Certificate()
    {
        string directory = Path.Combine(Path.GetTempPath(), "pfx-recovery-test-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            string invalidPath = Path.Combine(directory, "invalid.pfx");
            File.WriteAllText(invalidPath, "This is not a certificate.");
            try
            {
                using var invalid = new RecoverySession(Options with { CertificatePath = invalidPath });
                throw new Exception("Invalid certificate accepted.");
            }
            catch (Exception error) when (error is CryptographicException or InvalidDataException) { }
            using var key = RSA.Create(2048);
            var request = new CertificateRequest("CN=Recovery regression test", key,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1),
                DateTimeOffset.UtcNow.AddDays(1));
            var options = Options with { CertificatePath = Path.Combine(directory, "test.pfx"),
                LogPath = Path.Combine(directory, "attempts.log"), LogLevel = 3 };
            File.WriteAllBytes(options.CertificatePath, certificate.Export(X509ContentType.Pfx, "bbb"));
            var session = new RecoverySession(options);
            Check(await RecoveryRunner.RunAsync(options, session.TestPassword, session.LogAttempt, 4, default)
                == "bbb", "Could not recover synthetic password.");
            using (var reader = new StreamReader(new FileStream(options.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                Check(reader.ReadToEnd().Split("Found Password:").Length == 2,
                    "Success was not flushed immediately, or was logged twice.");
            session.Dispose();
            File.Delete(options.LogPath);
            options = options with { Symbols = "cd" };
            session = new RecoverySession(options);
            Check(await RecoveryRunner.RunAsync(options, session.TestPassword, session.LogAttempt, 4, default)
                == null, "Incorrect password accepted.");
            session.Dispose();
            Check(File.ReadAllLines(options.LogPath).Length == 8, "Concurrent log entries were lost.");

            using (var cached = new RecoverySession(options with { LogLevel = 0 }))
            {
                string moved = Path.Combine(directory, "moved.pfx");
                File.Move(options.CertificatePath, moved);
                try { Check(cached.TestPassword(new PasswordAttempt("bbb", 0)), "Session reread the file instead of cached bytes."); }
                finally { File.Move(moved, options.CertificatePath); }
            }
            using (var failedLog = new RecoverySession(options with { LogPath = directory, LogLevel = 3 }))
            {
                Check(await RecoveryRunner.RunAsync(options with { Symbols = "ab" }, failedLog.TestPassword,
                    failedLog.LogAttempt, 2, default) == "bbb", "Log failure prevented recovery.");
                Check(failedLog.LogWarning != null, "Log failure was hidden.");
            }

            // UI lifecycle checks run on a dedicated STA thread with an actual message loop.
            UiLifecycle(() => new Original::CertificatePasswordRecovery.CertPasswordRecoveryForm(), options.CertificatePath);
            UiLifecycle(() => new Modern::CertificatePasswordRecovery.CertPasswordRecoveryForm(), options.CertificatePath);
            UiSearchWorkflow(() => new Original::CertificatePasswordRecovery.CertPasswordRecoveryForm(), options.CertificatePath);
            UiSearchWorkflow(() => new Modern::CertificatePasswordRecovery.CertPasswordRecoveryForm(), options.CertificatePath);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void UiLifecycle(Func<Form> create, string certificatePath)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                Control.CheckForIllegalCrossThreadCalls = true;
                using var form = create();
                form.ShowInTaskbar = false;
                form.Opacity = 0;
                var type = form.GetType();
                object Field(string name) => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
                form.Shown += (_, _) =>
                {
                    try
                    {
                        ((TextBox)Field("pathToCertBx")).Text = certificatePath;
                        ((ComboBox)Field("modeBx")).SelectedIndex = 1;
                        ((ComboBox)Field("charactersBx")).SelectedIndex = 3;
                        ((TextBox)Field("symbolSequenceBx")).Text = "cd";
                        ((NumericUpDown)Field("maxGenBx")).Value = 20;
                        ((ComboBox)Field("logLevelBx")).SelectedIndex = 0;
                        var start = type.GetMethod("recoverPasswordBtn_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
                        start.Invoke(form, new object?[] { form, EventArgs.Empty });
                        var cancellation = Field("recoveryCancellation");
                        Check(!((Button)Field("recoverPasswordBtn")).Enabled, "Recover button was not locked.");
                        Check(!((Button)Field("certBrowseBtn")).Enabled, "Browse button was not locked.");
                        Check(((TextBox)Field("startStringBx")).ReadOnly, "Starting string was not locked.");
                        start.Invoke(form, new object?[] { form, EventArgs.Empty });
                        Check(ReferenceEquals(cancellation, Field("recoveryCancellation")), "Overlapping run started.");
                        form.Close();
                    }
                    catch (Exception error) { failure = error; form.Close(); }
                };
                Application.Run(form);
                Check(Field("recoveryCancellation") == null, "Form closed before worker completion.");
            }
            catch (Exception error) { failure = error; }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(20)), "Form did not close after cancellation.");
        if (failure != null) throw new Exception("UI lifecycle regression.", failure);
    }

    private static void UiSearchWorkflow(Func<Form> create, string certificatePath)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = create();
                form.ShowInTaskbar = false;
                form.Opacity = 0;
                var type = form.GetType();
                object? Field(string name) => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form);
                void Start() => type.GetMethod("recoverPasswordBtn_Click", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(form, new object?[] { form, EventArgs.Empty });
                var clock = System.Diagnostics.Stopwatch.StartNew();
                using var timer = new System.Windows.Forms.Timer { Interval = 25 };
                int stage = 0;
                timer.Tick += (_, _) =>
                {
                    try
                    {
                        if (clock.Elapsed > TimeSpan.FromSeconds(15)) throw new Exception("UI search timed out.");
                        if (Field("recoveryCancellation") != null) return;
                        string status = ((Label)Field("statusLbl")!).Text;
                        var remembered = (TextBox)Field("rememberedBx")!;
                        switch (stage++)
                        {
                            case 0:
                                Check(((ComboBox)Field("logLevelBx")!).SelectedIndex == 0, "Logging is not off by default.");
                                remembered.Text = "ccc";
                                ((CheckBox)Field("variationsBx")!).Checked = false;
                                Start();
                                Check(Field("recoveryCancellation") == null && ((Label)Field("statusLbl")!).Text == "Check your settings",
                                    "Missing file was not rejected inline.");
                                ((TextBox)Field("pathToCertBx")!).Text = certificatePath;
                                Start();
                                break;
                            case 1:
                                Check(status == "No match in this search", "No-match state missing.");
                                Check(((Button)Field("recoverPasswordBtn")!).Enabled, "Start not restored after exhaustion.");
                                remembered.Text = "bbb";
                                Start();
                                break;
                            case 2:
                                Check(status == "Password found" && ((TextBox)Field("resultBx")!).Text == "bbb", "Success result missing.");
                                Check(((TextBox)Field("resultBx")!).UseSystemPasswordChar, "Result was not masked.");
                                ((ComboBox)Field("modeBx")!).SelectedIndex = 1;
                                ((ComboBox)Field("charactersBx")!).SelectedIndex = 3;
                                ((TextBox)Field("symbolSequenceBx")!).Text = "cd";
                                ((NumericUpDown)Field("minGenBx")!).Value = 20;
                                ((NumericUpDown)Field("maxGenBx")!).Value = 20;
                                Start();
                                type.GetMethod("StopRecovery", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null);
                                break;
                            case 3:
                                Check(status == "Stopped", "Stop did not return to a usable state.");
                                Check(((Button)Field("recoverPasswordBtn")!).Enabled && !((Button)Field("stopBtn")!).Enabled,
                                    "Buttons not restored after Stop.");
                                ((ComboBox)Field("modeBx")!).SelectedIndex = 0;
                                remembered.Text = "bbb";
                                Start();
                                break;
                            case 4:
                                Check(status == "Password found", "Restart after cancellation failed.");
                                timer.Stop();
                                form.Close();
                                break;
                        }
                    }
                    catch (Exception error) { failure = error; timer.Stop(); form.Close(); }
                };
                form.Shown += (_, _) => timer.Start();
                Application.Run(form);
            }
            catch (Exception error) { failure = error; }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(20)), "UI workflow did not finish.");
        if (failure != null) throw new Exception("UI search workflow regression.", failure);
    }
}
