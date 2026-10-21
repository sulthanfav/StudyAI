using System.ComponentModel.DataAnnotations;

namespace StudyAI.Api.Composition;

public sealed class BrowserAuthOptions : IValidatableObject
{
    public const string SectionName = "Authentication:Browser";

    public string InternalApiOrigin { get; set; } = "http://localhost:5000";

    public bool RequireHttps { get; set; }

    public string[] AllowedOrigins { get; set; } = [];

    public bool AllowCredentials { get; set; }

    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "DELETE", "OPTIONS"];

    public string[] AllowedHeaders { get; set; } = ["Content-Type", "X-XSRF-TOKEN", "X-Correlation-Id", "X-Data-Space-Id"];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(InternalApiOrigin))
        {
            yield return new ValidationResult("InternalApiOrigin must not be empty.", [nameof(InternalApiOrigin)]);
        }

        if (AllowCredentials && AllowedOrigins.Contains("*", StringComparer.Ordinal))
        {
            yield return new ValidationResult(
                "Wildcard origin ('*') cannot be combined with AllowCredentials.",
                [nameof(AllowedOrigins)]);
        }

        if (RequireHttps)
        {
            foreach (var origin in AllowedOrigins)
            {
                if (origin != "*" && !origin.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        $"Origin '{origin}' must use HTTPS when RequireHttps is enabled.",
                        [nameof(AllowedOrigins)]);
                }
            }
        }
    }
}