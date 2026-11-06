# Development conventions

Use Odoo-style uppercase prefixes in commit subjects:

- `[FIX]`: bug fixes.
- `[IMP]`: improvements to existing functionality.
- `[ADD]`: new functionality.
- `[REF]`: refactoring without a behavior change.
- `[REM]`: removal of functionality or obsolete code.
- `[DOC]`: documentation changes.

Describe the affected area and the concrete change after the prefix. Keep unrelated
changes in separate commits. Verify fixes before pushing them.

## Validation

On Windows with the .NET 10 SDK:

```powershell
dotnet build CertificatePasswordRecovery.sln --configuration Release
dotnet run --project Recovery.Tests --configuration Release
```

`Recovery.Tests` is a dependency-free executable regression suite; a failed check
exits with an error. It tests candidate coverage, joined worker completion,
simultaneous successes, cancellation, exception propagation, serialized logging,
and both WinForms lifecycles with cross-thread checks enabled. Certificates are
generated temporarily for tests and deleted afterward. Never use real certificates
or commit password logs; see SECURITY.md.

## Performance measurements

Run the bounded benchmark with a generated temporary certificate:

```powershell
dotnet run --project Recovery.Benchmarks --configuration Release
```

For a local certificate that its owner has explicitly authorized for testing,
append `-- "C:\path\to\certificate.pfx"`. This bypasses the default synthetic-only
test policy only for that authorized file. The benchmark checks a random
candidate repeatedly; it does not attempt a dictionary or exhaustive recovery.
It compares file-backed and RAM-backed checks through the same runner, uses
short bounded samples, and prints throughput rather than passwords or file data.
Close other recovery runs for a meaningful comparison. Never commit the
certificate, its contents, or logs of real guesses.
