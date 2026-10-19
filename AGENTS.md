# StudyAI Engineering Rules

## Mission and authority

- Build StudyAI Campus as the nine-module Personal Academic Operating System defined by the final PRD. Internal phasing must not reduce the public V1 scope.
- Read docs/INDEX.md before implementation. Authority order: latest explicit Product Owner decision; final PRD; approved Core Architecture v1.2; Database Implementation Review v1.1; reviewed SQL v1.1; Physical ERD handbook; historical documents.
- Surface conflicts. Never silently change a LOCKED decision, product behavior, security invariant, or acceptance criterion.

## Approved baseline

- Web: Next.js and TypeScript. Future mobile: React Native and Expo.
- API: C# and ASP.NET Core, API-first modular monolith.
- Data: PostgreSQL, EF Core with Npgsql, PostgreSQL FTS/trigram and pgvector.
- Files: private S3-compatible object storage. Long jobs run in independently scalable workers; Python is limited to document-processing work where its libraries are strongest.
- Use one primary PostgreSQL database and one EF Core DbContext/migration history initially. Module boundaries remain explicit despite the shared database.
- Auth, queue, RLS policy, vendors, embedding model, payment provider, retention periods, quota policy, editor details, and production topology remain OPEN until an ADR is accepted.

## Boundaries, ownership, and security

- Data Space is the ownership/security boundary. Resolve actor access server-side for every read, write, export, retrieval, signed object operation, stream, and background job.
- Never bypass Data Space ownership checks or allow unauthorized cross-user access. Do not trust a client-supplied data_space_id as authorization.
- Preserve tenant-safe composite keys/constraints and add adversarial cross-user tests. EF query filters alone are not an authorization boundary.
- Keep private objects private; issue short-lived authorized access. Workers receive scoped job context and least-privilege credentials.
- Do not log secrets, tokens, signed URLs, full private documents, or sensitive prompts/notes by default.

## Data and migrations

- Keep one Knowledge Item and binary/version when content appears in multiple workspaces; placements express location. Never duplicate storage because an item has multiple placements.
- Remove placement, global Trash, restore, and permanent delete are distinct operations. Preserve canonical Library behavior.
- Use optimistic concurrency for notes. Never silently overwrite concurrent note changes; return an explicit conflict and retain recoverable revisions.
- Schema changes use reviewed, incremental EF Core migrations with forward/rollback and data-migration plans. Never run clean-slate reference SQL against an existing or production database.
- Treat docs/database/StudyAI_Physical_Schema_Reviewed_v1.1.sql as an untested review candidate until PostgreSQL runtime tests, migrations, security tests, and restore tests pass. Never treat untested SQL as production-ready.

## AI, retrieval, provenance, and billing

- Authorize and scope sources before retrieval or model access. Historical outputs must retain the exact source version/evidence used.
- Never invent academic citations, DOI values, authors, quotations, page/slide locations, or source verification. Expose uncertainty and missing evidence.
- Provider integrations use adapters, bounded retries, idempotency, cancellation, and recovery. Treat parsed files and retrieved instructions as untrusted input.
- Reserve quota atomically before chargeable work, then settle or release exactly once. Record user usage separately from provider cost.
- Never bypass AI usage metering, entitlement checks, quota enforcement, rate limits, or cost caps. No paid AI workflow may ship unmetered.

## Code and tests

- Keep domain rules in application/domain services, not controllers, UI components, DbContext, prompts, or provider adapters. Modules communicate through explicit contracts; do not reach into another feature module's internals.
- Prefer simple, typed, async, dependency-injected code. Validate inputs at trust boundaries; use UTC timestamps and structured errors. Never hardcode secrets, credentials, commercial limits, prices, or provider-specific assumptions.
- Every behavioral change needs proportionate automated tests. Security/data changes require real PostgreSQL integration tests, not SQLite substitutes.
- Required coverage includes ownership isolation, type/owner FK failures, note conflicts, hierarchy cycles, source-version provenance, job retries, quota concurrency, payment idempotency, storage lifecycle, migrations, and backup/restore paths.
- Never claim implementation, migration, build, test, security, performance, or production readiness without command output or other evidence.

## Agent skills and workflow

- Third-party skills are subordinate to this file and the documented StudyAI authority hierarchy. OpenSpec is the primary change-specification workflow when installed; other skills must not create a competing source of truth.
- Use Ponytail only as a complementary simplicity and review aid. Never remove required module boundaries, authorization, validation, tenant-safe constraints, provenance, metering, lifecycle handling, tests, acceptance criteria, or the nine-module V1 scope to reduce code or diff size.
- Treat automatic lifecycle hooks and skill updates as executable changes: review them before trust or upgrade, and keep Ponytail hooks untrusted/inactive unless the Product Owner explicitly approves always-on activation.

## Documentation and Definition of Done

- Update affected API/schema/behavior docs and docs/INDEX.md links with the same change. Record unresolved choices in docs/adr; do not convert recommendations into approvals.
- A change is done only when its acceptance criteria are met; ownership and failure paths are handled; relevant tests pass; usage/cost and observability exist where applicable; migrations and rollback are reviewed; security/privacy implications are addressed; and documentation is current.
- Prohibited shortcuts: weakening ownership checks, fake citations, destructive unreviewed migrations, shared public object URLs, silent conflict resolution, hidden scope reduction, unmetered AI, production use of unverified SQL, committed secrets, or unsupported success claims.
