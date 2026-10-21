# Developer & AI Agent Quickstart: START HERE

Welcome to **StudyAI Campus** — a production-grade, multi-application academic SaaS platform.

This guide is the single entry point for all software engineers and AI coding agents (OpenAI Codex, OpenCode, Google Antigravity).

---

## 1. Core Operating Principles

1. **Understand Before Modify**: Never write code without tracing the execution flow and checking caller/callee boundaries ([`.agents/rules/mandatory-map-check.md`](../.agents/rules/mandatory-map-check.md)).
2. **Progressive Context Loading**: Do NOT dump or scan the entire repository. Follow the navigation model:
   $$\text{Project} \rightarrow \text{Application} \rightarrow \text{Module} \rightarrow \text{Feature} \rightarrow \text{Target File}$$
3. **Smallest Safe Change**: Keep changes focused, minimal, and verified by tests before claiming completion.
4. **Authority Hierarchy**: When requirements conflict, follow [`docs/INDEX.md`](INDEX.md) (Product Owner > Final PRD > Core Architecture v1.2 > Database Review v1.1 > SQL Candidate > Historical docs).
5. **OpenSpec is Canonical for Implementation**: Feature and system code changes are tracked exclusively in `openspec/changes/<change-id>/`. Do not maintain a competing task tracker.

---

## 2. Fast Navigation Index

| Need | Document / Location | Description |
|---|---|---|
| **Operating Contract** | [`AGENTS.md`](../AGENTS.md) | Universal operating rules, safety invariants, and constraints. |
| **Living System Map** | [`SYSTEM_MAP.md`](../SYSTEM_MAP.md) | High-level system architecture, boundaries, flows, and entry points. |
| **Current Project State** | [`docs/development/CURRENT_STATE.md`](development/CURRENT_STATE.md) | Live active tasks, working branches, blockers, and build status. |
| **Authority & Documents** | [`docs/INDEX.md`](INDEX.md) | Authoritative source order, status of all PRDs, ADRs, and schemas. |
| **Active OpenSpec Changes** | [`openspec/changes/`](../openspec/changes/) | Canonical change proposals, designs, specs, and implementation tasks. |
| **Module Navigation** | [`docs/codebase/MODULE_INDEX.md`](codebase/MODULE_INDEX.md) | Directory of all applications, modules, APIs, and test suites. |
| **Feature Mapping** | [`docs/codebase/FEATURE_MAP.md`](codebase/FEATURE_MAP.md) | Map connecting user-facing features to code implementations. |
| **Codebase File Index** | [`docs/codebase/FILE_INDEX.md`](codebase/FILE_INDEX.md) | Aggregate catalog of source files, dependencies, and verification status. |
| **Module File Catalogs** | [`docs/codebase/catalog/`](codebase/catalog/) | Per-module structured file catalogs with VERIFIED/NEEDS_REVIEW status. |
| **Open Decisions** | [`docs/adr/OPEN_DECISIONS.md`](adr/OPEN_DECISIONS.md) | Unresolved technical and product decisions queue. |
| **Database Security Operations** | [`docs/development/DATABASE_SECURITY_BOOTSTRAP_RUNBOOK.md`](development/DATABASE_SECURITY_BOOTSTRAP_RUNBOOK.md) | Secret-free role bootstrap, migrations, runtime grants, clean-room checks, and credential rotation. |
| **Authentication Security** | [docs/development/AUTHENTICATION_SECURITY_RUNBOOK.md](development/AUTHENTICATION_SECURITY_RUNBOOK.md) | CSRF exchange, authentication request limits, and trusted proxy configuration. |
| **Browser Routing Contract** | [docs/development/BROWSER_ROUTING_CONTRACT.md](development/BROWSER_ROUTING_CONTRACT.md) | Next.js browser proxy routing, CSRF token acquisition, and cookie security. |

---

## 3. Technology Stack Baseline

