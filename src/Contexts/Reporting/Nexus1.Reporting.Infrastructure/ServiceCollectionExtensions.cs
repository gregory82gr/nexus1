using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Reporting.Application;
using Nexus1.Reporting.Infrastructure.Messaging;
using Nexus1.Reporting.Infrastructure.Persistence;

namespace Nexus1.Reporting.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// enableMessagingConsumer defaults to true, preserving exact prior
    /// behavior for Nexus1.ModularRuntime. Set false for a host that
    /// composes Reporting for its read surface only — e.g. Nexus1.Bff, which
    /// registers neither AddNexusMessaging (RabbitMqConnectionManager/
    /// RabbitMqOptions/IBrokerPublisher) nor AddNexusObservability
    /// (NexusRuntimeMetrics). Reporting's whole prior existence was
    /// write-side-only messaging (ReportingConsumerBackgroundService needs
    /// RabbitMqConnectionManager/RabbitMqOptions; ReportingProjectionMessageHandler
    /// needs NexusRuntimeMetrics; RetryDispatcher needs IBrokerPublisher) —
    /// all three confirmed unresolved by reading each constructor directly,
    /// the same class of startup crash already found and fixed for
    /// AlarmManagement (ADR-030 follow-up evidence). Running a second
    /// consumer/retry-dispatcher process against the same queue/ReportingDb
    /// alongside Nexus1.ModularRuntime's own would also be a duplicate-consumer
    /// hazard even without the DI crash.
    /// </summary>
    public static IServiceCollection AddReportingInfrastructure(
        this IServiceCollection services, string connectionString, bool enableMessagingConsumer = true)
    {
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Reporting")));

        services.AddScoped<ICaseSummaryFinder, EfCaseSummaryFinder>();

        if (enableMessagingConsumer)
        {
            // Assumes AddNexusMessaging(...) was already called (Host composition
            // root registers messaging once per host, not once per context).
            services.AddSingleton<ReportingProjectionMessageHandler>();
            services.AddHostedService<ReportingConsumerBackgroundService>();

            services.AddScoped<RetryDispatcher>();
            services.AddHostedService<RetryDispatcherBackgroundService>();
        }

        return services;
    }
}
