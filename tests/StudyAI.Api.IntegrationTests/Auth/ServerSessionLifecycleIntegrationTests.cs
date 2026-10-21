using System.Net;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Identity;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class ServerSessionLifecycleIntegrationTests : IAsyncLifetime
{
    private const string Password = "Password123!";
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
    public async Task ExactPreLogoutCookieReplayIsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("replay", ct);
        using var device = CreateCookieClient();
        var login = await LoginAsync(device, email, ct);
        var capturedSessionCookie = CaptureSessionCookie(login);

        var logout = await device.PostWithAntiforgeryAsync<object?>(
            "/api/v1/auth/logout", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        using var replayClient = CreateCookieClient(handleCookies: false);
        replayClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", capturedSessionCookie);
        var replay = await replayClient.GetAsync("/api/v1/auth/me", ct);

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
    }

    [Fact]
    public async Task SessionIsRejectedAfterAbsoluteLifetimeExpires()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var expiryFactory = CreateSessionLifetimeFactory(
            idleSeconds: 60,
            standardAbsoluteSeconds: 60,
            rememberMeAbsoluteSeconds: 1);
        var email = await RegisterUserAsync("absolute", ct, expiryFactory);
        using var device = CreateCookieClient(expiryFactory);
        var login = await LoginAsync(device, email, ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        await Task.Delay(TimeSpan.FromMilliseconds(1200), ct);
        var afterAbsoluteExpiry = await device.GetAsync("/api/v1/auth/me", ct);

        Assert.Equal(HttpStatusCode.Unauthorized, afterAbsoluteExpiry.StatusCode);
    }

    [Fact]
    public async Task StandardSessionIsRejectedAfterItsAbsoluteLifetimeExpires()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var expiryFactory = CreateSessionLifetimeFactory(
            idleSeconds: 60,
            standardAbsoluteSeconds: 1,
            rememberMeAbsoluteSeconds: 60);
        var email = await RegisterUserAsync("standard-absolute", ct, expiryFactory);
        using var device = CreateCookieClient(expiryFactory);
        var login = await LoginAsync(device, email, ct, rememberMe: false);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        await Task.Delay(TimeSpan.FromMilliseconds(1200), ct);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await device.GetAsync("/api/v1/auth/me", ct)).StatusCode);
    }

    [Fact]
    public async Task SessionIsRejectedAfterIdleLifetimeExpires()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var idleFactory = CreateSessionLifetimeFactory(
            idleSeconds: 1,
            standardAbsoluteSeconds: 60,
            rememberMeAbsoluteSeconds: 60);
        var email = await RegisterUserAsync("idle", ct, idleFactory);
        using var device = CreateCookieClient(idleFactory);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(device, email, ct)).StatusCode);

        await Task.Delay(TimeSpan.FromMilliseconds(1200), ct);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await device.GetAsync("/api/v1/auth/me", ct)).StatusCode);
    }

    [Fact]
    public async Task OrdinaryLogoutDoesNotRevokeAnotherDeviceSession()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("two-device", ct);
        using var deviceA = CreateCookieClient();
        using var deviceB = CreateCookieClient();
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceA, email, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceB, email, ct)).StatusCode);

        var logout = await deviceA.PostWithAntiforgeryAsync<object?>(
            "/api/v1/auth/logout", null, ct);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var deviceAAfterLogout = await deviceA.GetAsync("/api/v1/auth/me", ct);
        var deviceBAfterLogout = await deviceB.GetAsync("/api/v1/auth/me", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, deviceAAfterLogout.StatusCode);
        Assert.Equal(HttpStatusCode.OK, deviceBAfterLogout.StatusCode);

        await using var context = PersistenceFixture.CreateAdminContext();
        var userId = await context.Users
            .Where(user => user.Email == email)
            .Select(user => user.Id)
            .SingleAsync(ct);
        var sessions = await context.UserSessions
            .Where(session => session.UserId == userId)
            .ToListAsync(ct);
        Assert.Equal(2, sessions.Count);
        Assert.Single(sessions, session => session.RevokedAt is not null);
        Assert.Single(sessions, session => session.RevokedAt is null);
    }

    [Theory]
    [InlineData("suspended", false)]
    [InlineData("active", true)]
    public async Task ExistingSessionIsRejectedWhenAccountBecomesIneligible(
        string status,
        bool softDelete)
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("ineligible-session", ct);
        using var device = CreateCookieClient();
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(device, email, ct)).StatusCode);

        await using (var context = PersistenceFixture.CreateAdminContext())
        {
            var user = await context.Users.SingleAsync(candidate => candidate.Email == email, ct);
            user.Status = status;
            user.DeletedAt = softDelete ? DateTimeOffset.UtcNow : null;
            await context.SaveChangesAsync(ct);
        }

        var response = await device.GetAsync("/api/v1/auth/me", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("suspended", false)]
    [InlineData("active", true)]
    public async Task IneligibleAccountCannotCreateNewSession(string status, bool softDelete)
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("ineligible-login", ct);
        await using (var context = PersistenceFixture.CreateAdminContext())
        {
            var user = await context.Users.SingleAsync(candidate => candidate.Email == email, ct);
            user.Status = status;
            user.DeletedAt = softDelete ? DateTimeOffset.UtcNow : null;
            await context.SaveChangesAsync(ct);
        }

        using var device = CreateCookieClient();
        var response = await LoginAsync(device, email, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain(
            response.Headers.TryGetValues("Set-Cookie", out var values) ? values : [],
            value => value.StartsWith(".StudyAI.Session=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExistingSessionIsRejectedAfterSecurityStampChanges()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("security-stamp", ct);
        using var deviceA = CreateCookieClient();
        using var deviceB = CreateCookieClient();
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceA, email, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceB, email, ct)).StatusCode);
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);
            var result = await userManager.UpdateSecurityStampAsync(user);
            Assert.True(result.Succeeded);
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceA.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceB.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        await AssertAllSessionsRevokedAsync(email, ct);
    }

    [Fact]
    public async Task AccountSoftDeletionRevokesAllDeviceSessions()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("delete-all", ct);
        using var deviceA = CreateCookieClient();
        using var deviceB = CreateCookieClient();
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceA, email, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceB, email, ct)).StatusCode);
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);
            user.DeletedAt = DateTimeOffset.UtcNow;
            var result = await userManager.UpdateAsync(user);
            Assert.True(result.Succeeded);
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceA.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceB.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        await AssertAllSessionsRevokedAsync(email, ct);
    }

    [Fact]
    public async Task PasswordChangeRevokesAllDeviceSessionRowsAndCookies()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("password-change", ct);
        using var deviceA = CreateCookieClient();
        using var deviceB = CreateCookieClient();
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceA, email, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceB, email, ct)).StatusCode);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);
            var result = await userManager.ChangePasswordAsync(user, Password, "NewPassword456!");
            Assert.True(result.Succeeded);
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceA.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceB.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        await AssertAllSessionsRevokedAsync(email, ct);
    }

    [Fact]
    public async Task AccountSuspensionRevokesAllDeviceSessionRowsAndCookies()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await RegisterUserAsync("suspend-all", ct);
        using var deviceA = CreateCookieClient();
        using var deviceB = CreateCookieClient();
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceA, email, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(deviceB, email, ct)).StatusCode);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);
            user.Status = "suspended";
            var result = await userManager.UpdateAsync(user);
            Assert.True(result.Succeeded);
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceA.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await deviceB.GetAsync("/api/v1/auth/me", ct)).StatusCode);
        await AssertAllSessionsRevokedAsync(email, ct);
    }

    private async Task<string> RegisterUserAsync(
        string prefix,
        CancellationToken ct,
        StudyAIApiFactory? factory = null)
    {
        var email = $"session.{prefix}.{Guid.NewGuid():N}@studyai.test";
        using var registrationClient = CreateCookieClient(factory ?? _factory);
        var response = await registrationClient.PostWithAntiforgeryAsync(
            "/api/v1/auth/register",
            new RegisterRequest
            {
                Email = email,
                Password = Password,
                DisplayName = "Session Lifecycle User"
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return email;
    }

    private static async Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        CancellationToken ct,
        bool rememberMe = true) =>
        await client.PostWithAntiforgeryAsync(
            "/api/v1/auth/login",
            new LoginRequest
            {
                Email = email,
                Password = Password,
                RememberMe = rememberMe
            },
            ct);

    private HttpClient CreateCookieClient(bool handleCookies = true) =>
        CreateCookieClient(_factory, handleCookies);

    private static StudyAIApiFactory CreateSessionLifetimeFactory(
        int idleSeconds,
        int standardAbsoluteSeconds,
        int rememberMeAbsoluteSeconds) =>
        new(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = PersistenceFixture.AppConnectionString,
            ["Authentication:Sessions:IdleLifetimeSeconds"] = idleSeconds.ToString(CultureInfo.InvariantCulture),
            ["Authentication:Sessions:StandardAbsoluteLifetimeSeconds"] = standardAbsoluteSeconds.ToString(CultureInfo.InvariantCulture),
            ["Authentication:Sessions:RememberMeAbsoluteLifetimeSeconds"] = rememberMeAbsoluteSeconds.ToString(CultureInfo.InvariantCulture)
        });

    private static HttpClient CreateCookieClient(
        StudyAIApiFactory factory,
        bool handleCookies = true) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = handleCookies,
            AllowAutoRedirect = false
        });

    private static string CaptureSessionCookie(HttpResponseMessage loginResponse)
    {
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var setCookie = Assert.Single(
            loginResponse.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith(".StudyAI.Session=", StringComparison.Ordinal));
        return setCookie.Split(';', 2)[0];
    }

    private static async Task AssertAllSessionsRevokedAsync(string email, CancellationToken ct)
    {
        await using var context = PersistenceFixture.CreateAdminContext();
        var userId = await context.Users
            .Where(user => user.Email == email)
            .Select(user => user.Id)
            .SingleAsync(ct);
        var sessions = await context.UserSessions
            .Where(session => session.UserId == userId)
            .ToListAsync(ct);
        Assert.Equal(2, sessions.Count);
        Assert.All(sessions, session => Assert.NotNull(session.RevokedAt));
    }
}
