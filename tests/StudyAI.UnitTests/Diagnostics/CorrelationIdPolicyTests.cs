using StudyAI.Api.Diagnostics;
using Xunit;

namespace StudyAI.UnitTests.Diagnostics;

public sealed class CorrelationIdPolicyTests
{
    [Fact]
    public void ResolvePreservesAValidCallerIdentifier()
    {
        const string candidate = "studyai-request_2026.10-19";

        var result = CorrelationIdPolicy.Resolve(candidate, 64);

        Assert.Equal(candidate, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("contains space")]
    [InlineData("contains\r\ncontrol")]
    public void ResolveGeneratesASafeIdentifierForMissingOrInvalidInput(string? candidate)
    {
        var result = CorrelationIdPolicy.Resolve(candidate, 64);

        Assert.NotEmpty(result);
        Assert.InRange(result.Length, 16, 64);
        Assert.All(result, character => Assert.True(IsAllowed(character)));
        Assert.NotEqual(candidate, result);
    }

    [Fact]
    public void ResolveReplacesAnOversizedIdentifier()
    {
        var candidate = new string('a', 65);

        var result = CorrelationIdPolicy.Resolve(candidate, 64);

        Assert.NotEqual(candidate, result);
        Assert.InRange(result.Length, 16, 64);
    }

    private static bool IsAllowed(char character) =>
        char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.';
}
