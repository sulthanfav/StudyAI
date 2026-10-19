# StudyAI Campus V1 Implementation Roadmap

Status: planning baseline, 2026-10-19. This roadmap sequences delivery; it does not reduce the public V1 scope or approve any OPEN decision.

## Program rules

- Public V1 retains all nine PRD modules. Internal slices may be incomplete and unreleased.
- Security, Data Space ownership, source provenance, usage metering, and cost caps are foundations, not launch hardening added later.
- A phase may start discovery in parallel, but implementation that depends on an OPEN ADR cannot pass its gate until the ADR is accepted.
- The reviewed v1.1 SQL remains a clean-slate candidate until Phase 1 evidence exists. Production changes must become incremental, reviewed migrations.
- Partner-gated capabilities are not marketed or enabled until legal, license, quality, privacy, and cost gates pass.

## Phase 0 — Repository and Development Environment

### Objective

Create a reproducible, safe repository and development baseline without application scaffolding or business logic.

### Prerequisites

- Final PRD, approved Core Architecture v1.2, and database review artifacts are available.
- Product Owner accepts the documented source-of-truth hierarchy.

### Deliverables

- Organized monorepo, root engineering rules, documentation index, open-decision register, and this roadmap.
- Git repository, ignore rules, editor settings, and safe environment-variable template.
- Recorded toolchain inventory and a development-environment gap list.

### Implementation tasks

1. Preserve and hash all original documents, then move them into product, architecture, and database folders.
2. Establish repository conventions without installing packages or creating empty framework layers.
3. Confirm Git, .NET, Node/npm, Python, Docker/Compose, and PostgreSQL tooling availability.
4. Decide supported developer versions before generating lockfiles or global.json.
5. Define a secrets workflow and keep all real values outside Git.
6. Review proposed agent skills/plugins; approve each separately before installation.

### Dependencies

- None beyond local filesystem and Git.

### Acceptance criteria

- Every original document exists once with the same SHA-256 content hash.
- Git is initialized locally, with no remote and no commit.
- Root configuration contains no real credential.
- Missing runtimes are explicit; no success claim relies on an unavailable tool.

### Required tests

- File inventory and hash comparison.
- Git status and remote check.
- Secret-pattern review of tracked candidates.
- Version commands for each required development tool.

### Definition of Done

- Repository bootstrap is reproducible and documented; no application code, dependencies, database mutation, remote, or deployment was created.

## Phase 1 — PostgreSQL Runtime Validation

### Objective

Prove or correct the reviewed physical schema in an isolated PostgreSQL 16+ runtime before creating production-oriented EF Core migrations.

### Prerequisites

- Phase 0 complete.
- Docker/Compose or an equivalent isolated PostgreSQL 16+ environment is approved and available.
- Required extensions are available: vector, pg_trgm, and btree_gist.
- Initial decisions are recorded for target PostgreSQL/pgvector versions, runtime DB roles, and the scope of the RLS experiment.

### Deliverables

- Reproducible local PostgreSQL runtime definition.
- Sanitized schema and smoke-test execution logs.
- Constraint/index inventory and documented fixes, if any.
- Initial database test harness and migration strategy note.
- ADR outcomes or explicit deferrals for physical-tree, active processing run, note checkpoint, RLS, and retention decisions.

### Implementation tasks

1. Start an isolated empty database; never point validation commands at production.
2. Apply the reviewed v1.1 clean-slate schema with stop-on-error.
3. Verify extensions, 68-table expectation, foreign keys, unique/exclusion constraints, triggers, indexes, and schema namespaces.
4. Run the v1.1 smoke-test script and retain sanitized evidence.
5. Add missing P0/P1 negative cases for cross-owner links, immutable snapshots, active processing runs, subscription overlap, stale notes, and multi-period reservations.
6. Exercise candidate RLS with a non-owner, non-superuser, non-BYPASSRLS runtime role and pooled A-to-B context switching.
7. Benchmark only enough representative queries to validate query shape; do not select ANN or publish capacity claims yet.
8. Convert confirmed defects into reviewed SQL revisions and ADRs; do not patch silently.
9. Document incremental-migration requirements for later EF work; do not wrap the entire clean-slate file into one giant production migration.

