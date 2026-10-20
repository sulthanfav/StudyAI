using System.Net.Http.Json;
using StudyAI.Contracts.Auth;

namespace StudyAI.Api.IntegrationTests.Support;

internal static class AntiforgeryHttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostAsJsonWithCsrfAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken)
    {
        var csrf = await client.GetFromJsonAsync<CsrfTokenResponse>(
            "/api/v1/auth/csrf",
            cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add("X-XSRF-TOKEN", csrf!.Token);
        return await client.SendAsync(request, cancellationToken);
    }

    public static async Task<HttpResponseMessage> PostWithCsrfAsync(
        this HttpClient client,
        string requestUri,
        CancellationToken cancellationToken)
    {
        var csrf = await client.GetFromJsonAsync<CsrfTokenResponse>(
            "/api/v1/auth/csrf",
            cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Add("X-XSRF-TOKEN", csrf!.Token);
        return await client.SendAsync(request, cancellationToken);
    }
}
