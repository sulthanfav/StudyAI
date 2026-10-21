using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class AntiforgeryIntegrationTests : IAsyncLifetime
{
    private StudyAIApiFactory _factory = null!;

    public async ValueTask InitializeAsync()
    {
        await PersistenceFixture.EnsureInitializedAsync(TestContext.Current.CancellationToken);
        _factory = new StudyAIApiFactory(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = PersistenceFixture.AppConnectionString
        });
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task RegisterWithoutAntiforgeryTokenIsRejectedWithoutCreatingUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"csrf.missing.register.{Guid.NewGuid():N}@studyai.test";
        using var client = CreateCookieClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", Registration(email), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertUserDoesNotExistAsync(email, ct);
    }

    [Fact]
    public async Task RegisterWithInvalidAntiforgeryTokenIsRejectedWithoutCreatingUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"csrf.invalid.register.{Guid.NewGuid():N}@studyai.test";
        using var client = CreateCookieClient();

        var response = await client.PostWithAntiforgeryTokenAsync(
            "/api/v1/auth/register", Registration(email), "invalid-token", ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertUserDoesNotExistAsync(email, ct);
    }

    [Fact]
    public async Task RegisterWithValidAntiforgeryTokenIsAccepted()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"csrf.valid.register.{Guid.NewGuid():N}@studyai.test";
        using var client = CreateCookieClient();

        var response = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(email), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task LoginWithoutAntiforgeryTokenIsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await CreateRegisteredClientAsync(ct);
        using (client)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/login", Login(email), ct);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task LoginWithInvalidAntiforgeryTokenIsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await CreateRegisteredClientAsync(ct);
        using (client)
        {
            var response = await client.PostWithAntiforgeryTokenAsync(
                "/api/v1/auth/login", Login(email), "invalid-token", ct);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task LoginWithValidAntiforgeryTokenIsAccepted()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await CreateRegisteredClientAsync(ct);
        using (client)
        {
            var response = await client.PostWithAntiforgeryAsync(
                "/api/v1/auth/login", Login(email), ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task LogoutWithoutAntiforgeryTokenIsRejectedAndSessionRemainsValid()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateAuthenticatedClientAsync(ct);
        using (client)
        {
            var response = await client.PostAsync("/api/v1/auth/logout", null, ct);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var me = await client.GetAsync("/api/v1/auth/me", ct);
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        }
    }

    [Fact]
    public async Task LogoutWithInvalidAntiforgeryTokenIsRejectedAndSessionRemainsValid()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateAuthenticatedClientAsync(ct);
        using (client)
        {
            var response = await client.PostWithAntiforgeryTokenAsync<object?>(
                "/api/v1/auth/logout", null, "invalid-token", ct);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var me = await client.GetAsync("/api/v1/auth/me", ct);
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        }
    }

    [Fact]
    public async Task LogoutWithValidAntiforgeryTokenIsAccepted()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateAuthenticatedClientAsync(ct);
        using (client)
        {
            var response = await client.PostWithAntiforgeryAsync<object?>(
                "/api/v1/auth/logout", null, ct);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    private HttpClient CreateCookieClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        HandleCookies = true,
        AllowAutoRedirect = false
    });

    private async Task<(HttpClient Client, string Email)> CreateRegisteredClientAsync(CancellationToken ct)
    {
        var client = CreateCookieClient();
        var email = $"csrf.user.{Guid.NewGuid():N}@studyai.test";
        var response = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(email), ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (client, email);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(CancellationToken ct)
    {
        var (client, email) = await CreateRegisteredClientAsync(ct);
        var response = await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/login", Login(email), ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return client;
    }

    private static RegisterRequest Registration(string email) => new()
    {
        Email = email,
        Password = "Password123!",
        DisplayName = "CSRF Test User"
    };

    private static LoginRequest Login(string email) => new()
    {
        Email = email,
        Password = "Password123!"
    };

    private static async Task AssertUserDoesNotExistAsync(string email, CancellationToken ct)
    {
        await using var context = PersistenceFixture.CreateAdminContext();
        Assert.False(await context.Users.AnyAsync(user => user.Email == email, ct));
    }
}
