using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Compliance.Application;
using Nexus1.Compliance.Infrastructure.Messaging;
using Nexus1.Compliance.Infrastructure.Persistence;

namespace Nexus1.Compliance.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// enableMessagingConsumer defaults to true, preserving exact prior
    /// behavior for Nexus1.ModularRuntime. Set false for a host that
    /// composes Compliance for its read surface only — e.g. Nexus1.Bff,
    /// which registers neither AddNexusMessaging (RabbitMqConnectionManager/
    /// RabbitMqOptions/IBrokerPublisher) nor AddNexusObservability
    /// (NexusRuntimeMetrics). Confirmed by reading each constructor directly
    /// (same rigor as Audit/Reporting/AlarmManagement): ComplianceConsumerBackgroundService
    /// needs RabbitMqConnectionManager/RabbitMqOptions/NexusRuntimeMetrics;
    /// ComplianceVerdictMessageHandler needs NexusRuntimeMetrics; RetryDispatcher
    /// needs IBrokerPublisher — all three unresolved in the BFF's DI
    /// container. Running a second consumer/retry-dispatcher process against
    /// the same queue/ComplianceDb alongside Nexus1.ModularRuntime's own
    /// would also be a duplicate-consumer hazard even without the DI crash.
    /// </summary>
    public static IServiceCollection AddComplianceInfrastructure(
        this IServiceCollection services, string connectionString, bool enableMessagingConsumer = true)
    {
        // No append-only interceptor here, unlike AddAuditInfrastructure —
        // ComplianceReview is deliberately mutable (ADR-011).
        services.AddDbContext<ComplianceDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Compliance")));

        services.AddScoped<IComplianceReviewFinder, EfComplianceReviewFinder>();

        if (enableMessagingConsumer)
        {
            // Assumes AddNexusMessaging(...) was already called (Host composition
            // root registers messaging once per host, not once per context).
            services.AddSingleton<ComplianceVerdictMessageHandler>();
            services.AddHostedService<ComplianceConsumerBackgroundService>();

            services.AddScoped<RetryDispatcher>();
            services.AddHostedService<RetryDispatcherBackgroundService>();
        }

        return services;
    }
}
