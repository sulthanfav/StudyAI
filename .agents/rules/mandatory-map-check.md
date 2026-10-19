# Mandatory Map Check & Safety Governance

Rule ID: `RULE-MAP-CHECK-v2.0`  
Authority: Subordinate to root `AGENTS.md` and approved architecture documents in `docs/INDEX.md`.

---

## 1. Principle: Understand Before Modify

Before modifying existing code or introducing changes to existing behavior, every AI agent (Codex, OpenCode, Antigravity) must determine:

1. **Relevant Entry Points**: Which controllers, endpoints, CLI triggers, or event handlers invoke this code?
2. **Callers and Consumers**: Who depends on this module or interface?
3. **Callees and Dependencies**: What packages, services, domain rules, or database models does this code invoke?
4. **Data Flow**: What data enters, transforms, and exits the execution path?
5. **Public Interfaces**: Are any contracts, DTOs, or exported types changing?
6. **Persistence Implications**: Does this touch database schema, transactions, migrations, or object storage?
7. **Existing Tests**: What unit, integration, or architecture tests currently verify this behavior?

Start by consulting `SYSTEM_MAP.md` and `docs/codebase/MODULE_INDEX.md`, then verify the critical details against the actual source code. Never blindly trust stale documentation or assume unverified behavior.

---

## 2. Trace-by-Flow

Trace execution paths end-to-end according to the application tier:

### A. Backend API Flow
```
Client Request
  ↓
[Host Entry Point] (Program.cs / Middleware)
  ↓
[API Endpoint / Controller] (Health, Auth, Resource)
  ↓
[Application Service / Handler] (Orchestration, Validation, Quota Check)
  ↓
[Domain Model] (Business Invariants, Entities, Value Objects)
  ↓
[Infrastructure / Repository] (EF Core DbContext, Npgsql, S3 Client)
  ↓
[Data Store / External Provider] (PostgreSQL, Storage, AI Gateway)
```

### B. Frontend / UI Flow
```
User Interaction / Route Navigation
  ↓
[Route Page / View] (Next.js App Router / Screen)
  ↓
[Component] (React Component, UI Tree)
  ↓
[Hook / State Manager] (Zustand, React Query, Local State)
  ↓
[API Client / Contract Adapter] (Typed Client, Fetch, Schema Validator)
  ↓
[Backend Contract] (ASP.NET Core Endpoint Contract)
```

### C. Background Worker / Queue Flow
```
Trigger / Scheduled Event / Queue Message
  ↓
[Worker Entry Point] (BackgroundService / Worker Host)
  ↓
[Message / Job Handler] (Scoped Context, Tenant Authorization)
  ↓
[Processing Pipeline] (Document Parser, Python Extractor, AI Pipeline)
  ↓
[Persistence / S3 Lifecycle] (DB Transaction, S3 Object Storage)
```

---

## 3. Pre-Edit Analysis

For nontrivial changes, document a concise pre-edit summary:

- **Target Files**: Explicit repository-relative paths to be touched.
- **Affected Execution Flow**: Which of the flows above is affected?
- **Root Cause / Implementation Reason**: Why is this change necessary?
- **Expected Behavior**: What will happen after the change?
- **Compatibility Considerations**: Does this preserve backwards compatibility with API consumers and data schema?
- **Relevant Tests**: What tests will prove correctness and regression safety?

*Note*: Keep this analysis brief and focused. Typo corrections, documentation updates, or isolated trivial changes do not require a formal report.

---

## 4. Root Cause Over Symptom

When resolving bugs or test failures:
- Gather concrete evidence of the root failure before applying defensive code.
- Avoid arbitrary workarounds: no arbitrary timeouts, blind null-coalescing, unconditional retries, or silent exception swallowing that merely mask the root defect.
- Fix the problem at its origin (contract validation, domain model, or configuration validator).

---

## 5. Smallest Safe Change

- Prefer the minimal, coherent implementation that completely satisfies the acceptance criteria.
- Resist unprompted refactoring, gratuitous formatting churn, or re-architecting unrelated components.
- Keep diffs focused, reviewable, and testable.

---

## 6. Risk-Based Verification Matrix

High-risk subsystems require mandatory pre-edit risk assessment and explicit regression testing:

| Subsystem | Risk Factors | Required Verification |
|---|---|---|
| **Authentication & Authorization** | Cross-tenant leak, unauthorized bypass | Server-side Data Space ownership validation, negative access tests. |
| **Data Space Isolation** | Tenant cross-access, composite key bypass | Tenant-safe queries, composite FK checks, adversarial cross-user tests. |
| **Payments & Quota** | Double billing, unmetered AI usage, race conditions | Atomic quota reservation/settle/release, idempotency tests. |
| **Database Migrations** | Data loss, lock contention, irreversible drift | Incremental EF Core migration review, forward/rollback plans, PostgreSQL runtime test. |
| **Storage & File Lifecycle** | Private URL exposure, orphaned objects | Scoped signed URL lifetime, placement deletion vs binary lifecycle tests. |
| **AI Retrieval & Provenance** | Fake citations, unsourced generation, prompt injection | Source-version tracking, uncertainty exposure, scope validation. |
| **Concurrency & Workers** | Job loss, poisoned queues, split-brain state | Idempotent workers, bounded retries, scoped job credentials. |

---

## 7. Verification Before Claiming Completion

A change is complete only when:
1. The code builds without warnings or errors.
2. Targeted tests pass with actual command output recorded.
3. Relevant file catalogs and documentation are updated.
4. No secrets, credentials, or private artifacts are committed.
