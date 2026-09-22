using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Diagnosis;
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
        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("RootCause");
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

        // Fixed-incident diagnosis walking skeleton (ADR-032) -- the four LLM-free
        // seams, the audit seal, the run store, and the runner that composes them.
        // The natural-language generation step (Nexus1.RootCause.Explain) is
        // deliberately absent and not registered here: it waits on the served
        // model, and this pipeline validates a supplied DraftAnswer identically
        // whether it comes from a fixture or, later, that model.
        services.AddScoped<IGraphWalker, EfGraphWalker>();
        services.AddScoped<ITelemetryCorroborator, EfTelemetryCorroborator>();
        services.AddScoped<IRetriever, EfRetriever>();
        services.AddScoped<IAntiHallucinationValidator, RegistryAntiHallucinationValidator>();
        services.AddScoped<IAuditChainWriter, Sha256AuditChainWriter>();
        services.AddScoped<IDiagnosisRunStore, EfDiagnosisRunStore>();
        services.AddScoped<EmbeddingIngestor>();
        services.AddScoped<FixedIncidentDiagnosisRunner>();

        // LLM-free defaults: the semantic path stays dormant and generation
        // honestly abstains until AddRootCauseExplain replaces these with the
        // Ollama-backed implementations (ADR-033).
        services.AddScoped<IEmbedder, NoOpEmbedder>();
        services.AddScoped<IExplainer, NoOpExplainer>();

        return services;
    }
}
