# File Catalog: Domain Module (`src/StudyAI.Domain`)

Owning Module: `src/StudyAI.Domain`  
Module Documentation: [`../MODULE_INDEX.md`](../MODULE_INDEX.md)  
Status: **VERIFIED**

---

## Catalog Entries

### `src/StudyAI.Domain/StudyAI.Domain.csproj`
- **Module**: `src/StudyAI.Domain`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Project build configuration for enterprise domain logic targeting `net10.0`.
- **Public Interfaces / Exports**: MSBuild project.
- **Dependencies**: None (enforced zero dependencies by Clean Architecture rule).
- **Known Callers**: `StudyAI.Application`, `StudyAI.Infrastructure`.
- **Associated Tests**: `ProjectReferenceRulesTests.cs`.
- **Security / Architectural Notes**: Domain layer must remain pure; no EF Core, ASP.NET, or provider packages.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/packages.lock.json`
- **Module**: `src/StudyAI.Domain`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Identity/User.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Represents user identity and account state in the academic personal operating system.
- **Public Interfaces / Exports**: `User` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Pure domain POCO; maps to `identity.users`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Ownership/DataSpace.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Represents a tenant data boundary / ownership scope for user assets.
- **Public Interfaces / Exports**: `DataSpace` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Primary tenant boundary; maps to `ownership.data_spaces`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Workspace/ContentSpace.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Represents an organizational container (library, course, project) within a Data Space.
- **Public Interfaces / Exports**: `ContentSpace` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `workspace.content_spaces`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Knowledge/KnowledgeItem.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Canonical knowledge item entity representing notes, documents, or saved AI outputs.
- **Public Interfaces / Exports**: `KnowledgeItem` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `knowledge.knowledge_items`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Ai/EmbeddingModel.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Catalog definition of AI vector embedding models and dimensions.
- **Public Interfaces / Exports**: `EmbeddingModel` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Global catalog table; maps to `ai.embedding_models`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Documents/StorageObject.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Metadata record for binary files in private S3-compatible object storage.
- **Public Interfaces / Exports**: `StorageObject` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `documents.storage_objects`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Documents/Document.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Subtype entity linking a knowledge item to document version history.
- **Public Interfaces / Exports**: `Document` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `documents.documents`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Documents/DocumentVersion.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Specific immutable binary revision of an ingested document.
- **Public Interfaces / Exports**: `DocumentVersion` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `documents.document_versions`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Documents/ProcessingRun.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Execution record of document extraction and parsing.
- **Public Interfaces / Exports**: `ProcessingRun` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `documents.processing_runs`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Documents/DocumentChunk.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Extracted textual passage and bounding locator from an academic source.
- **Public Interfaces / Exports**: `DocumentChunk` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `documents.document_chunks`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Documents/ChunkEmbedding.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: High-dimensional semantic vector embedding for academic chunk retrieval.
- **Public Interfaces / Exports**: `ChunkEmbedding` class.
- **Dependencies**: None (`float[]` keeps domain pure of third-party pgvector dependency).
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `PersistenceFixture`.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Protected by RLS via `data_space_id`; maps to `documents.chunk_embeddings`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Ownership/DataSpaceMembership.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Expresses actor membership and roles (owner, member, viewer) within a Data Space.
- **Public Interfaces / Exports**: `DataSpaceMembership` class.
- **Dependencies**: None.
- **Known Callers**: `IApplicationDbContext`, `StudyAIDbContext`, `AuthService`, `AuthorizedDataSpaceScopeFactory`.
- **Associated Tests**: `UserRegistrationIntegrationTests.cs`, `DataSpaceAuthorizationIntegrationTests.cs`.
- **Security / Architectural Notes**: Enforces explicit user-to-dataspace authorization boundary; maps to `ownership.data_space_memberships`.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Domain/Identity/UserSession.cs`
- **Module**: `src/StudyAI.Domain`
- **Category**: Domain Entity
- **Responsibility**: Server-verifiable browser session state with a hashed opaque identifier, security-stamp snapshot, absolute expiry, revocation, and last-seen timestamps.
- **Public Interfaces / Exports**: `UserSession` class.
- **Dependencies**: None.
- **Known Callers**: `StudyAIDbContext`, authentication session services.
- **Associated Tests**: `SessionModelMetadataTests.cs`, `ServerSessionLifecycleIntegrationTests.cs`.
- **Security / Architectural Notes**: Never persists the plaintext session identifier carried by the encrypted cookie.
- **Verification Status**: `VERIFIED`
