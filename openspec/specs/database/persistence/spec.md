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
