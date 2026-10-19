using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StudyAI.Api.IntegrationTests.Support;
using Xunit;

namespace StudyAI.Api.IntegrationTests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task LivenessSucceedsWithoutExternalDependencyConfiguration()
    {
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("StudyAI.Api", payload.GetProperty("service").GetString());
        Assert.Equal("healthy", payload.GetProperty("status").GetString());
        Assert.Empty(payload.GetProperty("components").EnumerateArray());
    }

    [Fact]
    public async Task ReadinessReturnsOnlySafeComponentStatusWhenHealthy()
    {
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"name\":\"foundation\"", body);
        Assert.DoesNotContain("exception", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadinessReturnsServiceUnavailableWithoutLeakingFailureDetails()
    {
        using var factory = new StudyAIApiFactory(includeUnhealthyReadinessCheck: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("\"name\":\"forced-unhealthy\"", body);
        Assert.DoesNotContain("health-canary", body);
        Assert.DoesNotContain("Server=private", body);
    }
}
