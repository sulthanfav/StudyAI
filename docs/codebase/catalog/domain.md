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