### Dependencies

- PostgreSQL runtime, pgvector image/package, psql, and approved local secrets.
- OPEN-DB, OPEN-RLS, OPEN-RETRIEVAL, OPEN-STORAGE policy inputs where they affect schema behavior.

### Acceptance criteria

- Schema and smoke tests complete with zero unexplained errors in a fresh isolated database.
- All P0 review findings are enforced and demonstrated.
- RLS status is accurately recorded as accepted, rejected, or still experimental; application authorization remains mandatory.
- No SQL is labeled production-ready.

### Required tests

- Fresh-install and repeat-install behavior.
- Constraint/trigger negative tests and orphan scans.
- Tenant isolation through EF-equivalent SQL, raw retrieval SQL, and pooled RLS sessions.
- Quota and subscription concurrency tests.
- Backup and restore of the isolated database, followed by integrity checks.

### Definition of Done

- Runtime evidence, known limitations, and migration implications are reviewed. The schema may advance to a tested candidate only if the evidence supports that label.

## Phase 2 — Backend Foundation and Authentication

### Objective

Create the modular ASP.NET Core foundation, identity boundary, ownership enforcement, and operational guardrails required by every module.

### Prerequisites

- Phase 1 tested database candidate.
- Accepted ADR-AUTH before identity implementation.
- Accepted initial queue/outbox, runtime-role/RLS, secret-management, API versioning, and deployment-development ADRs.
- Compatible .NET, EF Core, Npgsql, and pgvector package versions selected and pinned.

### Deliverables

- Solution and project boundaries for Domain, Application, Infrastructure, Contracts, API, and workers without premature per-module services.
- One StudyAiDbContext and migration assembly with reviewed incremental migrations.
- Registration, verification, login/logout, reset/revocation, and server-side actor-to-Data-Space resolution.
- Health checks, structured redacted logging, correlation IDs, OpenAPI, error contract, rate-limit foundation, and outbox dispatch.
- Test projects for unit, PostgreSQL integration, security, migration, and API contract testing.

### Implementation tasks

1. Scaffold only the approved projects and enforce module references.
2. Implement Identity profile and credential/session integration according to ADR-AUTH; do not invent password or refresh-token cryptography.
3. Provision user, Personal Data Space, and canonical Library atomically.
4. Add authorization policies for Data Space operations and private object access.
5. Map the validated core database slice and create incremental migrations with rollback/data plans.
6. Implement request idempotency primitives, outbox publishing, job identity, and bounded retries.
7. Add entitlement and quota interfaces early; use fake providers in tests but do not allow unmetered chargeable paths.
8. Establish CI quality gates without claiming production deployment.

### Dependencies

- Auth/OIDC provider or ASP.NET Core Identity decision.
- Queue/outbox approach, database runtime roles, observability baseline, and secret storage.

### Acceptance criteria

- A user can complete the core account lifecycle and receives exactly one authorized Personal Data Space and Library.
- User A cannot access or link User B resources through API, direct IDs, raw SQL paths, object URLs, or job payloads.
- Migrations reproduce the validated schema slice on clean and populated test databases.
- Logs and errors contain no secrets or private payloads.

### Required tests

- Auth lifecycle, session revocation, CSRF/cookie or bearer-token behavior according to the ADR.
- TenantIsolationTests, authorization-policy tests, signed-object authorization tests.
- Migration up/forward-compatibility/restore tests using real PostgreSQL.
- Outbox replay, API idempotency, transient failure, and redaction tests.
- Dependency and secret scans.

### Definition of Done

- Foundation is buildable, documented, and protected by automated gates; authentication and authorization evidence exists; no feature module bypasses shared ownership policies.

## Phase 3 — Workspace and Knowledge Management

### Objective

Deliver M01 and the shared content foundation: Library, Course, Project, folders, notes, placements, links/tags, Trash/restore, and concurrency-safe editing.

### Prerequisites

- Phase 2 complete.
- Physical content-tree/subtree, canonical placement, note editor format, note checkpoint/retention, attachment, and conflict UX ADRs accepted.
- Storage adapter contract available before attachments/uploads are public.

