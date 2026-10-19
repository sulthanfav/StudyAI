using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StudyAI.Api.Health;

public static class FoundationHealthEndpoints
{
    public static Task GetLivenessAsync(
        HttpContext context,
        HealthCheckService healthCheckService,
        CancellationToken cancellationToken) =>
        RunAsync(context, healthCheckService, _ => false, cancellationToken);

    public static Task GetReadinessAsync(
        HttpContext context,
        HealthCheckService healthCheckService,
        CancellationToken cancellationToken) =>
        RunAsync(
            context,
            healthCheckService,
            registration => registration.Tags.Contains("ready"),
            cancellationToken);

    private static async Task RunAsync(
        HttpContext context,
        HealthCheckService healthCheckService,
        Func<HealthCheckRegistration, bool> predicate,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(predicate, cancellationToken);
        context.Response.StatusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;
        await SafeHealthResponseWriter.WriteAsync(context, report);
    }
}
