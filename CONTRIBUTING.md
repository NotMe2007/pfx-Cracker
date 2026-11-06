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
