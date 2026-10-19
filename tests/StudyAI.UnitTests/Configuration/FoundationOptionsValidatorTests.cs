using Microsoft.Extensions.Options;
using StudyAI.Api.Configuration;
using Xunit;

namespace StudyAI.UnitTests.Configuration;

public sealed class FoundationOptionsValidatorTests
{
    private readonly FoundationOptionsValidator _validator = new();

    [Fact]
    public void ValidateAcceptsValidFoundationOptions()
    {
        var options = new FoundationOptions
        {
            ServiceName = "StudyAI.Api",
            MaxCorrelationIdLength = 64
        };

        var result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void ValidateRejectsMissingServiceNameWithoutEchoingValues()
    {
        var options = new FoundationOptions
        {
            ServiceName = string.Empty,
            MaxCorrelationIdLength = 64
        };

        var result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        Assert.Contains("Foundation:ServiceName", Assert.Single(result.Failures));
    }

    [Theory]
    [InlineData("StudyAI Api secret-canary", 64, "Foundation:ServiceName", "secret-canary")]
    [InlineData("StudyAI.Api", 4, "Foundation:MaxCorrelationIdLength", "4")]
    [InlineData("StudyAI.Api", 129, "Foundation:MaxCorrelationIdLength", "129")]
    public void ValidateRejectsMalformedValuesWithoutEchoingThem(
        string serviceName,
        int maxCorrelationIdLength,
        string expectedKey,
        string suppliedValue)
    {
        var options = new FoundationOptions
        {
            ServiceName = serviceName,
            MaxCorrelationIdLength = maxCorrelationIdLength
        };

        var result = _validator.Validate(Options.DefaultName, options);
        Assert.NotNull(result.Failures);
        var failure = Assert.Single(result.Failures);

        Assert.Contains(expectedKey, failure);
        Assert.DoesNotContain(suppliedValue, failure);
    }
}
