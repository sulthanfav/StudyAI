# StudyAI Database Runtime Validation Report

Status: **PASSED as a clean-slate PostgreSQL runtime candidate; NOT production-ready**  
Validation date: 2026-10-19 (Asia/Jakarta)  
Canonical SQL under test: `StudyAI_Physical_Schema_Reviewed_v1.1.sql`  
Canonical SQL SHA-256: `0545E8A38069D8DCF739070FD65B4C6C129AC5B390724506FE17D7177538F7DE`

## 1. Executive result

The reviewed v1.1 schema executed without SQL errors in three separate empty databases on PostgreSQL 16.15. The final validation run verified the required extensions, 14 application schemas, 68 tables, foreign keys and other constraints, indexes, critical functions, and triggers. All 10 notices expected from the supplied smoke-test script were `PASS`; there were no `FAIL` notices. The smoke transaction rolled back cleanly. A custom-format dump restored successfully to a second empty database, where all 68 application tables were present.

No defect was found that justified changing the canonical SQL. The canonical file was therefore left byte-for-byte unchanged.

This evidence changes the schema's status from an unexecuted review candidate to a **runtime-validated clean-slate candidate only**. It does not prove production security, migration safety, concurrency behavior, performance, RLS behavior, or operational readiness.

## 2. Scope and controls

Validation was performed only against a local Docker database created for this work. No production or shared database was targeted.

| Control | Result |
|---|---|
| Docker Engine connectivity | PASS — client/server 29.8.2, Linux engine through `desktop-linux` |
| Docker Compose | PASS — 5.6.0 |
| Database image | `pgvector/pgvector:0.8.7-pg16-bookworm` |
| Pulled image digest | `pgvector/pgvector@sha256:7b822b0aac60967beb1ea5e576b8602c94c300a157d187f385ae3e0da199b90a` |
| Isolation | Timestamped validation and restore databases inside the dedicated Compose project `studyai-db-validation` |
| Persistence | Named volume `studyai-postgres-validation-data`; retained after validation |
| Host exposure | Local port `55432` mapped to container port `5432` |
| Credentials | Random local credential in ignored `infra/compose/.env.database.local`; values are redacted from logs |
| Canonical inputs | Database source directory mounted read-only in the container |
| Error behavior | `psql -X -v ON_ERROR_STOP=1`; script stops on any non-zero native exit code or failed assertion |

The running container was intentionally left healthy after validation, and the persistent volume was not removed.

The retained local runtime currently contains these six isolated evidence databases:

- `studyai_validation_20261019_145955`
- `studyai_validation_20261019_145955_restore`
- `studyai_validation_20261019_150215`
- `studyai_validation_20261019_150215_restore`
- `studyai_validation_20261019_150642`
- `studyai_validation_20261019_150642_restore`

They were not deleted because the validation request requires preserving the persistent Docker volume.

## 3. Reproducible assets

The following assets implement the validation harness:

- `infra/compose/compose.database.yml` — pinned PostgreSQL/pgvector service, health check, read-only SQL mounts, and named persistent volume.
- `infra/postgres/verify-runtime.sql` — catalog inventory plus fail-fast assertions for version, extensions, schema/table counts, critical functions, triggers, indexes, and constraints.
- `scripts/Validate-Database.ps1` — Windows entry point that creates an isolated database, applies the schema, verifies objects, runs smoke tests, checks rollback, and validates dump/restore.

Run from the repository root:

```powershell
& .\scripts\Validate-Database.ps1
```

The default behavior preserves the container and named volume. `-StopContainerAfter` stops the container but still retains the volume. The script has no volume-removal path.

Generated evidence is written below `artifacts/database-validation/<timestamp>/` and is ignored by Git because it is runtime output. The authoritative final run is:

- Validation database: `studyai_validation_20261019_150642`
- Restore database: `studyai_validation_20261019_150642_restore`
- Evidence directory: `artifacts/database-validation/20261019-150642`
- Completion time: `2026-10-19T15:06:53+07:00`
- Script exit code: `0`

Two earlier independent fresh-database runs (`studyai_validation_20261019_145955` and `studyai_validation_20261019_150215`) also completed successfully. This supplies repeatability evidence across three empty database names on the retained runtime.

## 4. Server and extension verification

| Item | Observed value | Result |
|---|---:|---|
| PostgreSQL | 16.15, Debian build, x86_64 | PASS |
| `vector` | 0.8.7 | PASS |
| `pg_trgm` | 1.6 | PASS |
| `btree_gist` | 1.7 | PASS |

The validation assertion rejects PostgreSQL major versions lower than 16 and fails if any required extension is missing.

## 5. Runtime object inventory

### Schemas and tables

All 14 expected application schemas exist, containing 68 tables in total.

| Schema | Tables |
|---|---:|
| `ai` | 7 |
| `billing` | 10 |
| `documents` | 8 |
| `identity` | 1 |
| `integrity` | 2 |
| `knowledge` | 9 |
| `learning` | 8 |
| `ops` | 3 |
| `ownership` | 1 |
| `planner` | 3 |
| `presentation` | 3 |
| `research` | 6 |
| `workspace` | 4 |
| `writing` | 3 |
| **Total** | **68** |

### Constraints

| Constraint type | Count |
|---|---:|
| Check | 84 |
| Exclusion | 1 |
| Foreign key | 107 |
| Primary key | 68 |
| Unique | 60 |
| **Total** | **320** |

The critical composite foreign key `fk_document_current_version` and the active-subscription exclusion constraint `ex_subscription_one_active_per_space` were explicitly resolved from the PostgreSQL catalogs.

