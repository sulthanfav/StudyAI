# Tasks

## 1. Accept and pin the foundation toolchain

- [ ] 1.1 Obtain explicit technical-owner acceptance of the .NET 10 SDK feature band, native Microsoft Testing Platform, xUnit v3, central package management, and lock-file policy proposed in `design.md`; record the accepted decision in `docs/adr` and verify its status is ACCEPTED before creating projects.
- [ ] 1.2 Add `global.json`, `Directory.Build.props`, `Directory.Packages.props`, and NuGet lock configuration with only accepted exact versions; verify `dotnet --version` resolves the accepted SDK and `dotnet restore StudyAI.slnx --locked-mode` succeeds after initial lock-file generation.
- [ ] 1.3 Document the selected native MTP runner and root verification commands beside the dependency decision; verify the documented test form is `dotnet test --solution StudyAI.slnx` with no VSTest bridge separator.

## 2. Scaffold and enforce the modular-monolith boundary

- [ ] 2.1 Create `StudyAI.slnx` with minimal `StudyAI.Domain`, `StudyAI.Application`, `StudyAI.Infrastructure`, `StudyAI.Contracts`, and `StudyAI.Api` projects plus unit, architecture, and API integration test projects; verify the solution inventory contains exactly the intended projects and no feature or provider package.
- [ ] 2.2 Add only the dependency direction defined in `design.md` and explicit API composition registration; verify `dotnet build StudyAI.slnx --no-restore` succeeds without circular references.
- [ ] 2.3 Add architecture tests for forbidden reverse references and accidental API-host dependencies, then verify them with `dotnet test --project tests/StudyAI.ArchitectureTests/StudyAI.ArchitectureTests.csproj`.
- [ ] 2.4 Document the project responsibilities and reference rules in the developer-facing architecture index, then verify every new documentation link resolves from `docs/INDEX.md`.

## 3. Implement safe host configuration and request diagnostics

- [ ] 3.1 Add typed foundation options with startup validation and unit tests for valid, missing, and malformed values; verify failures name the configuration key but never the supplied value.
- [ ] 3.2 Configure built-in structured JSON console logging and add a bounded `X-Correlation-ID` middleware with unit/API tests for accepted, generated, oversized, and control-character identifiers; verify one identifier is shared by response and log scope.
- [ ] 3.3 Add redaction tests using canary credentials, authorization headers, signed URLs, prompts, notes, and document text; verify captured default logs contain none of the canary values or request/response bodies.
- [ ] 3.4 Document safe logging and configuration rules with the implementation, then verify examples contain placeholders only and the repository secret scan reports no real credential.

## 4. Implement the host health contract

- [ ] 4.1 Add `/health/live` as a process-only endpoint and API tests proving it succeeds without database, queue, storage, auth, or AI settings.
- [ ] 4.2 Add `/health/ready` with tagged readiness checks and tests for healthy and injected-unhealthy states; verify the unhealthy response is non-success and both responses exclude exception details, configuration values, connection strings, and private content.
- [ ] 4.3 Add an API surface test proving the foundation host exposes no business, authentication, persistence, queue, storage, AI, billing, or tenant-authorization endpoint.
- [ ] 4.4 Document the health endpoint semantics and current absence of external dependency checks; verify README and `docs/INDEX.md` do not describe the foundation as tenant-secure, database-ready, or production-ready.

## 5. Verify the complete foundation without crossing readiness gates

- [ ] 5.1 From a clean workspace, run `dotnet restore StudyAI.slnx --locked-mode`, `dotnet build StudyAI.slnx --no-restore`, and `dotnet test --solution StudyAI.slnx --no-restore`; record exit codes and the literal passed/failed/skipped totals.
- [ ] 5.2 Inspect project/package references and source for EF Core, `DbContext`, migrations, auth/session code, RLS SQL, queues, storage providers, AI/billing logic, generic repositories, and fake relational providers; verify none were introduced by this change.
- [ ] 5.3 Reconcile the implementation against every `platform/api-host` scenario and update affected README, roadmap, ADR, and `docs/INDEX.md` entries with evidence and remaining gates; verify OpenSpec strict validation succeeds and no success claim exceeds the recorded command output.

## Workflow follow-up

- Review the implementation and verification evidence before marking tasks complete.
- Archive the change only after all acceptance scenarios pass and documentation reflects the resulting status.
