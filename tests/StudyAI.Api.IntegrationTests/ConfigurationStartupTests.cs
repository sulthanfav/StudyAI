using StudyAI.Api.IntegrationTests.Support;
using Xunit;

namespace StudyAI.Api.IntegrationTests;

public sealed class ConfigurationStartupTests
{
    [Fact]
    public void InvalidConfigurationStopsStartupWithoutEchoingTheValue()
    {
        const string suppliedValue = "invalid service name startup-secret-canary";
        using var factory = new StudyAIApiFactory(new Dictionary<string, string?>
        {
            ["Foundation:ServiceName"] = suppliedValue
        });

        var exception = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(exception);
        Assert.Contains("Foundation:ServiceName", exception.ToString());
        Assert.DoesNotContain(suppliedValue, exception.ToString());
    }
}
