# Open Architecture and Product Decisions

Status: consolidated decision queue, 2026-10-19.

This file does not approve a choice. It consolidates the OPEN items in the final PRD, Core Architecture v1.2, Physical ERD handbook, and Database Implementation Review v1.1. Each accepted decision should become a separate numbered ADR containing context, options, decision, consequences, security/cost impact, migration/rollback plan, and acceptance evidence.

## Decision policy

- Product behavior and LOCKED/MUST invariants are constraints, not options.
- Prefer reversible adapters and the smallest operational surface that meets measured needs.
- Vendor, price, quota, retention, capacity, and SLO values require evidence and owner approval.
- An implementation milestone cannot pass its gate while a decision it materially depends on remains open.

## Consolidated register

| ID | Decision | Must be resolved | Source mapping |
|---|---|---|---|
| ADR-001 | Authentication and session architecture | Before Phase 2 account/identity migrations | PRD OD-01; Architecture OPEN-05; Review ADR-AUTH/DBR-16; Handbook DB-ADR-001 |
| ADR-002 | Background jobs, scheduler, queue, leasing, retries, and outbox boundary | Before Phase 2 durable dispatch and before Phase 4 processing | PRD OD-02; Architecture OPEN-06; Review ADR-QUEUE; Handbook DB-ADR-008 |
| ADR-003 | PostgreSQL RLS, runtime roles, pooled connection context, and worker isolation | Trial in Phase 1; accepted production design before external user data | PRD OD-04; Architecture OPEN-09; Review ADR-RLS/DBR-10; Handbook DB-ADR-007 |
| ADR-004 | Document/object storage provider, upload protocol, quota bytes, version retention, Trash/purge, and backup retention | Before Phase 3 attachments and Phase 4 public upload | PRD OD-07; Architecture OPEN-10/12; Review ADR-STORAGE; Handbook DB-ADR-011 |
| ADR-005 | AI provider gateway, model routing, failure policy, data handling, and budget caps | Before Phase 5 AI implementation; evaluation starts in Phase 4 | PRD OD-08; Architecture OPEN-12/15; PRD PLT-06/09 |
| ADR-006 | Embedding model/dimensions, Indonesian/English evaluation, FTS/hybrid scoring, reranking, and ANN policy | Before Phase 4 retrieval quality sign-off | PRD OD-05; Architecture OPEN-07/08; Review ADR-EMBED/DBR-14; Handbook DB-ADR-006/014 |
| ADR-007 | Payment provider and subscription transitions, refunds, grace, tax/promo, Web/mobile sales | Provider before Phase 6 integration; mobile/IAP before native launch | PRD OD-09/14; Architecture OPEN-11/12/15; Review ADR-PAY; Handbook DB-ADR-009/010 |
| ADR-008 | Notes editor schema, attachments, autosave/checkpoints, conflict UX, import/export, and offline boundary | Before Phase 3 notes implementation | PRD OD-13; Architecture OPEN-04/17; Handbook DB-ADR-005 |
| ADR-009 | Quota/credit policy, period boundaries, event schema, overrun and chargeable-failure treatment | Interface in Phase 2; accepted before any paid AI test and Phase 6 | PRD OD-09; Architecture OPEN-11/18; Review ADR-QUOTA; Handbook DB-ADR-009 |
| ADR-010 | Production topology, hosting, secrets, observability, SLO/RPO/RTO, backup, and scaling triggers | Development subset before Phase 2; production decision before Phase 7 | PRD OD-12; Architecture OPEN-12/16; Handbook DB-ADR-014 |
| ADR-011 | Physical content tree, subtree lifecycle, canonical placement enforcement, and reparent concurrency | Before Phase 3 schema/migrations | Architecture OPEN-01/02; Handbook DB-ADR-002/003 |
| ADR-012 | Unified source-version representation, checkpoint retention, citation tombstones, and purge behavior | Before Phase 4 provenance and Phase 5 Tutor/Research | PRD OD-06; Architecture OPEN-03; Review ADR-SOURCE; Handbook DB-ADR-005/013 |
| ADR-013 | Document processing formats, OCR/media quality thresholds, sandboxing, and provider permissions | Before enabling each Phase 4/5 format | PRD OD-11; Architecture OPEN-14 |
| ADR-014 | Similarity provider, license, report claims, data processing, and billing | Before enabling or marketing similarity in Phase 5/7 | PRD OD-10; Architecture OPEN-13; Review ADR-SIMILARITY; Handbook DB-ADR-012 |

