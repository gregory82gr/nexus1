using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRootCauseInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RootCauseDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_RootCause")));

        services.AddScoped<IRepository<RootCauseAnalysis, RootCauseAnalysisId>, RootCauseAnalysisRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IOutboxWriter, EfOutboxWriter>();
        services.AddScoped<OutboxRelay>();
        services.AddHostedService<OutboxPublisherBackgroundService>();

        // Assumes AddNexusObservability(...) already registered OutboxMetricState
        // (host composition root registers observability once per host, not
        // once per context — same pattern as AddNexusMessaging).
        services.AddSingleton<RootCauseOutboxMetricSnapshotReader>();
        services.AddHostedService<OutboxMetricRefreshBackgroundService>();

        // Assumes AddNexusMessaging(...) was already called (Host composition
        // root registers messaging once per host, not once per context).
        services.AddSingleton<AlarmFloodMessageHandler>();
        services.AddHostedService<AlarmFloodConsumerBackgroundService>();

        services.AddScoped<RetryDispatcher>();
        services.AddHostedService<RetryDispatcherBackgroundService>();

        return services;
    }
}