### Indexes

The application schemas contain 157 indexes. The validation script explicitly found:

- `workspace.ux_active_item_per_content_space`
- `documents.ux_processing_active_run`
- `billing.ix_subscription_active`

The inventory includes indexes created for primary/unique constraints as well as explicitly declared indexes; it is an object inventory, not a query-performance claim.

### Functions and triggers

Five critical trigger functions were found:

- `ai.guard_embedding_model_identity`
- `documents.ensure_embedding_dimensions`
- `documents.guard_document_version_identity`
- `knowledge.guard_source_version_identity`
- `ops.reject_snapshot_update`

Eight non-internal trigger objects were found. `information_schema.triggers` returns nine event rows because `trg_embedding_dimensions` covers both `INSERT` and `UPDATE`; this is not a ninth trigger object.

## 6. Smoke-test results

`StudyAI_DB_Smoke_Tests_v1.1.sql` ran in a transaction and produced exactly the 10 expected PASS notices and zero FAIL notices.

| Scenario | Runtime result |
|---|---|
| 01 — Cross-tenant placement is blocked | PASS |
| 02 — Knowledge-item subtype mismatch is blocked | PASS |
| 03 — Source-version provenance is immutable | PASS |
| 03A — Note source snapshot is immutable | PASS |
| 03B — Document-version identity is immutable | PASS |
| 04 — Only one active processing run is allowed | PASS |
| 05 — Active subscription periods cannot overlap | PASS |
| 06 — A stale note version is detected | PASS |
| 07 — Quota overdraw is blocked | PASS |
| 08 — One job/resource can bind to daily and monthly periods | PASS |

After the script's `ROLLBACK`, `identity.users` contained zero rows, confirming that smoke fixtures did not persist.

## 7. Backup and restore evidence

The final validation database was exported with `pg_dump --format=custom`, restored with `pg_restore --exit-on-error` into `studyai_validation_20261019_150642_restore`, and queried through the application-schema inventory. The restored database contained all 68 expected tables. This proves a clean local schema dump/restore path for this empty fixture state; it is not yet a populated-data, point-in-time-recovery, disaster-recovery, or production backup test.

## 8. Failures and corrections

No canonical-schema statement and no supplied smoke scenario failed.

One issue was found in the validation harness during the first run: interpolated `docker compose config` output included the generated local database password in an ignored artifact log. The root cause was that the generic command logger captured command output without sensitive-value redaction. The PowerShell logger was corrected to redact registered secrets before console or file output, the first-run artifacts were sanitized, and the complete validation was rerun successfully. No canonical SQL was changed for this tooling issue.

## 9. What this validation does not prove

The following remain open or unverified and must not be inferred from the PASS result:

- RLS is not defined or exercised. There is no non-owner/non-superuser/non-`BYPASSRLS` role test or pooled user-A-to-user-B context-switch test. ADR-003 remains open.
- The smoke file demonstrates important constraint paths but is not a true multi-session concurrency test. Note-save races, quota reservations, and subscription changes still need concurrent sessions and deterministic lock-order checks.
- EF Core entities, mappings, provider behavior, migrations, rollback, and parity with the reviewed SQL have not been created or tested.
- No migration was tested against an existing populated database; the canonical file remains clean-slate-only and is not an incremental migration.
- Authorization is not proven by database constraints. Every application, retrieval, export, signed-object, stream, and worker path still requires server-side Data Space authorization.
- Recursive hierarchy cycles, canonical placement rules, lifecycle orchestration, deletion/restore behavior, retention, and orphan reconciliation require service-level transactions and dedicated tests.
- No representative data volume, query plans, performance/load limits, ANN strategy, embedding-model selection, or retrieval-quality result was evaluated.
- Only an empty-fixture schema backup/restore was tested; private object storage and coordinated database/object recovery were outside this phase.
- Auth, queue/outbox, runtime roles, storage lifecycle, AI/embedding provider, payments, quotas, editor, and production topology remain OPEN until accepted ADRs exist.

## 10. EF Core migration readiness

**Assessment: conditionally ready for EF Core mapping and migration design, but not ready to declare or ship production migrations.**

The runtime evidence is sufficient to use the reviewed v1.1 schema as a tested physical reference when designing one `DbContext` and one migration history. Before authoritative migrations are generated, the next phase must:

1. Accept or explicitly time-box the runtime-role/RLS design in ADR-003, and accept authentication choices before identity implementation.
2. Pin compatible .NET, EF Core, Npgsql, and pgvector package versions.
3. Define the migration ownership strategy for extensions, schemas, generated/search columns, partial indexes, GiST exclusion constraints, composite/deferrable foreign keys, vector columns, functions, and triggers.
4. Convert the clean-slate reference into reviewed incremental migrations rather than embedding the entire SQL file as one production migration.
5. Add migration parity checks against PostgreSQL catalogs, upgrade tests from populated fixtures, rollback/forward plans, and dump/restore integrity checks.
6. Add real concurrent-session and tenant-isolation tests, then decide which invariants belong in constraints, application/domain services, or both.

Phase 2 application scaffolding and EF migrations were intentionally not created during this work.

## 11. Final disposition

- Canonical SQL: unchanged; clean-slate runtime execution PASS.
- Supplied smoke tests: 10 PASS, 0 FAIL.
- Object inventory assertions: PASS.
- Empty-fixture dump/restore: PASS.
- Persistent Docker volume: retained.
- Production readiness: **NOT established**.
- EF Core migration work: **may proceed only as a gated design/implementation phase with the prerequisites above**.
