using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class AuthenticationRateLimitIntegrationTests : IAsyncLifetime
{
    private StudyAIApiFactory _factory = null!;

    public async ValueTask InitializeAsync()
    {
        await PersistenceFixture.EnsureInitializedAsync(TestContext.Current.CancellationToken);
        _factory = new StudyAIApiFactory(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = PersistenceFixture.AppConnectionString,
            ["Authentication:RateLimits:Register:PermitLimit"] = "2",
            ["Authentication:RateLimits:Register:WindowSeconds"] = "60",
            ["Authentication:RateLimits:Login:PermitLimit"] = "2",
            ["Authentication:RateLimits:Login:WindowSeconds"] = "60"
        });
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task RegisterLimitReturnsStableProblemDetailsWithoutInvokingHandler()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateCookieClient();
        var emails = Enumerable.Range(0, 3)
            .Select(index => $"rate.register.{index}.{Guid.NewGuid():N}@studyai.test")
            .ToArray();

        var first = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(emails[0]), ct);
        var second = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(emails[1]), ct);
        var rejected = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(emails[2]), ct);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        await AssertRateLimitProblemAsync(rejected, ct);

        await using var context = PersistenceFixture.CreateAdminContext();
        Assert.False(await context.Users.AnyAsync(user => user.Email == emails[2], ct));
    }

    [Fact]
    public async Task RegisterAndLoginUseIndependentEndpointLimits()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateCookieClient();
        var email = $"rate.independent.{Guid.NewGuid():N}@studyai.test";

        var registration = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(email), ct);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var loginOne = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/login", Login(email), ct);
        var loginTwo = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/login", Login(email), ct);
        var loginRejected = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/login", Login(email), ct);

        Assert.Equal(HttpStatusCode.OK, loginOne.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginTwo.StatusCode);
        await AssertRateLimitProblemAsync(loginRejected, ct);
    }

    [Fact]
    public async Task UntrustedForwardedAddressesCannotEvadeDirectPeerLimit()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateCookieClient();

        var first = await PostLoginWithForwardedAddressAsync(client, "198.51.100.10", ct);
        var second = await PostLoginWithForwardedAddressAsync(client, "198.51.100.11", ct);
        var rejected = await PostLoginWithForwardedAddressAsync(client, "198.51.100.12", ct);

        Assert.Equal(HttpStatusCode.Unauthorized, first.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
        await AssertRateLimitProblemAsync(rejected, ct);
    }

    private HttpClient CreateCookieClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        HandleCookies = true,
        AllowAutoRedirect = false
    });

    private static RegisterRequest Registration(string email) => new()
    {
        Email = email,
        Password = "Password123!",
        DisplayName = "Rate Limit User"
    };

    private static LoginRequest Login(string email) => new()
    {
        Email = email,
        Password = "Password123!"
    };

    private static async Task<HttpResponseMessage> PostLoginWithForwardedAddressAsync(
        HttpClient client,
        string forwardedAddress,
        CancellationToken ct)
    {
        var token = await client.GetAntiforgeryTokenAsync(ct);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = JsonContent.Create(Login("missing-rate-limit-user@studyai.test"))
        };
        request.Headers.Add("X-XSRF-TOKEN", token);
        request.Headers.Add("X-Forwarded-For", forwardedAddress);
        return await client.SendAsync(request, ct);
    }

    private static async Task AssertRateLimitProblemAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status429TooManyRequests, problem.Status);
        Assert.Equal("Too Many Requests", problem.Title);
        Assert.Equal("auth_rate_limit_exceeded", problem.Extensions["code"]?.ToString());
    }
}
