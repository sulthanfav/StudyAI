using System.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using StudyAI.Api.Configuration;

namespace StudyAI.Api.Diagnostics;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private static readonly Action<ILogger, string, string, int, double, string, Exception?> LogRequestCompleted =
        LoggerMessage.Define<string, string, int, double, string>(
            LogLevel.Information,
            new EventId(1001, nameof(LogRequestCompleted)),
            "HTTP {Method} {RouteTemplate} responded {StatusCode} in {ElapsedMilliseconds} ms with correlation {CorrelationId}");

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<FoundationOptions> options,
        ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = CorrelationIdPolicy.Resolve(
            context.Request.Headers[CorrelationIdPolicy.HeaderName].ToString(),
            options.Value.MaxCorrelationIdLength);
        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationIdPolicy.HeaderName] = correlationId;

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId
        });
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
        }
        finally
        {
            var routeTemplate = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "(unmatched)";
            LogRequestCompleted(
                logger,
                context.Request.Method,
                routeTemplate,
                context.Response.StatusCode,
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                correlationId,
                null);
        }
    }
}
