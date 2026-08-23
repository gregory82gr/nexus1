using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.AlarmManagement.Infrastructure.Messaging;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// enableOutboxRelay defaults to true, preserving exact prior behavior for
    /// every existing caller (Nexus1.ModularRuntime). Set false for a host that
    /// composes AlarmManagement for its Application/read-write surface only and
    /// does not also want to be a relay/metrics process for this context's
    /// outbox — e.g. Nexus1.Bff (ADR-030's follow-up slice), which registers
    /// neither AddNexusMessaging (IBrokerPublisher) nor AddNexusObservability
    /// (OutboxMetricState). Composing OutboxRelay/OutboxMetricRefreshBackgroundService
    /// without either of those crashes host startup with a DI validation error
    /// (confirmed directly, not assumed) — and even if it didn't crash, running
    /// a second, unrelated process's outbox relay loop against the same
    /// AlarmManagementDb outbox table alongside Nexus1.ModularRuntime's own
    /// would be a genuine duplicate-relay hazard, not just a DI wiring gap.
    /// IOutboxWriter/EfOutboxWriter stay registered either way — enqueuing a
    /// row has no broker/observability dependency; only relaying it does.
    /// </summary>
    public static IServiceCollection AddAlarmManagementInfrastructure(
        this IServiceCollection services, string connectionString, bool enableOutboxRelay = true)
    {
        services.AddDbContext<AlarmManagementDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_AlarmManagement")));

        services.AddScoped<IRepository<AlarmDefinition, AlarmDefinitionId>, EfRepository<AlarmDefinition, AlarmDefinitionId>>();
        services.AddScoped<IRepository<AlarmEvent, AlarmEventId>, EfRepository<AlarmEvent, AlarmEventId>>();
        services.AddScoped<IRepository<AlarmFlood, AlarmFloodId>, EfRepository<AlarmFlood, AlarmFloodId>>();
        services.AddScoped<IAlarmDefinitionFinder, EfAlarmDefinitionFinder>();
        services.AddScoped<IAlarmEventFinder, EfAlarmEventFinder>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IOutboxWriter, EfOutboxWriter>();

        if (enableOutboxRelay)
        {
            services.AddScoped<OutboxRelay>();
            services.AddHostedService<OutboxPublisherBackgroundService>();

            // Assumes AddNexusObservability(...) already registered OutboxMetricState
            // (host composition root registers observability once per host, not
            // once per context — same pattern as AddNexusMessaging).
            services.AddSingleton<AlarmManagementOutboxMetricSnapshotReader>();
            services.AddHostedService<OutboxMetricRefreshBackgroundService>();
        }

        return services;
    }
}