### Deliverables

- Content Spaces and workspace metadata for Library, Course, and Project.
- Knowledge Items, canonical/multi-space placements, folders, nested-page navigation, tags/links, and recent/favorite projections as approved.
- Structured notes with revisions, autosave, explicit conflict responses, Trash/restore, and authorized attachments.
- Responsive Web UI for core workspace flows; future mobile contract remains versioned.

### Implementation tasks

1. Implement atomic content creation: item, subtype, canonical Library placement, and outbox.
2. Implement placement reuse without copying content or storage.
3. Implement locked reparent/move checks, recursive cycle prevention, reorder, subtree removal, and restore-to-root fallback.
4. Implement optimistic note concurrency and immutable checkpoints used by AI.
5. Enforce storage authorization and upload lifecycle for note attachments.
6. Instrument activation, save conflicts, restore failures, and storage usage without logging content.

### Dependencies

- Phase 2 ownership/auth, editor decision, storage lifecycle decision, and object-store development adapter.

### Acceptance criteria

- M01 acceptance flows work end to end.
- One item can appear in Library/Course/Project with one content identity and one binary/version set.
- Remove-from-space, Trash, restore, and permanent-delete request have distinct verified behavior.
- Concurrent stale note saves return an explicit conflict; no content is silently overwritten.

### Required tests

- Hierarchy cycle and concurrent reparent tests.
- Canonical placement, multi-placement storage accounting, remove/restore/purge tests.
- Note autosave, revision, stale-client, attachment, and cross-user isolation tests.
- Responsive/accessibility and Playwright E2E tests for core workspace journeys.

### Definition of Done

- M01 is usable without AI, data ownership and lifecycle invariants pass, and public behavior/API documentation is current.

## Phase 4 — Documents and Knowledge Retrieval

### Objective

Deliver M02 and the secure source foundation: upload, immutable versions, extraction, indexing, annotations, scoped search, retrieval, and source-version provenance.

### Prerequisites

- Phase 3 content/storage foundation.
- Accepted queue, storage/retention, source-version, document pipeline, embedding model/dimension, FTS/hybrid evaluation, and RLS decisions needed for the slice.
- Indonesian/English evaluation corpus and malicious-document test corpus approved for use.

### Deliverables

- Verified private upload state machine, storage objects, documents, immutable document versions, and processing runs.
- Sandboxed Python document-processing worker with versioned extraction output.
- Active-run publication, chunks, embeddings, keyword/exact-vector/hybrid retrieval, annotations, and source selection.
- Central retrieval service that authorizes sources before query/model access.
- Evaluation reports for extraction and retrieval; no unsupported ANN index.

### Implementation tasks

1. Implement upload intent, object verification, DB commit, orphan cleanup, re-upload, and reprocessing recovery.
2. Dispatch scoped jobs through the approved durable queue/outbox pattern.
3. Parse PDF/DOCX/PPTX with limits, file-type validation, timeouts, and untrusted-content isolation.
4. Publish exactly one ready active processing run without dropping the old valid run early.
5. Implement revised Q12-R/Q13-R/Q14-R semantics and source filters.
6. Store immutable source versions and locators; preserve historical citations across new file versions.
7. Measure Indonesian/English recall, ranking, latency, and cost before ANN/reranker decisions.

### Dependencies

- Object storage, queue/workers, embedding/AI gateway adapter, PostgreSQL extensions, and RLS/security controls.

### Acceptance criteria

- M02 upload-to-reader/search workflow works for the required formats and truthful failure states.
- Retrieval never returns data outside authorized Data Spaces or selected source scope.
- Reprocessing never mixes old/new chunk sets, and old AI citations still identify the original source version.
- Extraction limitations are visible and no unverifiable locator is presented as fact.

### Required tests

- Malformed/oversized/malicious document tests; parser timeout and retry tests.
- Multiple-processing-run and active-publication tests.
- Retrieval source-scope, cross-user, RLS/pooling, prompt-injection, and provenance tests.
- Storage orphan, quota, retention, downgrade, purge, and reconciliation tests.
- Retrieval quality benchmarks and query-plan capture.

### Definition of Done

