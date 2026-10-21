using System.ComponentModel.DataAnnotations;
using StudyAI.Api.Composition;
using Xunit;

namespace StudyAI.Api.IntegrationTests.Auth;

public sealed class BrowserAuthConfigurationTests
{
    [Fact]
    public void ValidConfigurationPassesValidation()
    {
        var options = new BrowserAuthOptions
        {
            InternalApiOrigin = "https://api.internal:5000",
            RequireHttps = true,
            AllowedOrigins = ["https://app.studyai.local"],
            AllowCredentials = true
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void WildcardOriginCombinedWithCredentialsFailsValidation()
    {
        var options = new BrowserAuthOptions
        {
            InternalApiOrigin = "http://localhost:5000",
            AllowedOrigins = ["*"],
            AllowCredentials = true
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Wildcard origin", StringComparison.Ordinal));
    }

    [Fact]
    public void NonHttpsOriginFailsValidationWhenRequireHttpsIsEnabled()
    {
        var options = new BrowserAuthOptions
        {
            InternalApiOrigin = "https://api.internal:5000",
            RequireHttps = true,
            AllowedOrigins = ["http://insecure.studyai.local"],
            AllowCredentials = true
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("must use HTTPS", StringComparison.Ordinal));
    }
}