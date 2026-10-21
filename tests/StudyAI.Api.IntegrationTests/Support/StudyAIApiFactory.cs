using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace StudyAI.Api.IntegrationTests.Support;

internal sealed class StudyAIApiFactory : WebApplicationFactory<Program>
{
    private readonly IReadOnlyDictionary<string, string?> _configuration;
    private readonly bool _includeUnhealthyReadinessCheck;
    private readonly Action<IServiceCollection>? _configureServices;

    public StudyAIApiFactory(
        IReadOnlyDictionary<string, string?>? configuration = null,
        bool includeUnhealthyReadinessCheck = false,
        Action<IServiceCollection>? configureServices = null)
    {
        _configuration = configuration ?? new Dictionary<string, string?>();
        _includeUnhealthyReadinessCheck = includeUnhealthyReadinessCheck;
        _configureServices = configureServices;
    }

    public CapturingLoggerProvider Logs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            configurationBuilder.AddInMemoryCollection(_configuration));
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddProvider(Logs);
        });

        if (_includeUnhealthyReadinessCheck)
        {
            builder.ConfigureServices(services =>
                services.AddHealthChecks().AddCheck(
                    "forced-unhealthy",
                    () => HealthCheckResult.Unhealthy("Server=private;Password=health-canary"),
                    tags: ["ready"]));
        }

        if (_configureServices is not null)
        {
            builder.ConfigureServices(_configureServices);
        }
    }
}
