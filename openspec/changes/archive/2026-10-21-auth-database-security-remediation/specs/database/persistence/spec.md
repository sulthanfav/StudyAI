# Spec Delta

## ADDED Requirements

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
