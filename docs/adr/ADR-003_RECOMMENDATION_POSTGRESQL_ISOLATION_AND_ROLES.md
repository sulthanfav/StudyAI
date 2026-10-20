# ADR-003 Recommendation: PostgreSQL Runtime Roles, Connection Pooling, and RLS Defense-in-Depth

Status: **PROPOSED / PENDING APPROVAL**  
Date: 2026-10-20  
Governing references: PRD OD-04; Core Architecture v1.2 OPEN-09, LOCKED-01; Database Review v1.1 ADR-RLS / DBR-10

---

## 1. Executive Summary

This document presents the architectural recommendation for PostgreSQL runtime isolation, least-privilege database roles, connection pooling safety, and defense-in-depth Row-Level Security (RLS) policies for StudyAI Campus V1.

**Key Rule**: EF Core query filters and RLS are NOT mutually exclusive. EF Core filters provide application-level scoping, while PostgreSQL RLS provides an unbreakable defense-in-depth barrier preventing accidental cross-tenant data leakage even in the event of query bugs or SQL injection.

---

## 2. Recommended Database Role Hierarchy

The database server must distinguish administrative DDL permissions from application runtime DML:

```
[ superuser / postgres ] (Container / DB cluster bootstrap ONLY)
       │
       ├──> [ studyai_migrator ] (Schema Owner / DDL)
       │      • Owns application schemas, tables, indexes, views, and functions.
       │      • Full DDL permissions (CREATE, ALTER, DROP).
       │      • Used EXCLUSIVELY by EF Core migrations and CI/CD database tasks.
       │      • NEVER used by the running web API or background workers.
       │
       ├──> [ studyai_app ] (Web API Runtime / DML Only)
       │      • SELECT, INSERT, UPDATE, DELETE on application tables.
       │      • USAGE on sequences and schemas.
       │      • NO DDL privileges (cannot alter tables, drop indexes, or modify policies).
       │      • NO BYPASSRLS (unconditionally subject to Row-Level Security).
       │      • Enforced with FORCE ROW LEVEL SECURITY on sensitive tables.
       │
       ├──> [ studyai_worker ] (Background Worker Runtime)
       │      • Scoped DML on worker tables (`ops.*`, `documents.*`, `ai.*`).
       │      • Read-only access to knowledge/learning tables.
       │      • Lease-based job isolation.
       │
       └──> [ studyai_ro ] (Read-Only Analytical Replica)
              • SELECT only across analytical and reporting schemas.
```

---

## 3. Safe Connection Pooling & Transaction-Scoped Context

In ASP.NET Core applications using Npgsql connection pooling, multiple HTTP requests consecutively share physical PostgreSQL connections. Setting global session variables (e.g. `SET app.current_data_space_id = '...'`) creates a catastrophic risk of **ambient context leakage** if a transaction fails or the connection is returned without reset.

### Architectural Solution: Transaction-Local Configuration (`is_local = true`)

1. **Transaction Scoping**:
   All tenant context must be established inside an active transaction using:
   ```sql
   SELECT set_config('app.current_data_space_id', @DataSpaceId, true);
   ```
   The third parameter `true` enforces `LOCAL` lifetime. When the transaction commits or aborts (rolls back), PostgreSQL automatically discards the configuration.
2. **Post-Transaction Guarantee**:
   The subsequent query on the same physical connection (even if reused by the pool) sees `current_setting('app.current_data_space_id', true)` return `NULL`.
3. **EF Core Interceptor Integration**:
   A `DbTransactionInterceptor` in `StudyAI.Infrastructure` automatically invokes `set_config` immediately upon transaction creation and ensures no raw connection executes queries without context.

---

## 4. Defense-in-Depth RLS Policy Design

RLS is applied to high-risk tenant-segregated tables:
- `knowledge.knowledge_item`
- `documents.document_chunk`
- `documents.chunk_embedding`
- `learning.notes`
- `learning.note_revision`
- `billing.ai_quota_allocation`

### Policy Specification (Fail-Closed)
```sql
ALTER TABLE knowledge.knowledge_item ENABLE ROW LEVEL SECURITY;
ALTER TABLE knowledge.knowledge_item FORCE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_policy ON knowledge.knowledge_item
    FOR ALL
    TO studyai_app
    USING (
        data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid
    )
    WITH CHECK (
        data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid
    );
```

### Fail-Closed Behavior Matrix
| Session State | `current_setting(..., true)` | `NULLIF(..., '')` | Policy Evaluation | Result |
|---|---|---|---|---|
| Unset / Missing | `NULL` | `NULL` | `data_space_id = NULL` → `UNKNOWN` (False) | **FAIL-CLOSED (0 rows returned)** |
| Empty String `""` | `""` | `NULL` | `data_space_id = NULL` → `UNKNOWN` (False) | **FAIL-CLOSED (0 rows returned)** |
| Malformed UUID | Invalid text | Invalid text | Error on cast `::uuid` | **FAIL-CLOSED (Transaction aborts)** |
| Valid Data Space A | `"<uuid-A>"` | `"<uuid-A>"` | `data_space_id = '<uuid-A>'` | **PERMITS Space A records only** |

---

## 5. Security Invariants & Approvals Needed

1. **Application Code Responsibility**: The application must never rely on RLS alone. Queries must always filter by `data_space_id` explicitly; RLS acts as the secondary hardware lock.
2. **Approval Gate**: Applying RLS policies across all 68 production tables requires formal sign-off following the PoC results in this iteration.
