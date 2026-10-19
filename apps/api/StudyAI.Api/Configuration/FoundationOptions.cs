namespace StudyAI.Api.Configuration;

public sealed class FoundationOptions
{
    public const string SectionName = "Foundation";

    public string ServiceName { get; init; } = string.Empty;

    public int MaxCorrelationIdLength { get; init; } = 64;
}
