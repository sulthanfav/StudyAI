using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class RegistrationRaceIntegrationTests : IAsyncLifetime
{
    private StudyAIApiFactory _factory = null!;

    public async ValueTask InitializeAsync()
    {
        await PersistenceFixture.EnsureInitializedAsync(TestContext.Current.CancellationToken);
        _factory = new StudyAIApiFactory(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = PersistenceFixture.AppConnectionString,
            ["Authentication:RateLimits:Register:PermitLimit"] = "100",
            ["Authentication:RateLimits:Register:WindowSeconds"] = "60"
        });
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task ConcurrentMixedCaseEmailCreatesExactlyOneAccount()
    {
        var ct = TestContext.Current.CancellationToken;
        var localPart = $"Race.Mixed.{Guid.NewGuid():N}";
        var firstEmail = $"{localPart}@StudyAI.Test";
        var secondEmail = $"{localPart.ToLowerInvariant()}@studyai.test";

        var responses = await RegisterConcurrentlyAsync(firstEmail, secondEmail, ct);

        await AssertSingleCreatedAndStableConflictAsync(responses, ct);
        await using var context = PersistenceFixture.CreateAdminContext();
        var normalizedEmail = firstEmail.ToUpperInvariant();
        Assert.Equal(1, await context.Users.CountAsync(
            user => user.NormalizedEmail == normalizedEmail,
            ct));
    }

    [Fact]
    public async Task ConcurrentSameCaseEmailAlwaysCreatesExactlyOneAccount()
    {
        var ct = TestContext.Current.CancellationToken;
        for (var iteration = 0; iteration < 5; iteration++)
        {
            var email = $"race.same.{iteration}.{Guid.NewGuid():N}@studyai.test";
            var responses = await RegisterConcurrentlyAsync(email, email, ct);

            await AssertSingleCreatedAndStableConflictAsync(responses, ct);
            await using var context = PersistenceFixture.CreateAdminContext();
            Assert.Equal(1, await context.Users.CountAsync(user => user.Email == email, ct));
        }
    }

    private async Task<HttpResponseMessage[]> RegisterConcurrentlyAsync(
        string firstEmail,
        string secondEmail,
        CancellationToken ct)
    {
        using var firstClient = CreateCookieClient();
        using var secondClient = CreateCookieClient();
        var first = firstClient.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(firstEmail), ct);
        var second = secondClient.PostWithAntiforgeryAsync(
            "/api/v1/auth/register", Registration(secondEmail), ct);
        return await Task.WhenAll(first, second);
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
        DisplayName = "Registration Race User"
    };

    private static async Task AssertSingleCreatedAndStableConflictAsync(
        IEnumerable<HttpResponseMessage> responses,
        CancellationToken ct)
    {
        var responseArray = responses.ToArray();
        var statusCodes = responseArray.Select(response => response.StatusCode).ToArray();
        Assert.Equal(1, statusCodes.Count(status => status == HttpStatusCode.Created));
        Assert.Equal(1, statusCodes.Count(status => status == HttpStatusCode.Conflict));
        Assert.DoesNotContain(statusCodes, status =>
            status != HttpStatusCode.Created && status != HttpStatusCode.Conflict);

        var conflict = Assert.Single(responseArray, response => response.StatusCode == HttpStatusCode.Conflict);
        var problem = await conflict.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(ct);
        Assert.NotNull(problem);
        Assert.Equal("Email Already Registered", problem.Title);
        Assert.Equal("An account with this email already exists.", problem.Detail);
        Assert.Equal(
            "auth_email_already_registered",
            Assert.IsType<System.Text.Json.JsonElement>(problem.Extensions["code"]).GetString());
    }
}
