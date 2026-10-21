using System.Net;
using System.Net.Http.Json;
using StudyAI.Contracts.Auth;

namespace StudyAI.Api.IntegrationTests.Support;

internal static class AntiforgeryHttpClientExtensions
{
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";

    public static async Task<HttpResponseMessage> PostWithAntiforgeryAsync<T>(
        this HttpClient client,
        string requestUri,
        T body,
        CancellationToken cancellationToken)
    {
        var requestToken = await client.GetAntiforgeryTokenAsync(cancellationToken);

        return await client.PostWithAntiforgeryTokenAsync(
            requestUri,
            body,
            requestToken,
            cancellationToken);
    }

    public static async Task<string> GetAntiforgeryTokenAsync(
        this HttpClient client,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await client.GetAsync("/api/v1/auth/csrf", cancellationToken);
        if (tokenResponse.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                $"Could not acquire an antiforgery token. Status: {(int)tokenResponse.StatusCode}.");
        }

        var token = await tokenResponse.Content.ReadFromJsonAsync<CsrfTokenResponse>(cancellationToken);
        if (string.IsNullOrWhiteSpace(token?.Token))
        {
            throw new InvalidOperationException("The antiforgery endpoint returned an empty request token.");
        }

        return token.Token;
    }

    public static async Task<HttpResponseMessage> PostWithAntiforgeryTokenAsync<T>(
        this HttpClient client,
        string requestUri,
        T body,
        string requestToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = body is null ? null : JsonContent.Create(body)
        };
        request.Headers.Add(AntiforgeryHeader, requestToken);
        return await client.SendAsync(request, cancellationToken);
    }
}