## ADR-001 — Authentication and session architecture

### Questions to decide

- ASP.NET Core Identity owned by StudyAI, external OIDC provider, or a hybrid.
- Email verification/reset, Google/Apple login, MFA, account linking, session/device revocation, and future mobile token pattern.
- Credential/session tables and migration ownership versus the existing identity.users profile table.

### Constraints

- Authentication never replaces resource authorization. Server code must resolve the actor to permitted Data Spaces.
- Do not implement custom password hashing or ad-hoc refresh tokens.
- Account provisioning must atomically create the profile, Personal Data Space, and Library.

### Recommendation for ADR review

Evaluate ASP.NET Core Identity plus standards-based OIDC/OAuth adapters first because it fits the selected ASP.NET stack and keeps provider portability. Compare it against a managed provider on security operations, mobile support, export/migration, cost, outage dependency, and Indonesian delivery needs. This is not approved.

### Required evidence

- Threat model, token/cookie/CSRF design, session revocation test, account-linking failure cases, provider exit plan, and tenant-isolation tests.

## ADR-002 — Background jobs and queue

### Questions to decide

- PostgreSQL-backed durable queue/Hangfire, dedicated broker, or staged combination.
- Lease/heartbeat, scheduling, retry/dead-letter, cancellation, priority, deduplication, concurrency, and worker authorization.
- Exact boundary between transactional outbox delivery and business job execution.

### Recommendation for ADR review

Start by comparing a durable .NET/PostgreSQL-aligned option with Hangfire while retaining the outbox as the database-commit bridge. Adopt a separate broker only when measured throughput/reliability/operations justify it. Never let a queue choice move authorization, quota, or provider-cost authority out of the .NET application boundary. Not approved.

### Required evidence

- Crash/replay tests, duplicate delivery, lost lease recovery, poison job behavior, scoped worker credentials, operational dashboard, and migration path.

## ADR-003 — PostgreSQL RLS and roles

### Questions to decide

- Which tenant-sensitive tables receive RLS first, whether FORCE ROW LEVEL SECURITY is safe, and which roles own/migrate/run/query.
- Transaction-local actor/Data Space context with connection pooling and cancellation/error cleanup.
- How narrowly scoped workers obtain access without a global data bypass.

### Recommendation for ADR review

Trial RLS first on document_chunks and chunk_embeddings with a non-owner runtime role, transaction-local set_config, and adversarial pooled A-to-B tests. Keep explicit application authorization and owner-safe constraints mandatory. Expand only after operational and test evidence. Not approved.

### Required evidence

- Raw SQL leakage tests, pooled-connection reuse, worker paths, table-owner/BYPASSRLS checks, migration-role separation, query-plan impact, and incident diagnostics.

## ADR-004 — Object storage and retention

### Questions to decide

- Local development service and production S3-compatible vendor.
- Presigned upload/download protocol, object verification, encryption, malware checks, and regional/data-processing constraints.
- Billable bytes, old versions, exports, derived files, Trash duration, purge, legal hold, downgrade behavior, and backup deletion.

### Recommendation for ADR review

Keep domain code behind the approved S3-compatible interface; use a local compatible service only for development. Choose production storage by privacy, durability, egress, region, lifecycle, audit, and restore evidence—not API compatibility alone. Keep private-by-default objects and database-authorized short-lived access. Not approved.

### Required evidence

