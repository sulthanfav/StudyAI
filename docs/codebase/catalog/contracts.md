# File Catalog: Contracts Module (`src/StudyAI.Contracts`)

Owning Module: `src/StudyAI.Contracts`  
Module Documentation: [`../MODULE_INDEX.md`](../MODULE_INDEX.md)  
Status: **VERIFIED**

---

## Catalog Entries

### `src/StudyAI.Contracts/StudyAI.Contracts.csproj`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Project build configuration for shared DTOs and contracts targeting `net10.0`.
- **Public Interfaces / Exports**: MSBuild project.
- **Dependencies**: None (zero internal/external dependencies).
- **Known Callers**: `StudyAI.Api`, `StudyAI.Application`, `StudyAI.Infrastructure`.
- **Associated Tests**: `ProjectReferenceRulesTests.cs`.
- **Security / Architectural Notes**: Must not reference any implementation or persistence assemblies.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/packages.lock.json`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/Auth/AuthUserResponse.cs`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Data Transfer Object (DTO)
- **Responsibility**: Authenticated user summary contract for session introspection.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/Auth/CsrfTokenResponse.cs`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Data Transfer Object (DTO)
- **Responsibility**: Antiforgery token transfer contract for SPA cookie protections.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/Auth/DataSpaceSummaryResponse.cs`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Data Transfer Object (DTO)
- **Responsibility**: Summary response representing accessible Data Space details.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/Auth/LoginRequest.cs`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Data Transfer Object (DTO)
- **Responsibility**: Credentials payload contract for user login.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/Auth/RegisterRequest.cs`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Data Transfer Object (DTO)
- **Responsibility**: Registration payload contract for new account creation.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/Auth/RegisterResponse.cs`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Data Transfer Object (DTO)
- **Responsibility**: Provisioning response returning user and default data space IDs.
- **Verification Status**: `VERIFIED`

### `src/StudyAI.Contracts/Workspace/ContentSpaceResponse.cs`
- **Module**: `src/StudyAI.Contracts`
- **Category**: Data Transfer Object (DTO)
- **Responsibility**: Representation of a content space within a Data Space.
- **Verification Status**: `VERIFIED`
