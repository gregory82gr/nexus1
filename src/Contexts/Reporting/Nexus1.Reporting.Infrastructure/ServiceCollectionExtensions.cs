using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Reporting.Infrastructure.Messaging;
using Nexus1.Reporting.Infrastructure.Persistence;

namespace Nexus1.Reporting.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Reporting")));

        // Assumes AddNexusMessaging(...) was already called (Host composition
        // root registers messaging once per host, not once per context).
        services.AddSingleton<ReportingProjectionMessageHandler>();
        services.AddHostedService<ReportingConsumerBackgroundService>();

        services.AddScoped<RetryDispatcher>();
        services.AddHostedService<RetryDispatcherBackgroundService>();

        return services;
    }
}
