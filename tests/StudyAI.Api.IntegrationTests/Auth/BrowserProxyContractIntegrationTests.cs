using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using StudyAI.Api.IntegrationTests.Persistence;
using StudyAI.Api.IntegrationTests.Support;
using StudyAI.Contracts.Auth;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class BrowserProxyContractIntegrationTests : IAsyncLifetime
{
    private StudyAIApiFactory _factory = null!;
    private HttpClient _backendClient = null!;

    public async ValueTask InitializeAsync()
    {
        await PersistenceFixture.EnsureInitializedAsync(TestContext.Current.CancellationToken);

        _factory = new StudyAIApiFactory(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = PersistenceFixture.AppConnectionString,
            ["STUDYAI_TEST_APP_CONNECTION_STRING"] = PersistenceFixture.AppConnectionString,
            ["STUDYAI_TEST_ADMIN_CONNECTION_STRING"] = PersistenceFixture.AdminConnectionString
        });

        _backendClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });
    }

    public async ValueTask DisposeAsync()
    {
        _backendClient.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task ProxyPreservesAntiforgeryTokensAndCookies()
    {
        var ct = TestContext.Current.CancellationToken;

        // 1. GET /api/v1/auth/csrf through simulated proxy
        using var csrfRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/csrf");
        using var csrfResponse = await ForwardThroughProxyAsync(csrfRequest, ct);

        Assert.Equal(HttpStatusCode.OK, csrfResponse.StatusCode);
        Assert.True(csrfResponse.Headers.Contains("Set-Cookie"));

        var csrfPayload = await csrfResponse.Content.ReadFromJsonAsync<CsrfTokenResponse>(ct);
        Assert.NotNull(csrfPayload);
        Assert.False(string.IsNullOrWhiteSpace(csrfPayload.Token));

        var csrfCookie = ExtractCookie(csrfResponse, ".StudyAI.Antiforgery");
        Assert.NotNull(csrfCookie);

        // 2. POST /api/v1/auth/register through simulated proxy with cookie and header
        var email = $"proxy.{Guid.NewGuid():N}@studyai.test";
        using var regRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register")
        {
            Content = JsonContent.Create(new RegisterRequest
            {
                Email = email,
                Password = "Password123!",
                DisplayName = "Proxy User"
            })
        };
        regRequest.Headers.Add("Cookie", $".StudyAI.Antiforgery={csrfCookie}");
        regRequest.Headers.Add("X-XSRF-TOKEN", csrfPayload.Token);

        using var regResponse = await ForwardThroughProxyAsync(regRequest, ct);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        // 3. POST /api/v1/auth/login through simulated proxy
        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest
            {
                Email = email,
                Password = "Password123!"
            })
        };
        loginRequest.Headers.Add("Cookie", $".StudyAI.Antiforgery={csrfCookie}");
        loginRequest.Headers.Add("X-XSRF-TOKEN", csrfPayload.Token);

        using var loginResponse = await ForwardThroughProxyAsync(loginRequest, ct);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var sessionCookie = ExtractCookie(loginResponse, ".StudyAI.Session");
        Assert.NotNull(sessionCookie);

        // 4. GET /api/v1/auth/me through simulated proxy using session cookie
        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meRequest.Headers.Add("Cookie", $".StudyAI.Session={sessionCookie}");

        using var meResponse = await ForwardThroughProxyAsync(meRequest, ct);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await meResponse.Content.ReadFromJsonAsync<AuthUserResponse>(ct);
        Assert.NotNull(mePayload);
        Assert.Equal(email, mePayload.Email);
    }

    [Fact]
    public async Task ProxyPreservesProblemDetailsStatusAndBody()
    {
        var ct = TestContext.Current.CancellationToken;

        // Attempt login with invalid password through proxy without CSRF
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest
            {
                Email = "nobody@studyai.test",
                Password = "WrongPassword999!"
            })
        };

        using var response = await ForwardThroughProxyAsync(request, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("problem", response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<HttpResponseMessage> ForwardThroughProxyAsync(
        HttpRequestMessage clientRequest,
        CancellationToken ct)
    {
        using var upstreamRequest = new HttpRequestMessage(clientRequest.Method, clientRequest.RequestUri);
        if (clientRequest.Content is not null)
        {
            var bytes = await clientRequest.Content.ReadAsByteArrayAsync(ct);
            upstreamRequest.Content = new ByteArrayContent(bytes);
            if (clientRequest.Content.Headers.ContentType is not null)
            {
                upstreamRequest.Content.Headers.ContentType = clientRequest.Content.Headers.ContentType;
            }
        }

        foreach (var header in clientRequest.Headers)
        {
            upstreamRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var upstreamResponse = await _backendClient.SendAsync(upstreamRequest, ct);

        var clientResponse = new HttpResponseMessage(upstreamResponse.StatusCode);
        var responseBytes = await upstreamResponse.Content.ReadAsByteArrayAsync(ct);
        clientResponse.Content = new ByteArrayContent(responseBytes);
        if (upstreamResponse.Content.Headers.ContentType is not null)
        {
            clientResponse.Content.Headers.ContentType = upstreamResponse.Content.Headers.ContentType;
        }

        foreach (var header in upstreamResponse.Headers)
        {
            clientResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clientResponse;
    }

    private static string? ExtractCookie(HttpResponseMessage response, string cookieName)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            foreach (var value in values)
            {
                if (value.StartsWith($"{cookieName}=", StringComparison.OrdinalIgnoreCase))
                {
                    var cookiePart = value.Split(';')[0];
                    return cookiePart.Substring(cookieName.Length + 1);
                }
            }
        }
        return null;
    }
}