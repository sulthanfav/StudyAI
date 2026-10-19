# StudyAI Feature Map

This document connects features to their concrete code implementation, tests, and documentation.

Navigation: [`SYSTEM_MAP.md`](../../SYSTEM_MAP.md) | [`MODULE_INDEX.md`](MODULE_INDEX.md) | [`FILE_INDEX.md`](FILE_INDEX.md)

---

## 1. Implemented Foundation Features

| Feature ID | Feature Name | Owning Module | Entry Points & Core Files | Associated Tests | Status |
|---|---|---|---|---|---|
| **INF-01** | **Host Health Contract** | `apps/api/StudyAI.Api` | [`Health/FoundationHealthEndpoints.cs`](../../apps/api/StudyAI.Api/Health/FoundationHealthEndpoints.cs), [`Health/SafeHealthResponseWriter.cs`](../../apps/api/StudyAI.Api/Health/SafeHealthResponseWriter.cs) | [`HealthEndpointTests.cs`](../../tests/StudyAI.Api.IntegrationTests/HealthEndpointTests.cs) | **VERIFIED** |
| **INF-02** | **Correlation & Structured Logging** | `apps/api/StudyAI.Api` | [`Diagnostics/CorrelationIdMiddleware.cs`](../../apps/api/StudyAI.Api/Diagnostics/CorrelationIdMiddleware.cs), [`Diagnostics/CorrelationIdPolicy.cs`](../../apps/api/StudyAI.Api/Diagnostics/CorrelationIdPolicy.cs) | [`CorrelationAndLoggingTests.cs`](../../tests/StudyAI.Api.IntegrationTests/CorrelationAndLoggingTests.cs), [`CorrelationIdPolicyTests.cs`](../../tests/StudyAI.UnitTests/Diagnostics/CorrelationIdPolicyTests.cs) | **VERIFIED** |
| **INF-03** | **Typed Foundation Options Validation** | `apps/api/StudyAI.Api` | [`Configuration/FoundationOptions.cs`](../../apps/api/StudyAI.Api/Configuration/FoundationOptions.cs), [`Configuration/FoundationOptionsValidator.cs`](../../apps/api/StudyAI.Api/Configuration/FoundationOptionsValidator.cs) | [`ConfigurationStartupTests.cs`](../../tests/StudyAI.Api.IntegrationTests/ConfigurationStartupTests.cs), [`FoundationOptionsValidatorTests.cs`](../../tests/StudyAI.UnitTests/Configuration/FoundationOptionsValidatorTests.cs) | **VERIFIED** |
| **INF-04** | **Clean Architecture Boundaries** | `tests/StudyAI.ArchitectureTests` | [`ProjectReferenceRulesTests.cs`](../../tests/StudyAI.ArchitectureTests/ProjectReferenceRulesTests.cs) | Architecture test suite (zero circular/forbidden references) | **VERIFIED** |
| **INF-05** | **Safe API Surface Gate** | `tests/StudyAI.Api.IntegrationTests` | [`ApiSurfaceTests.cs`](../../tests/StudyAI.Api.IntegrationTests/ApiSurfaceTests.cs) | Proves no unauthorized endpoints exposed prior to security review | **VERIFIED** |

---

## 2. Planned V1 Business Modules & Features (Target PRD Scope)

The following features represent the public V1 scope specified in [`docs/product/StudyAI_Campus_MVP_Final_PRD_v1.0.md`](../product/StudyAI_Campus_MVP_Final_PRD_v1.0.md). They are planned for upcoming iterations:

| Module / Feature ID | Module & Feature Name | Planned Owning Modules | Relevant Specification | Current Status |
|---|---|---|---|---|
| **MOD-01** | **Academic Library & Document Ingestion** | `StudyAI.Domain`, `StudyAI.Infrastructure`, `workers/document-processing` | PRD §3.1, Architecture §4 | **PLANNED** |
| **MOD-02** | **Course Workspace** | `StudyAI.Domain`, `StudyAI.Application`, `apps/web` | PRD §3.2, Architecture §4 | **PLANNED** |
| **MOD-03** | **Research & Project Workspace** | `StudyAI.Domain`, `StudyAI.Application`, `apps/web` | PRD §3.3, Architecture §4 | **PLANNED** |
| **MOD-04** | **Note-Taking & Recoverable Concurrency** | `StudyAI.Domain`, `StudyAI.Application`, `apps/web` | PRD §3.4, Architecture §6 | **PLANNED** |
| **MOD-05** | **Spaced Repetition Flashcards** | `StudyAI.Domain`, `StudyAI.Application` | PRD §3.5, Architecture §4 | **PLANNED** |
| **MOD-06** | **Quiz & Assessment Generator** | `StudyAI.Domain`, `StudyAI.Application` | PRD §3.6, Architecture §4 | **PLANNED** |
| **MOD-07** | **Academic Chat & Provenance Retrieval** | `StudyAI.Infrastructure`, `StudyAI.Application` | PRD §3.7, Architecture §7 | **PLANNED** |
| **MOD-08** | **AI Quota Ledger & Metering** | `StudyAI.Domain`, `StudyAI.Infrastructure` | PRD §3.8, Architecture §8 | **PLANNED** |
| **MOD-09** | **Identity & Data Space Isolation** | `StudyAI.Domain`, `StudyAI.Infrastructure` | PRD §3.9, Architecture §3 | **PLANNED** |

---

## 3. Maintenance Policy

- Whenever an implemented feature is added or modified, update Section 1 with the exact entry point and test files.
- Never mark a planned feature as VERIFIED without real implementation code and passing automated tests.
