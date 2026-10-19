using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StudyAI.Api.Configuration;

namespace StudyAI.Api.Composition;

public static class FoundationServiceCollectionExtensions
{
    public static IServiceCollection AddStudyAIFoundation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<FoundationOptions>()
            .Bind(configuration.GetSection(FoundationOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<FoundationOptions>, FoundationOptionsValidator>();

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
            };
        });
        services.AddOpenApi();
        services
            .AddHealthChecks()
            .AddCheck(
                "foundation",
                () => HealthCheckResult.Healthy(),
                tags: ["ready"]);

        return services;
    }
}
