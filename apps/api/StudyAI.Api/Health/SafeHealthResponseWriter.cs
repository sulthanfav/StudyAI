using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StudyAI.Api.Configuration;

namespace StudyAI.Api.Health;

public static class SafeHealthResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var foundation = context.RequestServices.GetRequiredService<IOptions<FoundationOptions>>().Value;
        var components = report.Entries
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new HealthComponentResponse(entry.Key, ToWireValue(entry.Value.Status)))
            .ToArray();
        var response = new HealthResponse(
            foundation.ServiceName,
            ToWireValue(report.Status),
            components);

        context.Response.ContentType = "application/json; charset=utf-8";
        return JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            SerializerOptions,
            context.RequestAborted);
    }

    private static string ToWireValue(HealthStatus status) => status.ToString().ToLowerInvariant();

    private sealed record HealthResponse(
        string Service,
        string Status,
        IReadOnlyList<HealthComponentResponse> Components);

    private sealed record HealthComponentResponse(string Name, string Status);
}
