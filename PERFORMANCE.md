# Performance notes

## What changed

- Read the encrypted PFX bytes once per run, instead of reopening the file for
  every attempt. Workers share read-only bytes and dispose their own certificate
  objects. The byte buffer is cleared when the run ends.
- Keep one bounded log writer instead of opening and closing the file for every
  entry. Flush successful matches immediately and flush on normal completion,
  cancellation, and errors. Logging is off by default and then skips the log lock.
- Use a bounded worker pool and update the UI four times a second rather than
  posting an update for every attempt.
- Generate guesses as needed; do not keep the entire search space in RAM.

## Local measurement — 6 November 2026

Windows, Intel Core i9-12900KF (24 logical processors), .NET 10 Release build.
A 4,278-byte local PFX was used with its owner's explicit authorization. Its
contents, password, and identifying information are not part of this repository.
The RTX 3050 was not used; this version is CPU-only.

The benchmark uses the same candidate generator and bounded runner for both
implementations, with logging disabled. It repeatedly checks the same random
nonmatching candidate to isolate validation cost. Both paths are warmed up.
Each worker count has two 1.5-second samples per implementation, with order
reversed for the second pair. Values below are arithmetic means of attempts/sec.
The interactive recovery run was stopped for this measurement.

| Workers | Reopen file each attempt | Read once into RAM | Change |
| --- | ---: | ---: | ---: |
| 1 | 3,027/s | 3,580/s | +18% |
| 4 | 11,773/s | 14,295/s | +21% |
| 12 | 20,056/s | 25,476/s | +27% |
| 24 | 14,604/s | 29,150/s | about 2.0× |

The default multithreaded UI uses half the logical processors (12 on this machine)
to leave capacity for other applications. All 24 can be selected under More options.
The measurements show that adding workers to the old file-backed path did not
always help. Choose based on measured speed and responsiveness, not core count alone.

A separate 10,000-entry logging microbenchmark took **1,579 ms** for repeated
append/open/close versus **29 ms** for the buffered writer, including its normal
timestamp and attempt-number formatting (about 55× less time in this microbenchmark).
That is not a 55× improvement in overall password recovery.

These are short local samples, not sustained benchmarks or universal speed claims.
Disk cache, background activity, CPU power/temperature, certificate algorithms,
iteration counts, and successful certificate imports all affect throughput.
The app's displayed speed includes setup and run time, so it may differ.
An earlier concurrent run was excluded because it competed for CPU resources.

Run the harness described in CONTRIBUTING.md to measure another machine or a
generated fixture. No improvement makes exhaustive searching of arbitrary long
passwords practical; narrowing the guesses remains the most important step.
