# Spec Delta

## ADDED Requirements

### Requirement: Runtime role cannot mutate migration metadata
The application runtime role SHALL have no INSERT, UPDATE, DELETE, TRUNCATE, DDL, ownership, or grant privilege on EF migration history and other administrative metadata.

#### Scenario: Runtime migration-history mutation
- **WHEN** a connection authenticated as the application runtime role attempts to insert, update, or delete a migration-history row
- **THEN** PostgreSQL rejects the operation with insufficient privilege

### Requirement: Runtime grants are explicit and reproducible
Runtime database access SHALL be granted only to the tables, sequences, schemas, and operations required by approved application workflows. Newly created objects MUST NOT depend on undocumented cluster default privileges.

#### Scenario: Membership access after clean bootstrap
- **WHEN** the runtime role performs an approved membership lookup or mutation after clean-room bootstrap
- **THEN** PostgreSQL permits only the documented operation and enforces applicable authorization policy

#### Scenario: Access to unapproved object
- **WHEN** the runtime role accesses an administrative or unapproved table
- **THEN** PostgreSQL rejects the operation

### Requirement: Authorized tenant work executes inside one transaction boundary
After actor-to-Data-Space authorization succeeds, every RLS-dependent operation SHALL execute inside an explicit transaction where tenant context is set locally. Setting local tenant context outside an active unit of work MUST NOT be treated as authorization evidence.

#### Scenario: Authorized request transaction
- **WHEN** an authenticated actor is authorized for a Data Space and an RLS-dependent operation starts
- **THEN** the operation begins a transaction, sets that Data Space context locally, executes, and commits or rolls back as one unit

#### Scenario: Missing transaction or context
- **WHEN** an RLS-dependent query executes without the authorized transaction context
- **THEN** it fails closed and returns no cross-tenant data

### Requirement: Membership roles are constrained
Persisted Data Space membership roles SHALL be limited to the approved role set and authorization SHALL evaluate the role required by the requested operation rather than treating every membership as unrestricted access.

#### Scenario: Invalid membership role
- **WHEN** a write attempts to persist a role outside `owner`, `member`, or `viewer`
- **THEN** PostgreSQL rejects the write and no membership is created or changed

#### Scenario: Insufficient approved role
- **WHEN** a member with a valid but insufficient role requests a restricted operation
- **THEN** the application returns HTTP 403 and performs no database mutation
