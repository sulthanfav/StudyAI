using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Identity;
using StudyAI.Infrastructure.Identity;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class AuthenticationAndSessionIntegrationTests : IAsyncLifetime
{
    private StudyAIApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        await PersistenceFixture.EnsureInitializedAsync(TestContext.Current.CancellationToken);

        _factory = new StudyAIApiFactory(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = PersistenceFixture.AppConnectionString
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task SuccessfulLoginSetsHttpOnlySessionCookieAndEnablesMeEndpoint()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"login.{Guid.NewGuid():N}@studyai.test";
        var password = "Password123!";

        // 1. Register user
        var regResponse = await _client.PostWithAntiforgeryAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            DisplayName = "Login Test User"
        }, ct);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        // 2. Clear cookies by creating a fresh client
        using var sessionClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

        // 3. Login
        var loginResponse = await sessionClient.PostWithAntiforgeryAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = email,
            Password = password,
            RememberMe = true
        }, ct);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Verify Set-Cookie header exists and is HttpOnly
        Assert.True(loginResponse.Headers.Contains("Set-Cookie"));
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var sessionCookie = cookies.FirstOrDefault(c => c.Contains(".StudyAI.Session", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(sessionCookie);
        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthUserResponse>(ct);
        Assert.NotNull(loginPayload);
        Assert.Equal(email, loginPayload.Email);
        Assert.NotEqual(Guid.Empty, loginPayload.PersonalDataSpaceId);

        // 4. Access /api/v1/auth/me using authenticated cookie
        var meResponse = await sessionClient.GetAsync("/api/v1/auth/me", ct);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await meResponse.Content.ReadFromJsonAsync<AuthUserResponse>(ct);
        Assert.NotNull(mePayload);
        Assert.Equal(loginPayload.UserId, mePayload.UserId);
        Assert.Equal(email, mePayload.Email);
        Assert.Equal(loginPayload.PersonalDataSpaceId, mePayload.PersonalDataSpaceId);
    }

    [Fact]
    public async Task InvalidCredentialsReturns401WithGenericMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        var loginResponse = await _client.PostWithAntiforgeryAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = "nonexistent@studyai.test",
            Password = "WrongPassword123!"
        }, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

        var problem = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>(ct);
        Assert.NotNull(problem);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Contains("Invalid email or password", problem.Detail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AccountLockoutAfterConsecutiveFailedAttempts()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"lockout.{Guid.NewGuid():N}@studyai.test";
        var password = "Password123!";

        // Register
        var regResponse = await _client.PostWithAntiforgeryAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            DisplayName = "Lockout Candidate"
        }, ct);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        // 4 consecutive wrong password attempts return 401 Unauthorized
        for (var i = 0; i < 4; i++)
        {
            var failed = await _client.PostWithAntiforgeryAsync("/api/v1/auth/login", new LoginRequest
            {
                Email = email,
                Password = "WrongPassword999!"
            }, ct);
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        // 5th failed attempt reaches MaxFailedAccessAttempts threshold and triggers lockout (423 Locked)
        var triggerLockout = await _client.PostWithAntiforgeryAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongPassword999!"
        }, ct);
        Assert.Equal(HttpStatusCode.Locked, triggerLockout.StatusCode);

        // Subsequent attempt (even with correct password) remains locked out
        var lockedResponse = await _client.PostWithAntiforgeryAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = email,
            Password = password // even with correct password!
        }, ct);

        Assert.Equal(HttpStatusCode.Locked, lockedResponse.StatusCode);

        // Verify in DB that LockoutEnd is in the future
        await using var adminContext = PersistenceFixture.CreateAdminContext();
        var user = await adminContext.Users.FirstAsync(u => u.Email == email, ct);
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task StaleUserStoreUpdateReturnsControlledConcurrencyFailure()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"concurrency.update.{Guid.NewGuid():N}@studyai.test";
        var registration = await _client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register",
            new RegisterRequest
            {
                Email = email,
                Password = "Password123!",
                DisplayName = "Concurrency Update"
            },
            ct);
        var registeredUser = await registration.Content.ReadFromJsonAsync<RegisterResponse>(ct);
        Assert.NotNull(registeredUser);

        await using var firstContext = PersistenceFixture.CreateAppContext();
        await using var staleContext = PersistenceFixture.CreateAppContext();
        var firstUser = await firstContext.Users.SingleAsync(
            user => user.Id == registeredUser.UserId,
            ct);
        var staleUser = await staleContext.Users.SingleAsync(
            user => user.Id == registeredUser.UserId,
            ct);
        var firstStore = new StudyAIUserStore(firstContext, TimeProvider.System);
        var staleStore = new StudyAIUserStore(staleContext, TimeProvider.System);

        firstUser.DisplayName = "First Writer";
        Assert.True((await firstStore.UpdateAsync(firstUser, ct)).Succeeded);

        staleUser.DisplayName = "Stale Writer";
        var staleResult = await staleStore.UpdateAsync(staleUser, ct);

        Assert.False(staleResult.Succeeded);
        Assert.Contains(staleResult.Errors, error => error.Code == "ConcurrencyFailure");
    }

    [Fact]
    public async Task ConcurrentFailedLoginsNeverReturnUnhandledServerError()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"concurrency.login.{Guid.NewGuid():N}@studyai.test";
        var registration = await _client.PostWithAntiforgeryAsync(
            "/api/v1/auth/register",
            new RegisterRequest
            {
                Email = email,
                Password = "Password123!",
                DisplayName = "Concurrent Login"
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var clients = Enumerable.Range(0, 8)
            .Select(_ => _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false
            }))
            .ToArray();
        try
        {
            var attempts = clients.Select(client => client.PostWithAntiforgeryAsync(
                "/api/v1/auth/login",
                new LoginRequest
                {
                    Email = email,
                    Password = "WrongPassword999!"
                },
                ct));

            var responses = await Task.WhenAll(attempts);

            Assert.All(responses, response => Assert.Contains(
                response.StatusCode,
                new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Locked }));
        }
        finally
        {
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }
    }

    [Fact]
    public async Task LogoutClearsCookieAndInvalidatesSession()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"logout.{Guid.NewGuid():N}@studyai.test";
        var password = "Password123!";

        // Register
        await _client.PostWithAntiforgeryAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            DisplayName = "Logout User"
        }, ct);

        using var sessionClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

        // Login
        var loginResponse = await sessionClient.PostWithAntiforgeryAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        }, ct);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Verify authenticated access works
        var meBefore = await sessionClient.GetAsync("/api/v1/auth/me", ct);
        Assert.Equal(HttpStatusCode.OK, meBefore.StatusCode);

        // Logout
        var logoutResponse = await sessionClient.PostWithAntiforgeryAsync<object?>(
            "/api/v1/auth/logout", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        // Verify subsequent request with this client fails
        var meAfter = await sessionClient.GetAsync("/api/v1/auth/me", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, meAfter.StatusCode);
    }

    [Fact]
    public async Task UnauthorizedAccessWithoutCookieReturns401()
    {
        var ct = TestContext.Current.CancellationToken;
        using var cleanClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });

        var response = await cleanClient.GetAsync("/api/v1/auth/me", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
