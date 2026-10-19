# ADR-015: Backend Foundation Toolchain

Status: **ACCEPTED**  
Decision date: 2026-10-19  
Decision owner: Product Owner / technical owner

## Context

Backend Foundation — Iteration 1 needs a supported, reproducible .NET toolchain before any application project is created. The decision covers only the host and test foundation. It does not approve Entity Framework Core, Npgsql, pgvector, authentication, migrations, RLS, queues, storage, AI, billing, or production topology.

The local machine has .NET SDK `10.0.300`. Official package metadata available on the decision date shows that xUnit v3 `4.0.2` and Microsoft Testing Platform `2.5.1` support .NET 8 or later, while the selected ASP.NET Core testing and OpenAPI packages target .NET 10. The .NET 10 CLI supports native Microsoft Testing Platform selection through `global.json`.

## Decision

1. Target `net10.0` for all Backend Foundation — Iteration 1 projects.
2. Pin the repository SDK floor to `10.0.300`, disallow previews, and allow only the latest installed patch in the `10.0.3xx` feature band.
3. Select native `Microsoft.Testing.Platform` mode in `global.json`.
4. Use xUnit v3 test executables and the native MTP command form. Do not add `Microsoft.NET.Test.Sdk`, a VSTest adapter, or the VSTest bridge separator.
5. Manage all direct and selected transitive package versions centrally in `Directory.Packages.props`.
6. Pin these exact foundation versions:

   | Package | Version | Purpose |
   |---|---:|---|
   | `xunit.v3` | `4.0.2` | xUnit v3 framework and MTP v2 runner |
   | `Microsoft.Testing.Platform` | `2.5.1` | Centrally pinned native test platform dependency |
   | `Microsoft.AspNetCore.Mvc.Testing` | `10.0.13` | In-process API integration host |
   | `Microsoft.AspNetCore.OpenApi` | `10.0.13` | Built-in development OpenAPI document support |

7. Generate and commit NuGet `packages.lock.json` files for projects with package dependencies. Routine verification uses locked restore; intentional dependency updates regenerate and review the lock files.
8. Treat warnings as errors and keep the foundation dependency surface limited to the packages listed above unless a later accepted decision adds another package.

## Standard commands

Run from the repository root:

```powershell
dotnet restore StudyAI.slnx --locked-mode
dotnet build StudyAI.slnx --no-restore
dotnet test --solution StudyAI.slnx --no-restore
```

Native MTP mode uses `--solution` and does not use the VSTest bridge separator (`--`). A single native-MTP test project uses:

```powershell
dotnet test --project tests/StudyAI.UnitTests/StudyAI.UnitTests.csproj --no-restore
```

## Consequences

- Restore output is reproducible and fails when project dependencies drift from reviewed lock files.
- Test projects are executable MTP test applications and require the .NET 10 native test runner selected by the repository.
- Contributors with only .NET 8 or .NET 9 cannot run this solution without installing a compatible .NET 10 SDK.
- Patch updates are deliberate package decisions. The SDK may advance only inside the accepted `10.0.3xx` feature band through `latestPatch` roll-forward.
- The repository must prove compatibility by locked restore, build, and actual test execution before this implementation change is complete.

## Alternatives considered

- **.NET 8 LTS:** rejected for this new foundation because its support window ends in November 2026 and would force an immediate framework upgrade.
- **.NET 11 prerelease:** rejected because previews and release candidates are not the stable backend baseline.
- **VSTest compatibility packages:** rejected because the approved .NET 10 native MTP path does not need them.
- **Floating package ranges:** rejected because they make restore results non-deterministic.

## Rollback and update policy

Before dependent implementation is merged, rollback is removal of the foundation projects and this decision. After dependent work exists, SDK or package changes require a new reviewed ADR amendment, regenerated lock files, and fresh restore/build/test evidence. No database or external infrastructure rollback is involved.

## Evidence gate

The version selection is accepted. A failure at a later gate reopens the affected pin instead of silently switching framework, runner, or package versions.

Verification on 2026-10-19 established executable compatibility for this foundation:

| Command | Result |
|---|---|
| `dotnet --version` | `10.0.300` |
| `dotnet restore StudyAI.slnx --locked-mode` | Exit `0`; all projects up to date |
| `dotnet build StudyAI.slnx --no-restore` | Exit `0`; 0 warnings, 0 errors |
| `dotnet test --solution StudyAI.slnx --no-restore` | Exit `0`; 28 passed, 0 failed, 0 skipped |

This evidence applies only to the accepted foundation packages. EF Core, Npgsql, pgvector, authentication, database migration, and provider compatibility remain unapproved and untested by this ADR.