- Orphan/reconciliation tests, version/purge matrix, backup/restore, expired URL tests, quota accounting independent of placements, provider exit rehearsal, and approved retention policy.

## ADR-005 — AI providers and model routing

### Questions to decide

- Primary/fallback providers and models per workflow, routing data, data retention/training terms, regional constraints, timeout/retry/circuit behavior, and streaming.
- Per-job/user/feature/global budgets and behavior when price/usage is unknown.
- Evaluation, rollout, model-version pinning, and incident rollback.

### Recommendation for ADR review

Define a narrow provider gateway contract around capabilities, validated structured output, usage evidence, and model/version identity. Select providers per measured workflow quality, cost, latency, privacy, and reliability; do not create premature provider feature parity. Fail closed for chargeable work when cost cannot be bounded. Not approved.

### Required evidence

- Representative Indonesian/English evals, citation/grounding tests, cost model, provider data terms, retry/idempotency tests, fallback quality, and budget-cap demonstrations.

## ADR-006 — Embeddings and retrieval

### Questions to decide

- Embedding provider/model/dimension, re-index lifecycle, multilingual coverage, chunking, tokenization/trigram, hybrid fusion, reranker, and top-K.
- Exact search versus HNSW/other ANN, tenancy filtering, and index/memory implications.

### Recommendation for ADR review

Begin with the documented PostgreSQL FTS/trigram plus exact pgvector baseline and revised active-run/source-scoped queries. Choose model and scoring from a versioned Indonesian/English evaluation corpus. Add ANN only when measured scoped latency requires it without unacceptable recall or tenant-filter behavior. Not approved.

### Required evidence

- Recall/precision/nDCG or task-success metric, source leakage tests, query plans, latency and cost distributions, dimension/migration plan, and re-index rollback.

## ADR-007 — Payments and subscriptions

### Questions to decide

- Indonesian Web payment provider/methods, verification API/webhooks, refund/cancel, pending/failed, upgrade/downgrade/proration/grace, tax/promo, and reconciliation.
- Final prices/limits and future App Store/Play entitlement behavior.

### Recommendation for ADR review

Use a provider adapter with server-side verification, idempotent inbox, and subscription-period history. Evaluate local payment coverage, settlement/reconciliation, webhook security, refunds, support burden, fees, and sandbox quality. Keep Web decisions separate from later mobile IAP compliance while sharing entitlement state. Not approved.

### Required evidence

- Duplicate/reordered webhook tests, amount/order/currency mismatch, outage/replay, refund/proration policy approval, financial reconciliation, and legal/commercial sign-off.

## ADR-008 — Notes editor and concurrency

### Questions to decide

- Tiptap/structured JSON schema and versioning, supported blocks, attachment references, import/export, and backward compatibility.
- Autosave interval/batching, immutable checkpoints, storage/retention, merge/conflict UI, and future offline scope.

### Recommendation for ADR review

Evaluate the Architecture v1.2 recommended Tiptap structured JSON baseline with an explicit schemaVersion and server-authoritative revision. V1 should return explicit stale-write conflicts and preserve recovery; automatic merge/offline sync should be limited until tested. Not approved.

### Required evidence

- Cross-version document fixtures, concurrent web/mobile simulations, export/import round trips, checkpoint provenance, attachment authorization, accessibility, and data-loss recovery.

## ADR-009 — Quotas and credit policy

### Questions to decide

- Resource units visible to users versus provider tokens/cost, daily/monthly periods and timezone, rollover/top-ups, model overrides, partial output, cancellation, failure charging, and overrun.
- Append-only event schema, balance projection, reservations, TTL, admin adjustments, and reconciliation.

### Recommendation for ADR review

Preserve atomic multi-period reserve then settle/release, deterministic balance locks, separate provider-cost ledger, and hard caps. Keep limits and conversion rates data-driven. Define user-facing units from understandable product actions only after unit-economics tests. Not approved.

### Required evidence

- 2–100 concurrent job tests, replay/crash recovery, partial settlement, additional hold/stop at cap, reconciliation, DST/timezone boundaries, and approved commercial policy.

