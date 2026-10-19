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
