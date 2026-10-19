using Microsoft.Extensions.Options;

namespace StudyAI.Api.Configuration;

public sealed class FoundationOptionsValidator : IValidateOptions<FoundationOptions>
{
    private const int MinimumCorrelationIdLength = 16;
    private const int MaximumCorrelationIdLength = 128;

    public ValidateOptionsResult Validate(string? name, FoundationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            failures.Add("Foundation:ServiceName is required.");
        }
        else if (options.ServiceName.Length > 64 || options.ServiceName.Any(character => !IsServiceNameCharacter(character)))
        {
            failures.Add("Foundation:ServiceName must contain only ASCII letters, digits, dots, or hyphens and be at most 64 characters.");
        }

        if (options.MaxCorrelationIdLength is < MinimumCorrelationIdLength or > MaximumCorrelationIdLength)
        {
            failures.Add("Foundation:MaxCorrelationIdLength must be within the supported range.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsServiceNameCharacter(char character) =>
        char.IsAsciiLetterOrDigit(character) || character is '.' or '-';
}