- **API Host**: ASP.NET Core (`net10.0`), C# 14 (`LangVersion=14.0`), Native Microsoft Testing Platform (MTP), xUnit v3, Central Package Management (`Directory.Packages.props`), Locked dependencies.
- **Web App**: Next.js and TypeScript (Planned V1).
- **Mobile**: React Native / Expo (Future V1+).
- **Database**: PostgreSQL 16+ with `pgvector`, `pg_trgm`, `btree_gist`, EF Core + Npgsql (Single DbContext initially).
- **Object Storage**: Private S3-compatible storage with short-lived pre-signed URLs.
- **Workers**: Independent background workers (.NET background services and Python document processors).

---

## 4. How to Start a Work Session

### For an AI Coding Agent
1. **Load Context Incrementally**:
   - Read [`AGENTS.md`](../AGENTS.md).
   - Read [`docs/development/CURRENT_STATE.md`](development/CURRENT_STATE.md) to locate the active OpenSpec change or task.
   - For implementation changes, consult `openspec/changes/<change-id>/tasks.md`. Do NOT create duplicate task tables in `docs/plans/`.
   - For non-implementation governance/research work only, use `docs/plans/<TASK-ID>/`.
2. **Consult the Map**:
   - Check [`SYSTEM_MAP.md`](../SYSTEM_MAP.md) to identify the target module.
   - Consult [`docs/codebase/MODULE_INDEX.md`](codebase/MODULE_INDEX.md) and [`docs/codebase/catalog/`](codebase/catalog/) for existing code references.
3. **Analyze and Trace**:
   - Follow [`.agents/rules/mandatory-map-check.md`](../.agents/rules/mandatory-map-check.md).
4. **Implement & Test**:
   - Follow test-driven development: write tests first or alongside implementation.
   - Run tests using repository commands:
     ```powershell
     dotnet test --solution StudyAI.slnx --no-restore
     ```
5. **Update State & Handoff**:
   - Run documentation sync and validation:
     ```powershell
     python scripts/docs/sync_codebase_index.py --check
     python scripts/docs/validate_docs.py
     ```
   - Update `openspec/changes/<change-id>/tasks.md`.
   - Record resumable progress in `docs/plans/<TASK-ID>/HANDOFF.md` linking directly to the OpenSpec change.
   - Update [`docs/development/CURRENT_STATE.md`](development/CURRENT_STATE.md).

---

## 5. Development Commands Reference

| Action | Command | Notes |
|---|---|---|
| **Restore Dependencies** | `dotnet restore StudyAI.slnx --locked-mode` | Enforces exact locked NuGet packages |
| **Build Solution** | `dotnet build StudyAI.slnx --no-restore` | Fast zero-restore compilation |
| **Run All Tests** | `dotnet test --solution StudyAI.slnx --no-restore` | Native MTP test runner |
| **Run Unit Tests** | `dotnet test --project tests/StudyAI.UnitTests/StudyAI.UnitTests.csproj` | Fast isolated test suite |
| **Run Integration Tests** | `dotnet test --project tests/StudyAI.Api.IntegrationTests/StudyAI.Api.IntegrationTests.csproj` | Host integration suite |
| **Run Architecture Tests** | `dotnet test --project tests/StudyAI.ArchitectureTests/StudyAI.ArchitectureTests.csproj` | Dependency rule verification |
| **Check Doc Sync** | `python scripts/docs/sync_codebase_index.py --check` | Verifies codebase file index & catalog integrity |
| **Sync Doc Index** | `python scripts/docs/sync_codebase_index.py` | Refreshes codebase file index & catalog |
| **Validate Doc Health** | `python scripts/docs/validate_docs.py` | Verifies doc links, catalog integrity, skills |
| **Validate Local DB** | `powershell -File scripts/Validate-Database.ps1` | Docker PostgreSQL runtime validation |
| **Validate Clean DB Bootstrap** | `pwsh -NoProfile -File scripts/Test-CleanRoomPostgresBootstrap.ps1` | Ephemeral no-volume PostgreSQL/ACL acceptance test |
