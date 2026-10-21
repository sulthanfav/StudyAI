# Database Persistence & Concurrency Specification

## Purpose

Defines the Entity Framework Core persistence architecture, Central Package Management pinning, single DbContext boundary, multi-session concurrency protection, and schema parity verification for StudyAI Campus V1.

## Requirements

### Requirement: Central package management and reproducible dependencies
All database persistence packages SHALL be declared and pinned in `Directory.Packages.props` with locked dependencies. Transitive dependency drift SHALL be prevented via NuGet lock files.

#### Scenario: Clean restore with locked mode
- **WHEN** a contributor executes `dotnet restore StudyAI.slnx --locked-mode`
- **THEN** package restore succeeds using only pinned versions without floating dependencies

### Requirement: Single primary DbContext boundary
The application architecture SHALL maintain exactly one primary `DbContext` (`StudyAIDbContext`) in `StudyAI.Infrastructure`, registering all domain entity mappings through schema-isolated configuration classes without circular layer references.

#### Scenario: Architecture boundary verification
- **WHEN** architecture tests verify dependencies of the persistence layer
- **THEN** `StudyAI.Domain` and `StudyAI.Application` contain no package references to `Npgsql` or `Microsoft.EntityFrameworkCore`

### Requirement: Optimistic concurrency for note editing
The persistence layer SHALL enforce optimistic concurrency on note revisions. Concurrent updates to the same note version MUST detect the conflict and reject stale writes with an explicit conflict error.

#### Scenario: Concurrent note updates
- **WHEN** Session 1 and Session 2 concurrently read Note version 1, and Session 1 successfully commits revision 2
- **THEN** Session 2 attempting to commit revision 2 is rejected due to concurrency violation and does not overwrite Session 1's changes

### Requirement: Atomic AI quota reservation and settlement
Quota allocations SHALL be reserved atomically before initiating chargeable AI operations. Concurrent reservation requests MUST NOT cause over-allocation beyond the allocated limit.

#### Scenario: Concurrent quota reservation at capacity
- **WHEN** multiple concurrent transactions request quota exceeding the remaining unreserved allocation
- **THEN** exactly those requests fitting within the available quota succeed, and all excess requests fail atomically without quota deficit

### Requirement: Schema parity verification
The persistence metadata and generated migrations SHALL maintain 100% parity with the verified clean-slate schema `StudyAI_Physical_Schema_Reviewed_v1.1.sql` across all 68 tables, columns, constraints, and indexes.

#### Scenario: Parity check across database catalogs
- **WHEN** automated catalog parity checks compare the EF Core model against the clean-slate reference SQL
- **THEN** all schemas, tables, primary keys, foreign keys, unique constraints, and indexes match with zero discrepancies

### Requirement: Secret-free database configuration
Database passwords and replacement credentials MUST come from approved environment-specific secret configuration. Source files, EF migrations, generated SQL, test output, and default logs SHALL contain no usable database password.

#### Scenario: Missing required database secret
- **WHEN** a migration, bootstrap, or runtime process starts without its required credential configuration
- **THEN** it fails before connecting and reports only the missing configuration key

### Requirement: Reproducible clean-room database bootstrap
The repository SHALL document and automate prerequisites that create an isolated PostgreSQL database, required extensions, role grants, and schema from an empty approved server without relying on prior volume state or manual ACL changes.

#### Scenario: Empty isolated server
- **WHEN** the clean-room procedure runs against a new supported PostgreSQL server with supplied bootstrap credentials
- **THEN** all migrations and explicit grants succeed and the runtime role passes the required permission checks

#### Scenario: Existing ambient default privileges are absent
- **WHEN** the target server has no pre-existing default ACL for StudyAI roles
- **THEN** the resulting runtime permissions are identical to the documented baseline

### Requirement: Forward-only remediation of applied migrations
Previously applied migrations and Git history MUST NOT be silently rewritten. Security corrections SHALL use reviewed forward migrations or an explicitly approved clean-baseline replacement plan.

#### Scenario: Existing database receives remediation
- **WHEN** an environment already contains the Iteration 3 or Iteration 4 migrations
- **THEN** applying the remediation preserves existing user data and advances migration history without editing prior rows

### Requirement: Normalized-email constraint preflight
Before enforcing case-insensitive identity uniqueness, deployment SHALL detect normalized collisions and stop without choosing or deleting a record automatically.

#### Scenario: Collision-free migration
- **WHEN** no two users share the same normalized email
- **THEN** the unique normalized-email constraint is added successfully

#### Scenario: Collision detected
- **WHEN** two or more existing users normalize to the same email
- **THEN** migration stops before constraint creation and produces a non-secret operator diagnostic
