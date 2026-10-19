using System.Net;
using System.Text;
using StudyAI.Api.Diagnostics;
using StudyAI.Api.IntegrationTests.Support;
using Xunit;

namespace StudyAI.Api.IntegrationTests;

public sealed class CorrelationAndLoggingTests
{
    [Fact]
    public async Task ValidCorrelationIdentifierIsReturnedAndIncludedInLogScope()
    {
        const string correlationId = "studyai-integration-20261019";
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add(CorrelationIdPolicy.HeaderName, correlationId);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(correlationId, Assert.Single(response.Headers.GetValues(CorrelationIdPolicy.HeaderName)));
        Assert.Contains(factory.Logs.Entries, entry =>
            entry.Contains($"scope:CorrelationId={correlationId}", StringComparison.Ordinal) &&
            entry.Contains($"state:CorrelationId={correlationId}", StringComparison.Ordinal));
    }

    [Fact]
    public async Task MissingCorrelationIdentifierIsReplacedWithABoundedIdentifier()
    {
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
        var correlationId = Assert.Single(response.Headers.GetValues(CorrelationIdPolicy.HeaderName));

        Assert.InRange(correlationId.Length, 16, 64);
        Assert.Contains(factory.Logs.Entries, entry => entry.Contains($"scope:CorrelationId={correlationId}", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UnsafeCorrelationIdentifierIsReplaced(bool oversized)
    {
        var unsafeIdentifier = oversized ? new string('a', 65) : "control\u007fcharacter";
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        Assert.True(request.Headers.TryAddWithoutValidation(CorrelationIdPolicy.HeaderName, unsafeIdentifier));

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var correlationId = Assert.Single(response.Headers.GetValues(CorrelationIdPolicy.HeaderName));

        Assert.NotEqual(unsafeIdentifier, correlationId);
        Assert.InRange(correlationId.Length, 16, 64);
    }

    [Fact]
    public async Task DefaultLogsExcludeSensitiveHeadersQueriesAndBodies()
    {
        const string authorizationCanary = "authorization-canary-9811";
        const string signedUrlCanary = "signed-url-canary-2844";
        const string credentialCanary = "credential-canary-6732";
        const string promptCanary = "prompt-canary-7751";
        const string noteCanary = "note-canary-4865";
        const string documentCanary = "document-canary-3992";
        var body = $$"""
            {
              "credential": "{{credentialCanary}}",
              "prompt": "{{promptCanary}}",
              "note": "{{noteCanary}}",
              "document": "{{documentCanary}}"
            }
            """;
        using var factory = new StudyAIApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/not-found?returnUrl={Uri.EscapeDataString($"https://private.invalid/file?sig={signedUrlCanary}")}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new("Bearer", authorizationCanary);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var logs = string.Join(Environment.NewLine, factory.Logs.Entries);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(authorizationCanary, logs);
        Assert.DoesNotContain(signedUrlCanary, logs);
        Assert.DoesNotContain(credentialCanary, logs);
        Assert.DoesNotContain(promptCanary, logs);
        Assert.DoesNotContain(noteCanary, logs);
        Assert.DoesNotContain(documentCanary, logs);
    }
}
