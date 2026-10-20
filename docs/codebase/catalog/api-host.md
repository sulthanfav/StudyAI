# File Catalog: API Host Module (`apps/api/StudyAI.Api`)

Owning Module: `apps/api/StudyAI.Api`  
Module Documentation: [`../MODULE_INDEX.md`](../MODULE_INDEX.md)  
Status: **VERIFIED**

---

## Catalog Entries

### `apps/api/StudyAI.Api/Security/AntiforgeryValidationFilter.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: HTTP Security Filter
- **Responsibility**: Validates antiforgery tokens on state-changing auth endpoints.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Program.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (Host Entry Point)
- **Responsibility**: WebApplication builder configuration, structured logging setup, middleware registration, health route mapping, and application lifecycle startup.
- **Public Interfaces / Exports**: Host process entry point.
- **Dependencies**: `FoundationServiceCollectionExtensions`, `CorrelationIdMiddleware`, `FoundationHealthEndpoints`.
- **Known Callers**: Runtime host runtime execution (`dotnet run`).
- **Associated Tests**: `tests/StudyAI.Api.IntegrationTests/*`.
- **Security / Architectural Notes**: Excludes unauthenticated business endpoints; registers sanitized console logging.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Composition/FoundationServiceCollectionExtensions.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (Service Registration)
- **Responsibility**: Binds and validates typed `FoundationOptions`, adds health checks, and registers foundation infrastructure services.
- **Public Interfaces / Exports**: `AddFoundationHostServices(this IServiceCollection services, IConfiguration configuration)`.
- **Dependencies**: `Microsoft.Extensions.DependencyInjection`, `FoundationOptions`, `FoundationOptionsValidator`.
- **Known Callers**: `Program.cs`.
- **Associated Tests**: `ConfigurationStartupTests.cs`.
- **Security / Architectural Notes**: Enforces eager validation at startup (`ValidateOnStart`).
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Configuration/FoundationOptions.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (Options Model)
- **Responsibility**: Strongly-typed options class for foundation host configuration (ApplicationName, EnvironmentName, EnableDetailedErrors).
- **Public Interfaces / Exports**: `FoundationOptions` class.
- **Dependencies**: None.
- **Known Callers**: `FoundationServiceCollectionExtensions`, `FoundationOptionsValidator`.
- **Associated Tests**: `FoundationOptionsValidatorTests.cs`.
- **Security / Architectural Notes**: Never stores credentials or secrets.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Configuration/FoundationOptionsValidator.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (Validation)
- **Responsibility**: Implements `IValidateOptions<FoundationOptions>` to validate configuration values without leaking secret values in error messages.
- **Public Interfaces / Exports**: `Validate(string? name, FoundationOptions options)`.
- **Dependencies**: `Microsoft.Extensions.Options`.
- **Known Callers**: ASP.NET Core Options framework.
- **Associated Tests**: `FoundationOptionsValidatorTests.cs`, `ConfigurationStartupTests.cs`.
- **Security / Architectural Notes**: Error messages report missing key names, never raw or malformed values.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Diagnostics/CorrelationIdMiddleware.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (HTTP Middleware)
- **Responsibility**: Extracts or generates a valid `X-Correlation-ID`, attaches it to response headers and logging scope.
- **Public Interfaces / Exports**: `InvokeAsync(HttpContext context)`.
- **Dependencies**: `CorrelationIdPolicy`, `ILogger`.
- **Known Callers**: ASP.NET Core request pipeline (`Program.cs`).
- **Associated Tests**: `CorrelationAndLoggingTests.cs`.
- **Security / Architectural Notes**: Mitigates log-injection attacks by validating identifier format and size.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Diagnostics/CorrelationIdPolicy.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (Policy Utility)
- **Responsibility**: Sanitizes caller-supplied correlation identifiers: enforces max length (64 chars), strips control characters, or generates a secure GUID fallback.
- **Public Interfaces / Exports**: `SanitizeOrGenerate(string? candidate)`.
- **Dependencies**: System primitives.
- **Known Callers**: `CorrelationIdMiddleware`.
- **Associated Tests**: `CorrelationIdPolicyTests.cs`.
- **Security / Architectural Notes**: Prevents carriage-return log injection and header tampering.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Health/FoundationHealthEndpoints.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (Routing Endpoints)
- **Responsibility**: Maps process liveness (`/health/live`) and dependency readiness (`/health/ready`) with customized options and tag filters.
- **Public Interfaces / Exports**: `MapFoundationHealthEndpoints(this IEndpointRouteBuilder endpoints)`.
- **Dependencies**: `SafeHealthResponseWriter`, ASP.NET Core HealthChecks.
- **Known Callers**: `Program.cs`.
- **Associated Tests**: `HealthEndpointTests.cs`.
- **Security / Architectural Notes**: Liveness is strictly process-only (no DB/queue checks).
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Health/SafeHealthResponseWriter.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Source (HTTP Response Formatter)
- **Responsibility**: Serializes health reports into clean JSON without exposing stack traces, connection strings, or system environment internals.
- **Public Interfaces / Exports**: `WriteResponse(HttpContext context, HealthReport report)`.
- **Dependencies**: `System.Text.Json`, `Microsoft.Extensions.Diagnostics.HealthChecks`.
- **Known Callers**: `FoundationHealthEndpoints`.
- **Associated Tests**: `HealthEndpointTests.cs`.
- **Security / Architectural Notes**: Sanitizes health responses to prevent information leakage.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/StudyAI.Api.csproj`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Project build configuration targeting `net10.0`, references `StudyAI.Contracts`, `StudyAI.Application`, `StudyAI.Infrastructure`.
- **Public Interfaces / Exports**: MSBuild project.
- **Dependencies**: Core projects.
- **Known Callers**: Solution build (`StudyAI.slnx`).
- **Associated Tests**: `ProjectReferenceRulesTests.cs`.
- **Security / Architectural Notes**: Central Package Management enabled.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/appsettings.json`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Configuration
- **Responsibility**: Base runtime environment settings for FoundationOptions and Logging.
- **Public Interfaces / Exports**: JSON configuration.
- **Dependencies**: None.
- **Known Callers**: ASP.NET Core Configuration Provider.
- **Associated Tests**: `ConfigurationStartupTests.cs`.
- **Security / Architectural Notes**: Contains placeholders only; zero secrets.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/appsettings.Development.json`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Configuration
- **Responsibility**: Development environment configuration overrides for local debugging.
- **Public Interfaces / Exports**: JSON configuration.
- **Dependencies**: None.
- **Known Callers**: ASP.NET Core Configuration Provider.
- **Associated Tests**: `ConfigurationStartupTests.cs`.
- **Security / Architectural Notes**: Contains placeholders only; zero secrets.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Properties/launchSettings.json`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Configuration (Debug Profiles)
- **Responsibility**: Local developer launch profile defining host endpoint (`http://localhost:5080`).
- **Public Interfaces / Exports**: Launch settings profile.
- **Dependencies**: None.
- **Known Callers**: .NET CLI and IDE debug runners.
- **Associated Tests**: `HealthEndpointTests.cs`.
- **Security / Architectural Notes**: Local loopback binding only.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/packages.lock.json`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution for the API host.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/StudyAI.Api.http`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Tooling (HTTP Scratchpad)
- **Responsibility**: Test requests for manual verification of `/health/live` and `/health/ready`.
- **Public Interfaces / Exports**: HTTP request script.
- **Dependencies**: None.
- **Known Callers**: Developer IDE REST client.
- **Associated Tests**: `HealthEndpointTests.cs`.
- **Security / Architectural Notes**: Safe endpoints only.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Composition/AuthServiceCollectionExtensions.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: DI Composition / Configuration
- **Responsibility**: Configures ASP.NET Core Identity Core, cookie options, antiforgery, and authorization policies.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Endpoints/AuthEndpoints.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: API Endpoints / Minimal API
- **Responsibility**: Maps `/api/v1/auth/register`, `/login`, `/logout`, `/me`, and `/csrf` endpoints.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Endpoints/DataSpaceEndpoints.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: API Endpoints / Minimal API
- **Responsibility**: Maps `/api/v1/data-spaces/{id}/content-spaces` with server-side authorization filter.
- **Verification Status**: `VERIFIED`

### `apps/api/StudyAI.Api/Security/DataSpaceAuthorizationFilter.cs`
- **Module**: `apps/api/StudyAI.Api`
- **Category**: Security / Endpoint Filter
- **Responsibility**: Validates authenticated actor membership in target Data Space and sets transaction-local RLS context.
- **Verification Status**: `VERIFIED`
