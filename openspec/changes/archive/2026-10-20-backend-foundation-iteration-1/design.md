# Design

## Context

See `proposal.md` for motivation and `specs/platform/api-host/spec.md` for the behavior contract. The repository currently contains documentation, an empty implementation layout, and an isolated PostgreSQL validation harness; it has no .NET solution, projects, dependency lock, application code, or test runner configuration.

The clean-slate schema, catalog assertions, supplied transactional smoke tests, and empty-fixture dump/restore are runtime-verified. Production RLS roles, pooled context switching, true multi-session service races, EF mappings/migrations, populated upgrades, and production security remain unverified. Those gaps constrain later data work but do not require the host-only foundation to connect to PostgreSQL.

## Goals / Non-Goals

**Goals:**

- Establish one compilable API deployable with explicit Domain, Application, Infrastructure, Contracts, and API dependency direction.
- Establish safe configuration, health, correlation, structured logging, and automated host-contract verification.
- Minimize external packages and centralize every package version and restore policy.
- Leave clear extension points for later accepted module changes without adding speculative provider abstractions.

**Non-Goals:**

- No EF Core package, `DbContext`, entity, migration, SQL execution, or database startup dependency.
- No user registration/login, session/token, Data Space authorization policy, RLS role/policy, or ownership endpoint.
- No outbox dispatcher, job queue, worker, object-storage adapter implementation, AI/provider adapter, metering, billing, or feature module.
- No production deployment topology, CI vendor pipeline, or claims of security/performance/production readiness.

## Decisions

### 1. Use a narrow layered modular-monolith solution

Create one solution with `StudyAI.Domain`, `StudyAI.Application`, `StudyAI.Infrastructure`, `StudyAI.Contracts`, and `StudyAI.Api`, plus unit, architecture, and API integration test projects. Dependency direction is Domain <- Application <- Infrastructure, while Contracts remains independent and API is the composition root.

This matches Core Architecture v1.2 and the Phase 2 roadmap while avoiding per-feature services or one project per future module. Feature boundaries can begin as namespaces/folders and public registration contracts inside the approved layers.

Alternatives considered:

- One API project only: smaller initially, but it makes the approved domain/application/infrastructure boundary harder to enforce as modules arrive.
- One project per product module: stronger physical isolation, but premature for an empty codebase and contrary to the initial one-DbContext modular-monolith guidance.

### 2. Make dependency acceptance a pre-apply gate

Target `net10.0` because .NET 10 is the current active LTS baseline. Before implementation, accept and record the exact SDK feature band, central package versions, test platform, and lock-file policy. The working recommendation is a `global.json` in the .NET 10.0.3xx feature band with latest-patch roll-forward, central package management, and committed NuGet lock files with locked restore in verification. Foundation test-package candidates are xUnit v3 `4.0.2`, Microsoft Testing Platform `2.5.1`, and `Microsoft.AspNetCore.Mvc.Testing` `10.0.13`; compatibility must be proven by restore/build/test before the record is accepted.

EF Core and database providers are not added in this iteration. For the later database change, the compatibility candidates to prove together are Microsoft EF Core `10.0.13`, `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.3`, and `Pgvector.EntityFrameworkCore` `0.3.0`; these numbers are recommendations, not an accepted ADR or permission to create migrations.

Alternatives considered:

- .NET 8 LTS: mature but near its November 2026 support end, creating immediate upgrade work.
- .NET 11 release candidate: not a stable production baseline on the review date and would add provider/test ecosystem risk.
- Floating package ranges: reduce manual updates but make restore output non-reproducible and are rejected.

### 3. Use the ASP.NET Core shared framework for host capabilities

Use built-in options binding/validation, health checks, JSON console logging, dependency injection, and minimal hosting. Do not add Serilog, a mediator, validation framework, resilience package, or service-discovery stack until a concrete requirement justifies it.

Alternatives considered:

- Serilog from day one: capable, but the built-in logger satisfies the Iteration 1 structured-log contract with less package and configuration surface.
- Custom service locator or reflection-based module discovery: obscures dependencies and startup failures; explicit registrations are easier to audit and test.