- M02 and the shared retrieval/provenance layer meet evidence-backed quality, security, recovery, and cost gates.

## Phase 5 — AI Engine and Academic Modules

### Objective

Build the AI orchestration layer and complete M03–M09 workflows using authorized, versioned evidence while retaining truthful partner gates.

### Prerequisites

- Phase 4 secure retrieval/provenance.
- AI provider/routing, job lifecycle, output-validation, quota-event/chargeable-failure, and model-evaluation ADRs accepted.
- Editor/export decisions for writing and presentation; research-provider rights and similarity partner status documented.

### Deliverables

- Provider-agnostic AI gateway; job lifecycle, cancellation, retries, history, evaluation, grounding, and citation validation.
- M03 AI Tutor and personalized learning.
- M04 quizzes, flashcards, attempts, review, and exam preparation.
- M05 research discovery/library/literature matrix with DOI and full-text rights separation.
- M06 writing drafts/revisions/citations and required exports.
- M07 editable presentation generation/export and render validation.
- M08 citation audit plus similarity adapter only if partner gate passes.
- M09 planner, RPS-assisted planning, tasks/events/reminders, and learning integration.

### Implementation tasks

1. Enforce authorize → reserve → dispatch → process → validate → settle/release for every chargeable job.
2. Persist exact selected source versions, evidence, provider/model/version, usage, costs, and recoverable state.
3. Reject or mark unsupported DOI, author, quote, page/slide, and claim-to-source output.
4. Build each module as an end-to-end workflow, not a placeholder page.
5. Keep metadata-only research distinct from rights to analyze full text.
6. Render-test DOCX/PDF/PPTX outputs for validity and editability.
7. Integrate planner reminders through idempotent jobs.
8. Maintain evaluation datasets for Indonesian/English, scans, formulas, tables, citations, quiz correctness, and slide layout.

### Dependencies

- AI/embedding providers, external research sources, export libraries, payment-independent entitlement checks, and optional licensed similarity provider.

### Acceptance criteria

- Each of M03–M09 passes its PRD workflow and error/empty/quota states.
- No academic citation, DOI, quote, or locator is invented or marked verified without evidence.
- Failed/retried jobs produce consistent user charges and separately recorded provider costs.
- Similarity remains disabled/unmarketed unless contractual and QA gates pass.

### Required tests

- AI grounding, citation hallucination, source-version, prompt-injection, model fallback, cancellation, and replay tests.
- Quiz correctness/persistence and learning-history tests.
- Research DOI normalization/dedup/rights tests; writing citation and export tests.
- Presentation render/editability tests; integrity audit tests; planner timezone/reminder idempotency tests.
- Per-feature unit-cost and quality evaluations.

### Definition of Done

- All nine modules now have complete internal workflows, with source truthfulness, metering, authorization, recovery, and module-specific acceptance evidence.

## Phase 6 — Subscription, Billing, and Operations

### Objective

Make entitlements, payment, quotas, administration, observability, privacy operations, and cost protection launch-capable.

### Prerequisites

- Quota-event, period boundaries, overrun/failure treatment, pricing/limits, payment, refund/grace, tax/promo, and storage-retention policies accepted.
- Payment provider and contracts approved; legal/privacy review underway.
- Phase 2 metering primitives and Phase 5 usage evidence available.

### Deliverables

- Data-driven Free/Plus/Pro catalog and entitlements.
- Atomic multi-period quota reservations, settlement/release, append-only ledger, balance projections, and reconciliation.
- Verified idempotent payment webhooks, subscription periods, upgrade/downgrade/refund/grace flows, and history.
- Admin views for jobs, usage, cost, quota, payment mismatches, storage, errors, and support actions with audit.
- Metrics, alerts, backup schedule, privacy export/deletion workflows, and documented runbooks.

### Implementation tasks

1. Implement hard orchestration caps per job/user/feature/global budget before paid AI exposure.
2. Lock balances in deterministic order and reconcile projections to ledger.
3. Verify webhook signatures, order/amount/currency, deduplicate delivery, and activate entitlement atomically.
4. Keep existing personal data readable after quota exhaustion/downgrade; restrict new writes according to policy without silent deletion.
5. Add audited admin adjustments and support access with least privilege.
6. Complete privacy, retention, purge, backup-deletion, incident, and payment-reconciliation runbooks.

