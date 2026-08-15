using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Audit.Infrastructure.Messaging;
using Nexus1.Audit.Infrastructure.Persistence;

namespace Nexus1.Audit.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuditDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Audit"))
            .AddInterceptors(new AuditAppendOnlyInterceptor()));

        // Assumes AddNexusMessaging(...) was already called (Host composition
        // root registers messaging once per host, not once per context).
        services.AddSingleton<AuditVerdictMessageHandler>();
        services.AddHostedService<AuditConsumerBackgroundService>();

        services.AddScoped<RetryDispatcher>();
        services.AddHostedService<RetryDispatcherBackgroundService>();

        return services;
    }
}
