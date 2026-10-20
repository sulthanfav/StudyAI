using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using StudyAI.Contracts.Workspace;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class DataSpaceAuthorizationIntegrationTests : IAsyncLifetime
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

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    private async Task<(HttpClient Client, RegisterResponse Info)> RegisterAndLoginUserAsync(
        string prefix,
        CancellationToken ct)
    {
        var email = $"{prefix}.{Guid.NewGuid():N}@studyai.test";
        var password = "Password123!";

        var registrationClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

        var regResponse = await registrationClient.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            DisplayName = $"{prefix} User"
        }, ct);

        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);
        var regPayload = await regResponse.Content.ReadFromJsonAsync<RegisterResponse>(ct);
        Assert.NotNull(regPayload);

        var sessionClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

        var loginResponse = await sessionClient.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        }, ct);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        return (sessionClient, regPayload);
    }

    [Fact]
    public async Task AccessToOwnedDataSpaceSucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, user) = await RegisterAndLoginUserAsync("owner", ct);
        using (client)
        {
            var response = await client.GetAsync($"/api/v1/data-spaces/{user.PersonalDataSpaceId}/content-spaces", ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contentSpaces = await response.Content.ReadFromJsonAsync<List<ContentSpaceResponse>>(ct);
            Assert.NotNull(contentSpaces);
            Assert.Single(contentSpaces);
            Assert.Equal("library", contentSpaces[0].Kind);
            Assert.Equal("Personal Library", contentSpaces[0].Title);
            Assert.Equal(user.PersonalDataSpaceId, contentSpaces[0].DataSpaceId);
        }
    }

    [Fact]
    public async Task AccessToAnotherUserDataSpaceDeniedWith403()
    {
        var ct = TestContext.Current.CancellationToken;
        var (clientA, userA) = await RegisterAndLoginUserAsync("usera", ct);
        var (clientB, userB) = await RegisterAndLoginUserAsync("userb", ct);

        using (clientA)
        using (clientB)
        {
            // User A attempts to access User B's personal data space
            var crossAccessResponse = await clientA.GetAsync($"/api/v1/data-spaces/{userB.PersonalDataSpaceId}/content-spaces", ct);
            Assert.Equal(HttpStatusCode.Forbidden, crossAccessResponse.StatusCode);

            var problem = await crossAccessResponse.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status403Forbidden, problem.Status);
            Assert.Contains("not authorized", problem.Detail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ForgedOrManipulatedDataSpaceIdDeniedWith403()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await RegisterAndLoginUserAsync("forger", ct);
        using (client)
        {
            var fakeId = Guid.NewGuid();
            var response = await client.GetAsync($"/api/v1/data-spaces/{fakeId}/content-spaces", ct);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task MissingAuthenticationReturns401()
    {
        var ct = TestContext.Current.CancellationToken;
        using var unauthClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });

        var fakeId = Guid.NewGuid();
        var response = await unauthClient.GetAsync($"/api/v1/data-spaces/{fakeId}/content-spaces", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConcurrentUsersPooledConnectionsZeroContextLeakage()
    {
        var ct = TestContext.Current.CancellationToken;
        var (clientA, userA) = await RegisterAndLoginUserAsync("pooled_a", ct);
        var (clientB, userB) = await RegisterAndLoginUserAsync("pooled_b", ct);

        using (clientA)
        using (clientB)
        {
            // Run 20 rapid interleaved requests across both clients sharing pooled database connections
            var tasks = new List<Task<HttpResponseMessage>>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(clientA.GetAsync($"/api/v1/data-spaces/{userA.PersonalDataSpaceId}/content-spaces", ct));
                tasks.Add(clientB.GetAsync($"/api/v1/data-spaces/{userB.PersonalDataSpaceId}/content-spaces", ct));
            }

            var responses = await Task.WhenAll(tasks);

            foreach (var response in responses)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }

    [Fact]
    public async Task CsrfTokenEndpointIssuesValidToken()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/csrf", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CsrfTokenResponse>(ct);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
    }
}
