# api-host Specification

## Purpose

Defines the safe, observable operating contract for the StudyAI ASP.NET Core API host before feature modules and decision-gated infrastructure are implemented.

## Requirements

### Requirement: Deterministic host startup
The API host SHALL start from a repository-pinned supported .NET SDK and SHALL terminate with a non-zero exit when startup configuration validation fails.

#### Scenario: Valid foundation configuration
- **WHEN** the API host starts with valid foundation configuration
- **THEN** it begins accepting HTTP requests without requiring authentication, database, queue, storage, or AI provider configuration

#### Scenario: Invalid explicit configuration
- **WHEN** a configured foundation value is missing or malformed and is required for startup
- **THEN** startup fails before accepting requests and reports the configuration key without reporting its secret value

### Requirement: Separate liveness and readiness signals
The API host SHALL expose separate unauthenticated liveness and readiness endpoints with stable machine-readable responses that contain no secrets, credentials, connection strings, or private content.

#### Scenario: Process is alive
- **WHEN** a client requests `/health/live` while the process event loop is responsive
- **THEN** the host returns a successful status without probing optional external dependencies

#### Scenario: Foundation is ready
- **WHEN** a client requests `/health/ready` after all registered readiness checks succeed
- **THEN** the host returns a successful status identifying the service and overall health state

#### Scenario: A required readiness check fails
- **WHEN** a registered required dependency reports unhealthy
- **THEN** `/health/ready` returns a non-success status and a safe component-level result without exposing configuration values

### Requirement: Correlated structured request logs
The API host SHALL emit structured request lifecycle logs with one correlation identifier and SHALL return that identifier to the caller for support diagnostics.

#### Scenario: Caller supplies a valid correlation identifier
- **WHEN** a request contains a syntactically valid correlation identifier
- **THEN** the host uses that identifier in the request scope and response header

#### Scenario: Caller omits or supplies an invalid identifier
- **WHEN** a request has no acceptable correlation identifier
- **THEN** the host generates a new bounded identifier and uses it consistently for that request

### Requirement: Sensitive data is excluded from default telemetry
Default host logging SHALL record operational metadata only and MUST NOT record credentials, tokens, authorization headers, signed URLs, connection strings, request or response bodies, prompts, notes, or document contents.

#### Scenario: Request contains sensitive headers or body content
- **WHEN** the host processes a request containing authorization material or private content
- **THEN** default logs contain request metadata and correlation context but not the sensitive values or body content

#### Scenario: Startup validation encounters a secret setting
- **WHEN** startup validation rejects a secret-bearing configuration setting
- **THEN** the diagnostic identifies the setting name and validation reason without outputting the setting value

### Requirement: Explicit modular composition boundary
The backend foundation SHALL preserve one API deployable and explicit dependency direction between Domain, Application, Infrastructure, Contracts, and API composition so future modules cannot rely on forbidden reverse references.

#### Scenario: Foundation dependency rules are checked
- **WHEN** automated architecture checks inspect project references
- **THEN** Domain has no dependency on Application, Infrastructure, Contracts, or API, and no lower layer references the API host

#### Scenario: A future module is composed
- **WHEN** a module is later added through its public registration contract
- **THEN** the API composition root can register it without accessing another feature module's internals

### Requirement: No premature capability claim
The foundation host SHALL NOT expose business, authentication, database, queue, storage, AI, billing, or tenant-authorization behavior until the corresponding accepted change and readiness gate exists.

#### Scenario: Foundation-only host is inspected
- **WHEN** the Iteration 1 host is started
- **THEN** only host-operability endpoints and framework-generated development metadata explicitly enabled by the environment are reachable

### Requirement: Reproducible automated verification
The repository SHALL provide documented root-level restore, build, and test verification that fails on compilation errors or failing tests and includes in-process API tests for the host contract.

#### Scenario: Foundation verification succeeds
- **WHEN** a contributor uses the documented verification sequence with the pinned SDK and resolved packages
- **THEN** restore, build, unit tests, architecture checks, and API host integration tests complete successfully

#### Scenario: Database behavior is introduced later
- **WHEN** a later change adds database mappings, migrations, authorization, or tenant-sensitive queries
- **THEN** its integration tests use real PostgreSQL and do not substitute SQLite or EF InMemory for PostgreSQL behavior
