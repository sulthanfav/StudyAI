using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyAI.Application.Common.Interfaces;
using StudyAI.Infrastructure.Persistence;
using StudyAI.Infrastructure.Persistence.Interceptors;
using StudyAI.Application.Workspace;
using StudyAI.Infrastructure.Workspace;

namespace StudyAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructurePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<CurrentDataSpaceAccessor>();
        services.AddScoped<ICurrentDataSpaceAccessor>(sp => sp.GetRequiredService<CurrentDataSpaceAccessor>());
        services.AddScoped<ICurrentDataSpaceContext>(sp => sp.GetRequiredService<CurrentDataSpaceAccessor>());
        services.AddScoped<ICurrentActorAccessor>(sp => sp.GetRequiredService<CurrentDataSpaceAccessor>());
        services.AddScoped<ICurrentActorContext>(sp => sp.GetRequiredService<CurrentDataSpaceAccessor>());

        services.AddScoped<TenantSessionInterceptor>();

        services.AddDbContext<StudyAIDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("StudyAIDatabase")
                ?? config["Database:ConnectionString"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var interceptor = sp.GetRequiredService<TenantSessionInterceptor>();
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.UseVector();
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", "ops");
                });
                options.AddInterceptors(interceptor);
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<StudyAIDbContext>());

        services.AddScoped<Microsoft.AspNetCore.Identity.IUserStore<StudyAI.Domain.Identity.User>, StudyAI.Infrastructure.Identity.StudyAIUserStore>();
        services.AddScoped<StudyAI.Application.Auth.IAuthService, StudyAI.Infrastructure.Identity.AuthService>();
        services.AddScoped<IDataSpaceAuthorizationService, StudyAI.Infrastructure.Identity.DataSpaceAuthorizationService>();
        services.AddScoped<IContentSpaceReader, EfContentSpaceReader>();

        return services;
    }
}
