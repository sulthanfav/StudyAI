# ADR-003: PostgreSQL Runtime Roles, Connection Pooling, and Row-Level Security

Status: **ACCEPTED**  
Decision Date: 2026-10-20  
Decision Owner: Product Owner / Technical Owner  
Governing References: PRD OD-04; Core Architecture v1.2 OPEN-09, LOCKED-01; Database Review v1.1 ADR-RLS / DBR-10

---

## 1. Context

StudyAI Campus V1 requires robust tenant isolation and database-level security. The architecture must defend against:
1. Cross-tenant data leakage if an application query omits a tenant filter.
2. Ambient context leakage across shared physical connections in Npgsql connection pooling.
3. Excessive database privileges leading to destructive DDL from web runtime accounts.
4. Silent failures or data exposure when tenant context is missing or malformed.

---

## 2. Decision

1. **Least-Privilege Database Role Hierarchy**:
   - `studyai_migrator`: Schema owner; full DDL; used strictly by CI/CD migrations.
   - `studyai_app`: Web API runtime role; DML only (`SELECT`, `INSERT`, `UPDATE`, `DELETE`); USAGE on sequences and schemas; **NO DDL; NO BYPASSRLS**.
   - `studyai_worker`: Least-privilege worker scoped to `ops.*`, `documents.*`, and `ai.*` processing tables (introduced incrementally as needed).
   - `studyai_ro`: Read-only replica account for reporting and analytics.
   - **Superuser / Schema Owner Prohibition**: The running application and test harnesses must NEVER connect as superuser or table owner.

2. **Row-Level Security as Defense-in-Depth**:
   - RLS is activated as a secondary hardware lock beneath application-level authorization.
   - Application code remains obligated to explicitly filter queries by `data_space_id`.
   - Sensitive tenant-segregated tables enforce `ENABLE ROW LEVEL SECURITY` and `FORCE ROW LEVEL SECURITY`.

3. **Transaction-Local Session Context (`is_local = true`)**:
   - Tenant context MUST be established within an active transaction using:
     ```sql
     SELECT set_config('app.current_data_space_id', @DataSpaceId, true);
     ```
   - Scoping to the transaction (`true`) guarantees that PostgreSQL automatically discards the context when the transaction commits or aborts.
   - Connections returned to the Npgsql connection pool have zero lingering ambient state.

4. **Fail-Closed Policy**:
   - RLS policies use `NULLIF(current_setting('app.current_data_space_id', true), '')::uuid`.
   - If context is missing, blank, or invalid, the policy evaluates to FALSE and rejects reads/writes immediately.

5. **Staged Adoption**:
   - RLS is initially enabled on high-risk document, chunk, note, and quota tables. Expansion across the full 68-table catalog occurs in verified increments.

---

## 3. Consequences

- **Positive**: Immune to connection-pooling context leakage; hard barrier against accidental cross-tenant queries; compromised app credentials cannot alter database schema.
- **Negative / Constraints**: Queries executed without an active transaction cannot read RLS-protected tables; requires EF Core transaction interceptors or explicit transaction wrappers.
