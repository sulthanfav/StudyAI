using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using StudyAI.Api.Composition;
using StudyAI.Api.Diagnostics;
using StudyAI.Api.Endpoints;
using StudyAI.Api.Health;

using StudyAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = false;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});
builder.Services.AddStudyAIFoundation(builder.Configuration);
builder.Services.AddInfrastructurePersistence(builder.Configuration);
builder.Services.AddStudyAIAuth(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    var statusCode = httpContext.Response.StatusCode;
    var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
    await problemDetailsService.WriteAsync(new ProblemDetailsContext
    {
        HttpContext = httpContext,
        ProblemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Type = $"https://www.rfc-editor.org/rfc/rfc9110#status.{statusCode}"
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health/live", FoundationHealthEndpoints.GetLivenessAsync)
    .WithName("GetLiveness")
    .WithSummary("Check whether the API process is responsive")
    .WithDescription("Returns process-only liveness without probing external dependencies.")
    .Produces(StatusCodes.Status200OK, contentType: "application/json");
app.MapGet("/health/ready", FoundationHealthEndpoints.GetReadinessAsync)
    .WithName("GetReadiness")
    .WithSummary("Check whether registered foundation components are ready")
    .WithDescription("Returns safe component names and states without configuration values or exception details.")
    .Produces(StatusCodes.Status200OK, contentType: "application/json")
    .Produces(StatusCodes.Status503ServiceUnavailable, contentType: "application/json");

app.MapAuthEndpoints();
app.MapDataSpaceEndpoints();

app.Run();

public partial class Program;