### 4. Keep configuration typed, layered, and secret-safe

Bind only foundation options in Iteration 1 and validate them at startup. Use standard configuration precedence, keep real secrets outside Git, and ensure diagnostics print option/key names rather than values. Provider-specific option types are introduced only with the change that selects that provider.

Alternatives considered:

- Add placeholder auth/database/storage options now: creates misleading readiness and validation burden for unaccepted decisions.
- Custom configuration loader: duplicates framework behavior without a requirement.

### 5. Separate liveness from readiness

Map `/health/live` to process-only checks and `/health/ready` to tagged readiness checks. Responses are bounded JSON with service name, overall status, and safe component names; exception details, connection strings, hostnames, and secret values are excluded. External dependency checks are added only by the change that introduces the dependency.

Alternatives considered:

- One health endpoint: cannot distinguish process failure from dependency readiness and becomes unsafe for orchestration later.
- Database check in the initial endpoint: would couple a host-only foundation to an unapproved persistence implementation.

### 6. Correlate requests without logging content

Add a small middleware that accepts a bounded, syntactically constrained `X-Correlation-ID` or creates one, stores it in a logging scope, and echoes it in the response. Log method, normalized route template where available, status, duration, and correlation ID. Do not enable request/response body logging or raw authorization/header logging.

Alternatives considered:

- Trust arbitrary caller IDs: permits oversized or control-character log injection.
- Log request bodies in Development: risks normalizing unsafe telemetry and conflicts with private academic content constraints.

### 7. Use in-process API tests and reserve real PostgreSQL tests for data changes

Select native Microsoft Testing Platform mode in the .NET 10 `global.json` and use xUnit v3 test executables. Add unit/architecture tests and an in-process API host harness for startup, health, correlation, and redaction behavior. The root verification command is `dotnet test --solution StudyAI.slnx`; native MTP mode uses `--solution` and does not use a VSTest bridge separator. Do not add a fake relational provider.

A real PostgreSQL integration fixture may be added as an opt-in boundary only if it executes against the isolated Compose service and does not create EF mappings or migrations. Database behavioral tests remain work for the later persistence change.

Alternatives considered:

- SQLite or EF InMemory: cannot validate PostgreSQL schemas, JSONB, partial indexes, GiST constraints, triggers, pgvector, or RLS and is rejected for database claims.
- Require Docker for every host-only test: slows the initial feedback loop without exercising any database behavior.

## Risks / Trade-offs

- [Risk] The layered skeleton can become empty ceremony. -> Keep projects minimal, enforce only the approved dependency direction, and add no generic repositories or speculative base classes.
- [Risk] A dependency pin accepted today becomes stale. -> Pin exact versions, automate dependency review separately, and require restore/build/test evidence for each update.
- [Risk] Health endpoints can leak operational detail. -> Return only bounded status and safe component names; test responses for known secret markers.
- [Risk] Correlation middleware can permit log injection or unbounded cardinality. -> Constrain length/character set and generate a replacement for invalid input.
- [Risk] A green host test may be mistaken for tenant/security readiness. -> Keep the scope warning in README/docs and do not register auth, database, queue, or storage readiness checks.
- [Trade-off] Built-in logging has fewer enrichment/sink features than Serilog. -> Accept the smaller surface until an observability ADR demonstrates a missing requirement.
- [Trade-off] Deferring EF Core means no database-backed readiness signal. -> This is intentional; database readiness arrives with the gated persistence change and real PostgreSQL tests.

## Migration Plan

1. Obtain explicit acceptance of the dependency baseline and test platform; update the candidate pins if official compatibility evidence changes.
2. Scaffold the solution/projects and enforce project-reference direction before adding host behavior.
3. Add configuration validation, health endpoints, correlation, and structured logging with tests added alongside each behavior.
4. Run locked restore, build, unit/architecture tests, and API integration tests from a clean workspace.
5. Update README, `docs/INDEX.md`, and the roadmap/ADR references with exact commands and evidence.

No database or external state is migrated. Rollback is removal/reversion of the new source and project files before any dependent change is merged. If later changes already depend on the foundation, rollback must instead be a forward corrective change that preserves their public contracts.
