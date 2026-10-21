# File Catalog: Application Module (`src/StudyAI.Application`)

Owning Module: `src/StudyAI.Application`  
Module Documentation: [`../MODULE_INDEX.md`](../MODULE_INDEX.md)  
Status: **VERIFIED**

---

## Catalog Entries

### `src/StudyAI.Application/StudyAI.Application.csproj`
- **Module**: `src/StudyAI.Application`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Project build configuration for application use cases targeting `net10.0`.
- **Public Interfaces / Exports**: MSBuild project.
- **Dependencies**: `StudyAI.Domain`, `StudyAI.Contracts`.
- **Known Callers**: `StudyAI.Api`, `StudyAI.Infrastructure`.
- **Associated Tests**: `ProjectReferenceRulesTests.cs`.
- **Security / Architectural Notes**: May only depend on Domain and Contracts.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Common/Interfaces/ICurrentActorContext.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Service Contract / Interface
- **Responsibility**: Trusted actor context used to scope membership RLS.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Workspace/IContentSpaceReader.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Query Contract
- **Responsibility**: Persistence-neutral boundary for authorized content-space reads.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Workspace/ContentSpaceSummary.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Query Model
- **Responsibility**: Persistence-neutral content-space projection.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/packages.lock.json`
- **Module**: `src/StudyAI.Application`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Common/Interfaces/IApplicationDbContext.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Service Contract / Interface
- **Responsibility**: Abstract persistence gateway providing `IQueryable<T>` for application use cases.
- **Public Interfaces / Exports**: `IApplicationDbContext` interface.
- **Dependencies**: `StudyAI.Domain`.
- **Known Callers**: Application use cases, API controllers.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Keeps Application layer independent of Entity Framework Core implementation details.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Common/Interfaces/ICurrentDataSpaceAccessor.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Service Contract / Interface
- **Responsibility**: Mutable accessor contract for resolving and mutating the active tenant scope.
- **Public Interfaces / Exports**: `ICurrentDataSpaceAccessor` interface.
- **Dependencies**: `ICurrentDataSpaceContext`.
- **Known Callers**: Authentication middleware, background job runners, unit test fixtures.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Tenant ID must only be set after server-side authentication and authorization.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Common/Interfaces/ICurrentDataSpaceContext.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Service Contract / Interface
- **Responsibility**: Read-only tenant context interface providing `CurrentDataSpaceId`.
- **Public Interfaces / Exports**: `ICurrentDataSpaceContext` interface.
- **Dependencies**: None.
- **Known Callers**: `TenantSessionInterceptor`, domain services, query handlers.
- **Associated Tests**: `DatabaseSecurityAndIsolationTests.cs`.
- **Security / Architectural Notes**: Safe read-only view of active tenant scope.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Auth/IAuthService.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Service Contract / Interface
- **Responsibility**: Application interface for user registration, authentication, logout, and session queries.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Auth/Models/LoginCommand.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Command Model
- **Responsibility**: Application command carrying user credentials for authentication.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Auth/Models/LoginResult.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Result Model
- **Responsibility**: Authentication result containing success/failure status, lockout flags, and user summary.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Auth/Models/RegisterCommand.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Command Model
- **Responsibility**: Application command carrying user registration input.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Auth/Models/RegisterResult.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Result Model
- **Responsibility**: Explicit registration outcome distinguishing successful provisioning from duplicate email without exception/message inspection.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Application/Common/Interfaces/IAuthorizedDataSpaceScope.cs`
- **Module**: `src/StudyAI.Application`
- **Category**: Security Boundary Contract
- **Responsibility**: Defines the authorized Data Space transaction scope opened by Infrastructure and completed only after the API handler succeeds.
- **Public Interfaces / Exports**: `IAuthorizedDataSpaceScope`, `IAuthorizedDataSpaceScopeFactory`.
- **Security / Architectural Notes**: Keeps EF Core and transaction implementation details out of API endpoints and filters.
- **Verification Status**: `VERIFIED`
