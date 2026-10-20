# Proposal

## Why

StudyAI has a verified clean-slate PostgreSQL candidate but no executable backend host or automated .NET test baseline. A narrowly scoped ASP.NET Core foundation is needed so later modules can be added behind consistent configuration, dependency-injection, health, logging, and test boundaries without prematurely implementing unresolved authentication, RLS, queue, storage, or migration decisions.

## What Changes

- Add the first ASP.NET Core modular-monolith solution and API host skeleton, preserving the approved single-deployable API boundary and explicit module composition.
- Add environment-based configuration with startup validation for required non-secret settings and no committed credentials.
- Add liveness and readiness health endpoints whose checks do not disclose secrets or private data.
- Add structured logging with correlation identifiers and default redaction rules for credentials, tokens, signed URLs, prompts, notes, and document contents.
- Add unit and API/integration test projects, including an in-process API test harness and an opt-in real-PostgreSQL fixture boundary for later database tests.
- Add build and test commands plus documentation needed to reproduce the foundation locally.
- Keep EF Core models/migrations, authentication/session behavior, RLS policies/runtime roles, durable queue selection, storage provider integration, business endpoints, and feature-module implementation out of scope until their readiness gates are accepted.
- Treat the proposed .NET/EF/Npgsql version baseline as an explicit pre-apply technical decision; this proposal does not approve it.

## Capabilities

### New Capabilities

- `platform/api-host`: Defines the observable startup, configuration, health, logging, composition, and automated test behavior of the StudyAI backend host.

### Modified Capabilities

None.

## Impact

- Expected implementation areas: root .NET solution/build files, `apps/api`, foundation projects under `src`, test projects under `tests`, and affected developer documentation/index links.
- Governing sources: `AGENTS.md`, `docs/INDEX.md`, the Final PRD v1.0, Core Architecture v1.2, `docs/plans/IMPLEMENTATION_ROADMAP.md`, the Database Implementation Review v1.1, and `docs/adr/OPEN_DECISIONS.md`.
- Security and ownership: this change establishes safe host defaults only. It must not imply that authentication, Data Space authorization, RLS, worker isolation, object authorization, or tenant isolation is implemented.
- Data and migrations: no `DbContext`, entity mapping, schema mutation, or EF migration is included. The reviewed SQL remains a parity reference and clean-slate candidate, not a production migration.
- AI, provenance, quotas, lifecycle, jobs, and storage: no chargeable workflow or domain behavior is introduced, so no metering or lifecycle state machine is added. Future implementations remain bound by the approved invariants.
- Open decisions: ADR-001 through ADR-004 and the dependency/schema-ownership recommendations remain unresolved. Only the dependency baseline directly gates applying this scaffold; the other ADRs gate their respective later implementations.
- Testing: the change must prove restore/build/test/host startup and health behavior. Real PostgreSQL remains mandatory for future database/security tests; SQLite and EF InMemory must not be presented as database-validation substitutes.
