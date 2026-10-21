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

---

## 4. Verified Bootstrap and Privilege Clarification (2026-10-20)

- Database service roles are created or rotated by the administrator-run `scripts/Initialize-DatabaseRoles.ps1` and `infra/postgres/bootstrap-roles.sql` **before** EF migrations. Passwords come from approved process environment configuration and are never embedded in supported scripts or command arguments.
- After migrations, `scripts/Set-DatabaseRuntimePrivileges.ps1` replaces runtime access with the explicit allowlist in `infra/postgres/apply-runtime-privileges.sql`. `studyai_app` receives only database CONNECT, approved schema USAGE, and DML on reviewed tables.
- Automated PostgreSQL evidence denies `studyai_app` access to `ops.__ef_migrations_history`, DDL/ALTER, table ownership, grant effects/options, TEMP, public-schema CREATE, elevated role membership, and newly created unapproved tables.
- These repository scripts manage explicit privileges in the target database only. They do **not** claim ownership of cluster-wide configuration or ambient/default ACLs created by a hosting provider, DBA, image, or another object owner. Every target environment must run the scripts and the privilege probes; inherited cluster state is not trusted.
- The current RLS evidence covers the incrementally migrated high-risk subset. It is not a claim that all future Campus tables already have policies. The request-owned authorization/transaction boundary remains separately tracked by `auth-database-security-remediation` section 5.
