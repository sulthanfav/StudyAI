# Developer & AI Agent Quickstart: START HERE

Welcome to **StudyAI Campus** — a production-grade, multi-application academic SaaS platform.

This guide is the single entry point for all software engineers and AI coding agents (OpenAI Codex, OpenCode, Google Antigravity).

---

## 1. Core Operating Principles

1. **Understand Before Modify**: Never write code without tracing the execution flow and checking caller/callee boundaries (`.agents/rules/mandatory-map-check.md`).
2. **Progressive Context Loading**: Do NOT dump or scan the entire repository. Follow the navigation model:
   $$\text{Project} \rightarrow \text{Application} \rightarrow \text{Module} \rightarrow \text{Feature} \rightarrow \text{Target File}$$
3. **Smallest Safe Change**: Keep changes focused, minimal, and verified by tests before claiming completion.
4. **Authority Hierarchy**: When requirements conflict, follow `docs/INDEX.md` (Product Owner > Final PRD > Core Architecture v1.2 > Database Review v1.1 > SQL Candidate > Historical docs).

---

## 2. Fast Navigation Index

| Need | Document / Location | Description |
|---|---|---|
| **Operating Contract** | [`AGENTS.md`](../AGENTS.md) | Universal operating rules, safety invariants, and constraints. |
| **Living System Map** | [`SYSTEM_MAP.md`](../SYSTEM_MAP.md) | High-level system architecture, boundaries, flows, and entry points. |
| **Current Project State** | [`docs/development/CURRENT_STATE.md`](development/CURRENT_STATE.md) | Live active tasks, working branches, blockers, and build status. |
| **Authority & Documents** | [`docs/INDEX.md`](INDEX.md) | Authoritative source order, status of all PRDs, ADRs, and schemas. |
| **Module Navigation** | [`docs/codebase/MODULE_INDEX.md`](codebase/MODULE_INDEX.md) | Directory of all applications, modules, APIs, and test suites. |
| **Feature Mapping** | [`docs/codebase/FEATURE_MAP.md`](codebase/FEATURE_MAP.md) | Map connecting user-facing features to code implementations. |
| **Codebase File Index** | [`docs/codebase/FILE_INDEX.md`](codebase/FILE_INDEX.md) | Aggregate catalog of source files, dependencies, and verification status. |
| **Module File Catalogs** | [`docs/codebase/catalog/`](codebase/catalog/) | Per-module structured file catalogs with VERIFIED/NEEDS_REVIEW status. |
| **Open Decisions** | [`docs/adr/OPEN_DECISIONS.md`](adr/OPEN_DECISIONS.md) | Unresolved technical and product decisions queue. |

---

## 3. Technology Stack Baseline

- **API Host**: ASP.NET Core (`net10.0`), C# 13, Native Microsoft Testing Platform (MTP), xUnit v3, Central Package Management (`Directory.Packages.props`), Locked dependencies.
- **Web App**: Next.js and TypeScript (Planned V1).
- **Mobile**: React Native / Expo (Future V1+).
- **Database**: PostgreSQL 16+ with `pgvector`, `pg_trgm`, `btree_gist`, EF Core + Npgsql.
- **Object Storage**: Private S3-compatible storage with short-lived pre-signed URLs.
- **Workers**: Independent background workers (.NET background services and Python document processors).

---

## 4. How to Start a Work Session

### For an AI Coding Agent
1. **Load Context Incrementally**:
   - Read [`AGENTS.md`](../AGENTS.md).
   - Read [`docs/development/CURRENT_STATE.md`](development/CURRENT_STATE.md) to locate the active task or branch.
   - If starting a new feature, read [`docs/plans/TASK-TEMPLATE/`](plans/TASK-TEMPLATE/) and create `docs/plans/<TASK-ID>/`.
2. **Consult the Map**:
   - Check [`SYSTEM_MAP.md`](../SYSTEM_MAP.md) to identify the target module.
   - Consult [`docs/codebase/MODULE_INDEX.md`](codebase/MODULE_INDEX.md) and [`docs/codebase/catalog/`](codebase/catalog/) for existing code references.
3. **Analyze and Trace**:
   - Follow [`.agents/rules/mandatory-map-check.md`](../.agents/rules/mandatory-map-check.md).
4. **Implement & Test**:
   - Write tests first or alongside implementation.
   - Run tests using repository commands:
     ```powershell
     dotnet test --solution StudyAI.slnx
     ```
5. **Update State & Handoff**:
   - Run documentation sync and validation:
     ```powershell
     python scripts/docs/validate_docs.py
     ```
   - Update `docs/plans/<TASK-ID>/task.md` and `HANDOFF.md`.
   - Update `docs/development/CURRENT_STATE.md`.

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
| **Sync Doc Index** | `python scripts/docs/sync_codebase_index.py` | Refreshes codebase file index & catalog |
| **Validate Doc Health** | `python scripts/docs/validate_docs.py` | Verifies doc links, catalog integrity, skills |
| **Validate Local DB** | `powershell -File scripts/Validate-Database.ps1` | Docker PostgreSQL runtime validation |
