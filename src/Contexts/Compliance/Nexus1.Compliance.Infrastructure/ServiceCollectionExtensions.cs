using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Compliance.Infrastructure.Messaging;
using Nexus1.Compliance.Infrastructure.Persistence;

namespace Nexus1.Compliance.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddComplianceInfrastructure(this IServiceCollection services, string connectionString)
    {
        // No append-only interceptor here, unlike AddAuditInfrastructure —
        // ComplianceReview is deliberately mutable (ADR-011).
        services.AddDbContext<ComplianceDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Compliance")));

        // Assumes AddNexusMessaging(...) was already called (Host composition
        // root registers messaging once per host, not once per context).
        services.AddSingleton<ComplianceVerdictMessageHandler>();
        services.AddHostedService<ComplianceConsumerBackgroundService>();

        services.AddScoped<RetryDispatcher>();
        services.AddHostedService<RetryDispatcherBackgroundService>();

        return services;
    }
}