## ADR-010 — Production deployment and operability

### Questions to decide

- Initial VPS versus managed services, container topology, secrets, TLS/ingress, monitoring/logs/traces, deploy/rollback, and environment separation.
- SLOs, capacity, alert thresholds, RPO/RTO, PostgreSQL/object backups, restore frequency, and scale triggers.

### Recommendation for ADR review

Keep the approved portable container boundary and design a simple soft-launch topology, but place private objects and backups outside a single VPS failure domain. Choose managed components when reduced operational risk justifies cost. No capacity number or SLO should be accepted before load and restore evidence. Not approved.

### Required evidence

- Threat model, cost envelope, load tests, resource saturation behavior, secrets rotation, zero/low-downtime migration assessment, offsite restore drill, and VPS-to-cloud rehearsal.

## ADR-011 — Physical content tree and lifecycle

### Questions to decide

- Final folder/placement parent FKs, at-least-one canonical Library enforcement, subtype completeness, lock/advisory-lock strategy, subtree remove/restore, and duplicate-active-placement conflicts.

### Recommendation for ADR review

Retain the proposed placement-based nested-page model and serialize every reparent/restore path per Content Space with a recursive cycle check. Compare service-transaction enforcement versus deferrable database triggers for canonical placement/subtype existence. Not approved.

### Required evidence

- Concurrent cycle attempts, subtree lifecycle matrix, orphan scanner, restore-to-root behavior, owner/type FK tests, and UI semantics approval.

## ADR-012 — Unified provenance and retention

### Questions to decide

- source_versions supertype versus typed references, note checkpoint frequency, immutable snapshot payload, locator/evidence format, tombstones, and purge/anonymization.

### Recommendation for ADR review

Keep a unified source-version identity with typed immutable snapshots as the reviewed schema proposes, but decide retention from auditability, privacy deletion, storage cost, and legal obligations. Historical citations must never silently point at new content. Not approved.

### Required evidence

- PDF v1-to-v2, note revision, reference metadata, saved output, tombstone/purge, privacy deletion, and historical citation tests.

## ADR-013 — Document pipeline and media

### Questions to decide

- Required format libraries, OCR threshold/languages, scanned PDFs, formulas/tables, PPTX/DOCX fidelity, audio/video priority, and processing-provider rights.
- Sandbox, file validation, resource limits, and disclosure of extraction quality.

### Recommendation for ADR review

Use a Python worker only for extraction libraries, with .NET retaining authorization/orchestration/metering. Enable formats incrementally behind measured quality gates and explicit user-visible failure/limitations. Audio/video remain off until product/legal/cost gates pass. Not approved.

### Required evidence

- Malicious/oversized files, timeout/memory limits, Indonesian scans, tables/formulas, locator accuracy, consent/provider terms, and cost benchmarks.

## ADR-014 — Similarity partner

### Questions to decide

- Licensed provider and permitted claims, upload/data retention, report storage, pricing, refund/failure behavior, and marketing language.

### Recommendation for ADR review

Implement only an adapter boundary and citation audit until a signed agreement grants the required rights. Do not imply Turnitin or equivalent access, redistribute reports, or market similarity readiness based on database tables. Not approved.

### Required evidence

- Executed contract/legal approval, privacy assessment, provider security/data terms, sandbox quality/cost tests, billing failure paths, and approved user disclosures.

## Candidate implementation choices that are not separate approvals

- Database Review v1.1 proposes active processing runs, immutable source/document identities and snapshot payloads, multi-period quota uniqueness, provider invocation identity, and non-overlapping active subscription periods. These are the current physical candidates and must be runtime-tested in Phase 1.
- Tiptap, EF Core/Npgsql, PostgreSQL FTS/pgvector exact search, UUID v7, and Python extraction are recommended defaults in approved architecture, not immutable vendor decisions. An equivalent alternative requires an ADR and must preserve all invariants.
