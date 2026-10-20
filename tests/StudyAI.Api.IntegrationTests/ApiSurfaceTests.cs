using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudyAI.Api.IntegrationTests.Support;
using Xunit;

namespace StudyAI.Api.IntegrationTests;

public sealed class ApiSurfaceTests
{
    [Fact]
    public void FoundationExposesOnlyOperabilityAndDevelopmentMetadataRoutes()
    {
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();
        var routes = factory.Services
            .GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Where(route => route is not null)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                "/api/v1/auth/csrf",
                "/api/v1/auth/login",
                "/api/v1/auth/logout",
                "/api/v1/auth/me",
                "/api/v1/auth/register",
                "/api/v1/data-spaces/{id:guid}/content-spaces",
                "/health/live",
                "/health/ready",
                "/openapi/{documentName}.json"
            ],
            routes);
    }

    [Fact]
    public async Task UnknownRouteReturnsSafeProblemDetails()
    {
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/not-implemented", TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Not Found", problem["title"]?.ToString());
        Assert.DoesNotContain("exception", problem.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DevelopmentOpenApiDocumentsBothHealthEndpoints()
    {
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var document = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var paths = document.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/health/live", out _));
        Assert.True(paths.TryGetProperty("/health/ready", out _));
    }
}
