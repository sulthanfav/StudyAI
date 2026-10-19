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

### `src/StudyAI.Application/packages.lock.json`
- **Module**: `src/StudyAI.Application`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution.
- **Verification Status**: `VERIFIED`
