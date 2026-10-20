# Tenant Isolation & Database Security Specification

## Purpose

Defines the PostgreSQL runtime role separation, transaction-scoped tenant session context, fail-closed access control, connection-pooling safety, and defense-in-depth Row-Level Security (RLS) policies for StudyAI Campus V1.

## Requirements

### Requirement: Database role segregation
The database architecture SHALL define separate PostgreSQL roles for migration execution (`studyai_migrator`), application runtime (`studyai_app`), background worker execution (`studyai_worker`), and read-only querying (`studyai_ro`). The application runtime role SHALL NOT possess DDL privileges or `BYPASSRLS`.

#### Scenario: Application role attempts DDL
- **WHEN** a connection authenticated as `studyai_app` executes `CREATE TABLE`, `ALTER TABLE`, or `DROP TABLE`
- **THEN** PostgreSQL rejects the statement with insufficient privilege error

#### Scenario: Application role executes authorized DML
- **WHEN** a connection authenticated as `studyai_app` executes `SELECT`, `INSERT`, `UPDATE`, or `DELETE` on application tables within authorized schemas
- **THEN** PostgreSQL permits the statement subject to active RLS policies

### Requirement: Transaction-local session context
The application persistence layer SHALL establish tenant context using transaction-local configuration parameters (`SET LOCAL app.current_data_space_id` or `set_config('app.current_data_space_id', ..., true)`). Ambient session parameters MUST NOT persist across transaction boundaries.

#### Scenario: Transaction commit clears context
- **WHEN** a transaction sets `app.current_data_space_id` and subsequently commits
- **THEN** the subsequent query on the same physical connection sees no active `app.current_data_space_id`

#### Scenario: Transaction rollback clears context
- **WHEN** a transaction sets `app.current_data_space_id` and subsequently aborts or rolls back
- **THEN** the subsequent query on the same physical connection sees no active `app.current_data_space_id`

### Requirement: Fail-closed tenant isolation
All queries subject to Row-Level Security SHALL fail closed when `app.current_data_space_id` is missing, empty, or malformed, returning zero rows for reads and rejecting writes.

#### Scenario: Query without tenant context
- **WHEN** a query is executed on an RLS-protected table without setting `app.current_data_space_id`
- **THEN** the query returns zero rows and inserts are rejected

#### Scenario: Query with malformed tenant context
- **WHEN** a query is executed with an invalid UUID format in `app.current_data_space_id`
- **THEN** the query fails closed with a cast error or returns zero rows without leaking any data

### Requirement: Cross-tenant data isolation
The system SHALL prevent User A from reading or modifying records belonging to Data Space B even when User A submits direct SQL queries or forged identifiers.

#### Scenario: User A queries Data Space A
- **WHEN** the session context is set to Data Space A
- **THEN** queries return only records where `data_space_id` equals Data Space A

#### Scenario: User A attempts to access Data Space B
- **WHEN** the session context is set to Data Space A and a query targets records where `data_space_id` equals Data Space B
- **THEN** PostgreSQL returns zero rows and forbids updates/deletions

### Requirement: Pooled connection reuse safety
Sequential transactions executing on a reused pooled database connection SHALL be completely isolated.

#### Scenario: User B follows User A on pooled connection
- **WHEN** Transaction 1 executes for User A on Data Space A, completes, and Transaction 2 executes on the same connection for User B on Data Space B
- **THEN** Transaction 2 accesses only records in Data Space B and cannot observe any Data Space A records
