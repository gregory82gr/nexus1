using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Audit.Application;
using Nexus1.Audit.Infrastructure.Messaging;
using Nexus1.Audit.Infrastructure.Persistence;

namespace Nexus1.Audit.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// enableMessagingConsumer defaults to true, preserving exact prior
    /// behavior for Nexus1.ModularRuntime. Set false for a host that
    /// composes Audit for its read surface only — e.g. Nexus1.Bff, which
    /// registers neither AddNexusMessaging (RabbitMqConnectionManager/
    /// RabbitMqOptions/IBrokerPublisher) nor AddNexusObservability
    /// (NexusRuntimeMetrics). Confirmed by reading each constructor directly
    /// (same rigor as Reporting/AlarmManagement, not assumed from either
    /// precedent): AuditConsumerBackgroundService needs
    /// RabbitMqConnectionManager/RabbitMqOptions/NexusRuntimeMetrics;
    /// AuditVerdictMessageHandler needs NexusRuntimeMetrics; RetryDispatcher
    /// needs IBrokerPublisher — all three unresolved in the BFF's DI
    /// container. Running a second consumer/retry-dispatcher process against
    /// the same queue/AuditDb alongside Nexus1.ModularRuntime's own would
    /// also be a duplicate-consumer hazard even without the DI crash.
    /// </summary>
    public static IServiceCollection AddAuditInfrastructure(
        this IServiceCollection services, string connectionString, bool enableMessagingConsumer = true)
    {
        services.AddDbContext<AuditDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Audit"))
            .AddInterceptors(new AuditAppendOnlyInterceptor()));

        services.AddScoped<IAuditEvidenceFinder, EfAuditEvidenceFinder>();

        if (enableMessagingConsumer)
        {
            // Assumes AddNexusMessaging(...) was already called (Host composition
            // root registers messaging once per host, not once per context).
            services.AddSingleton<AuditVerdictMessageHandler>();
            services.AddHostedService<AuditConsumerBackgroundService>();

            services.AddScoped<RetryDispatcher>();
            services.AddHostedService<RetryDispatcherBackgroundService>();
        }

        return services;
    }
}