### Dependencies

- Commercial and legal decisions, payment sandbox, notification delivery, observability stack, and offsite backup target.

### Acceptance criteria

- Concurrent jobs cannot overdraw or double debit; provider cost is never confused with user credit.
- Duplicate/reordered/mismatched payment events cannot grant duplicate or incorrect entitlement.
- Free non-AI features remain usable after AI quota exhaustion.
- Operations can detect and reconcile stuck jobs, ledger drift, payment mismatch, orphan storage, and failed purge.

### Required tests

- 2–100 concurrent reservation/settlement/release tests over daily and monthly periods.
- Payment replay, signature, mismatch, refund, upgrade/downgrade, overlap, and outage tests.
- Admin authorization/audit tests and log-redaction tests.
- Backup/restore, privacy export/deletion, retention, purge, and reconciliation drills.
- Unit-economics and abuse/fraud limit tests.

### Definition of Done

- Billing and operations have transactional, security, cost, support, and recovery evidence; hypotheses are not mislabeled as final policy.

## Phase 7 — Integration Testing and Launch Readiness

### Objective

Prove the complete nine-module product, security model, cost envelope, recoverability, and operational readiness before public launch.

### Prerequisites

- Phases 1–6 complete for the intended launch build.
- All blocking ADRs accepted and partner/legal gates have explicit status.
- Staging resembles the target production topology and contains only authorized test data.

### Deliverables

- PRD traceability matrix with test evidence for all nine modules and GP-01 through GP-05.
- Security assessment, privacy/legal sign-off, AI/retrieval evaluation, accessibility review, and performance/cost report.
- Migration rehearsal, backup/restore drill, incident/rollback/runbook exercises, and launch checklist.
- Explicit list of gated features that will remain unavailable if external conditions are unmet.

### Implementation tasks

1. Run full E2E across accounts, modules, files, AI, billing, admin, and lifecycle operations.
2. Execute cross-user, IDOR, object-access, raw retrieval, worker-scope, RLS, prompt-injection, and secrets assessments.
3. Load-test common API reads, note autosave, retrieval, uploads, AI streaming/jobs, quota contention, exports, and webhooks.
4. Measure AI quality and unit cost on representative Indonesian/English material.
5. Rehearse clean deploy, forward migration, rollback/compensation, VPS-to-cloud restore, and disaster recovery.
6. Validate observability, alerts, support procedures, accessibility, responsive behavior, terms/privacy/refund/copyright/PDP/PSE review, and marketing claims.

### Dependencies

- Production candidate infrastructure, external-provider sandboxes/contracts, legal review, and representative test corpus.

### Acceptance criteria

- All PRD launch gates and E2E-01 through E2E-12 pass with retained evidence.
- No unresolved P0/P1 security, data-integrity, billing, migration, backup, or citation-truthfulness issue remains.
- Measured SLO/capacity/cost values replace unsupported hypotheses.
- Partner-gated features are either approved and tested or clearly disabled and not marketed.

### Required tests

- Full unit, integration, security, E2E, accessibility, migration, load, resilience, backup/restore, and AI evaluation suites.
- Production-role and configuration smoke tests with no real customer data.
- Independent review of critical ownership, quota, payment, provenance, and deletion paths.

### Definition of Done

- Product Owner, engineering, QA/security, operations, and required legal/commercial owners accept evidence for the complete Campus V1 launch; release and rollback procedures are executable.

## Immediate next action

Review the Phase 1 runtime evidence in `docs/database/DATABASE_RUNTIME_VALIDATION_REPORT.md`, then close the remaining Phase 1 gates: runtime DB roles/RLS and pooled-context experiments, true concurrent-session tests, populated incremental-migration rehearsal, and the relevant ADR outcomes. Prepare but do not silently approve ADRs for authentication, queue/outbox, runtime DB roles/RLS, storage lifecycle, and PostgreSQL/pgvector version pinning. Do not scaffold feature modules or production EF Core migrations until those gates are accepted.
