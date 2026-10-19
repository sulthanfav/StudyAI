namespace StudyAI.Api.Diagnostics;

public static class CorrelationIdPolicy
{
    public const string HeaderName = "X-Correlation-ID";

    public static string Resolve(string? candidate, int maxLength)
    {
        if (IsAcceptable(candidate, maxLength))
        {
            return candidate!;
        }

        var generated = Guid.CreateVersion7().ToString("N");
        return generated[..Math.Min(generated.Length, maxLength)];
    }

    private static bool IsAcceptable(string? candidate, int maxLength) =>
        !string.IsNullOrEmpty(candidate) &&
        candidate.Length <= maxLength &&
        candidate.All(IsAllowedCharacter);

    private static bool IsAllowedCharacter(char character) =>
        char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.';
}
