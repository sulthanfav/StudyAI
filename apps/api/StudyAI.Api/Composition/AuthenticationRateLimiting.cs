using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace StudyAI.Api.Composition;

internal static class AuthenticationRateLimitPolicies
{
    public const string Register = "auth-register";
    public const string Login = "auth-login";
}

internal sealed class AuthenticationRateLimitsOptions
{
    public AuthenticationRateLimitPolicyOptions Register { get; init; } = new();
    public AuthenticationRateLimitPolicyOptions Login { get; init; } = new();
}

internal sealed class AuthenticationRateLimitPolicyOptions
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
}

internal static class AuthenticationRateLimiting
{
    public static IServiceCollection AddAuthenticationAbuseControls(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthenticationRateLimitsOptions>(
            configuration.GetRequiredSection("Authentication:RateLimits"));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                await Results.Problem(
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Too Many Requests",
                    type: "https://www.rfc-editor.org/rfc/rfc6585#section-4",
                    detail: "The authentication request limit was exceeded.",
                    extensions: new Dictionary<string, object?>
                    {
                        ["code"] = "auth_rate_limit_exceeded"
                    }).ExecuteAsync(context.HttpContext);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var settings = context.RequestServices
                    .GetRequiredService<IOptions<AuthenticationRateLimitsOptions>>()
                    .Value;
                if (!HttpMethods.IsPost(context.Request.Method))
                {
                    return RateLimitPartition.GetNoLimiter("non-auth-mutation");
                }

                return context.Request.Path.Value switch
                {
                    "/api/v1/auth/register" => CreatePartition(
                        context, AuthenticationRateLimitPolicies.Register, settings.Register),
                    "/api/v1/auth/login" => CreatePartition(
                        context, AuthenticationRateLimitPolicies.Login, settings.Login),
                    _ => RateLimitPartition.GetNoLimiter("other-mutation")
                };
            });
        });

        ConfigureForwardedAddresses(services, configuration);
        return services;
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext context,
        string policyName,
        AuthenticationRateLimitPolicyOptions settings)
    {
        if (settings.PermitLimit <= 0 || settings.WindowSeconds <= 0)
        {
            throw new InvalidOperationException(
                $"Authentication rate-limit policy '{policyName}' requires positive PermitLimit and WindowSeconds values.");
        }

        var peerAddress = context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown-peer";
        return RateLimitPartition.GetFixedWindowLimiter(
            $"{policyName}:{peerAddress}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.PermitLimit,
                Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    }

    private static void ConfigureForwardedAddresses(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ForwardedHeadersOptions>().Configure<IConfiguration>((options, resolvedConfiguration) =>
        {
            var section = resolvedConfiguration.GetSection("Authentication:ForwardedHeaders");
            var knownProxyValues = section.GetSection("KnownProxies").Get<string[]>() ?? [];
            var knownNetworkValues = section.GetSection("KnownNetworks").Get<string[]>() ?? [];
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            options.ForwardLimit = 1;
            options.ForwardedHeaders = knownProxyValues.Length == 0 && knownNetworkValues.Length == 0
                ? ForwardedHeaders.None
                : ForwardedHeaders.XForwardedFor;

            foreach (var value in knownProxyValues)
            {
                if (!IPAddress.TryParse(value, out var address))
                {
                    throw new InvalidOperationException($"Configured trusted proxy '{value}' is not a valid IP address.");
                }

                options.KnownProxies.Add(address);
            }

            foreach (var value in knownNetworkValues)
            {
                if (!System.Net.IPNetwork.TryParse(value, out var network))
                {
                    throw new InvalidOperationException($"Configured trusted network '{value}' is not valid CIDR notation.");
                }

                options.KnownIPNetworks.Add(network);
            }
        });
    }
}
