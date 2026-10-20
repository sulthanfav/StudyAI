# File Catalog: Infrastructure Module (`src/StudyAI.Infrastructure`)

Owning Module: `src/StudyAI.Infrastructure`  
Module Documentation: [`../MODULE_INDEX.md`](../MODULE_INDEX.md)  
Status: **VERIFIED**

---

## Catalog Entries

### `src/StudyAI.Infrastructure/StudyAI.Infrastructure.csproj`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Project build configuration for persistence and external adapters targeting `net10.0`.
- **Public Interfaces / Exports**: MSBuild project.
- **Dependencies**: `StudyAI.Domain`, `StudyAI.Application`, `StudyAI.Contracts`, EF Core, Npgsql, pgvector.
- **Known Callers**: `StudyAI.Api`.
- **Associated Tests**: `ProjectReferenceRulesTests.cs`, `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Houses EF Core persistence, transaction interceptors, and RLS policies.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/packages.lock.json`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/DependencyInjection.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Service Registration
- **Responsibility**: Registers Infrastructure persistence services (`StudyAIDbContext`, accessors, interceptors).
- **Public Interfaces / Exports**: `AddInfrastructurePersistence` extension method.
- **Dependencies**: `StudyAI.Application`, `StudyAI.Domain`.
- **Known Callers**: `Program.cs` in `StudyAI.Api`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Wires scoped interceptors and configures Npgsql connection strings.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/StudyAIDbContext.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence / EF Core
- **Responsibility**: Primary EF Core `DbContext` implementation for StudyAI Campus V1.
- **Public Interfaces / Exports**: `StudyAIDbContext`, implements `IApplicationDbContext`.
- **Dependencies**: `StudyAI.Domain`, `StudyAI.Application`, `Microsoft.EntityFrameworkCore`.
- **Known Callers**: Service layer, repository operations, test fixtures.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Enforces schema conventions, extension loading, and interceptor integration.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/StudyAIDbContextFactory.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence / Design-Time
- **Responsibility**: Design-time factory for EF Core CLI migrations tooling.
- **Public Interfaces / Exports**: `IDesignTimeDbContextFactory<StudyAIDbContext>`.
- **Dependencies**: `Microsoft.EntityFrameworkCore.Design`.
- **Known Callers**: `dotnet ef` CLI tool.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Reads optional environment connection string for local schema generation.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/CurrentDataSpaceAccessor.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence / Multi-Tenancy
- **Responsibility**: Manages the ambient transaction-scoped tenant identity via `AsyncLocal<Guid?>`.
- **Public Interfaces / Exports**: `CurrentDataSpaceAccessor`, implements `ICurrentDataSpaceAccessor`, `ICurrentDataSpaceContext`.
- **Dependencies**: `StudyAI.Application`.
- **Known Callers**: `TenantSessionInterceptor`, HTTP auth middleware, tests.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Context is kept transaction-local and fails closed when empty.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Interceptors/TenantSessionInterceptor.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence / Security
- **Responsibility**: `DbTransactionInterceptor` executing `SELECT set_config('app.current_data_space_id', @space_id, true)`.
- **Public Interfaces / Exports**: `TenantSessionInterceptor`.
- **Dependencies**: `Microsoft.EntityFrameworkCore.Diagnostics`, `StudyAI.Application`.
- **Known Callers**: EF Core pipeline.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Ensures fail-closed transaction-local tenant context and connection-pooling hygiene.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Identity/UserConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `identity.users` including xmin row versioning.
- **Public Interfaces / Exports**: `UserConfiguration`.
- **Dependencies**: `StudyAI.Domain.Identity`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Mapped to `identity.users`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Ownership/DataSpaceConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `ownership.data_spaces`.
- **Public Interfaces / Exports**: `DataSpaceConfiguration`.
- **Dependencies**: `StudyAI.Domain.Ownership`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Mapped to `ownership.data_spaces`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Workspace/ContentSpaceConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `workspace.content_spaces`.
- **Public Interfaces / Exports**: `ContentSpaceConfiguration`.
- **Dependencies**: `StudyAI.Domain.Workspace`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Mapped to `workspace.content_spaces`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Knowledge/KnowledgeItemConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `knowledge.knowledge_items` with xmin concurrency.
- **Public Interfaces / Exports**: `KnowledgeItemConfiguration`.
- **Dependencies**: `StudyAI.Domain.Knowledge`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS policy.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Ai/EmbeddingModelConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `ai.embedding_models`.
- **Public Interfaces / Exports**: `EmbeddingModelConfiguration`.
- **Dependencies**: `StudyAI.Domain.Ai`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Mapped to `ai.embedding_models`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Documents/StorageObjectConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `documents.storage_objects`.
- **Public Interfaces / Exports**: `StorageObjectConfiguration`.
- **Dependencies**: `StudyAI.Domain.Documents`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS policy.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Documents/DocumentConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `documents.documents`.
- **Public Interfaces / Exports**: `DocumentConfiguration`.
- **Dependencies**: `StudyAI.Domain.Documents`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS policy.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Documents/DocumentVersionConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `documents.document_versions`.
- **Public Interfaces / Exports**: `DocumentVersionConfiguration`.
- **Dependencies**: `StudyAI.Domain.Documents`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS policy.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Documents/ProcessingRunConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `documents.processing_runs`.
- **Public Interfaces / Exports**: `ProcessingRunConfiguration`.
- **Dependencies**: `StudyAI.Domain.Documents`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS policy.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Documents/DocumentChunkConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `documents.document_chunks`.
- **Public Interfaces / Exports**: `DocumentChunkConfiguration`.
- **Dependencies**: `StudyAI.Domain.Documents`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS policy.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Documents/ChunkEmbeddingConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Persistence Configuration
- **Responsibility**: Entity configuration for `documents.chunk_embeddings` with pgvector mapping.
- **Public Interfaces / Exports**: `ChunkEmbeddingConfiguration`.
- **Dependencies**: `StudyAI.Domain.Documents`, `Pgvector.EntityFrameworkCore`.
- **Known Callers**: `StudyAIDbContext`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS policy.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Migrations/20261020045112_InitialPersistenceBaseline.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Database Migration
- **Responsibility**: Initial EF Core migration creating foundational 11-table subset, RLS, and triggers.
- **Public Interfaces / Exports**: `InitialPersistenceBaseline` migration class.
- **Dependencies**: `Microsoft.EntityFrameworkCore.Migrations`.
- **Known Callers**: EF Core migrator.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Applies least-privilege role grants and fail-closed RLS policies.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Migrations/20261020045112_InitialPersistenceBaseline.Designer.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Database Migration Metadata
- **Responsibility**: Designer metadata for initial baseline migration.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Migrations/StudyAIDbContextModelSnapshot.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Database Migration Metadata
- **Responsibility**: Current compiled model snapshot for EF Core migrations pipeline.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Identity/AuthService.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Service Implementation
- **Responsibility**: Implements `IAuthService` coordinating atomic user registration, SignInManager logins, logouts, and user summaries.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Identity/DataSpaceAuthorizationService.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Service Implementation
- **Responsibility**: Server-side authorization verifying actor data space ownership or active membership.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Identity/StudyAIUserStore.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Identity Store Implementation
- **Responsibility**: EF Core backed ASP.NET Core Identity store for users, passwords, emails, security stamps, and lockouts.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Migrations/20261020053351_AddIdentityAndMembership.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Database Migration
- **Responsibility**: Incremental EF Core migration introducing ASP.NET Core Identity columns and `ownership.data_space_memberships`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Migrations/20261020053351_AddIdentityAndMembership.Designer.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Database Migration Metadata
- **Responsibility**: EF Core designer metadata for identity and membership migration.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Infrastructure/Persistence/Configurations/Ownership/DataSpaceMembershipConfiguration.cs`
- **Module**: `src/StudyAI.Infrastructure`
- **Category**: Entity Configuration
- **Responsibility**: EF Core entity type configuration mapping `DataSpaceMembership` to `ownership.data_space_memberships`.
- **Verification Status**: `VERIFIED`
