using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        services.AddScoped<IIncidentGraphReader, EfIncidentGraphReader>();
        services.AddScoped<EmbeddingIngestor>();
        services.AddScoped<FixedIncidentDiagnosisRunner>();

        // LLM-free defaults: the semantic path stays dormant and generation
        // honestly abstains until AddRootCauseExplain replaces these with the
        // Ollama-backed implementations (ADR-033).
        services.AddScoped<IEmbedder, NoOpEmbedder>();
        services.AddScoped<IExplainer, NoOpExplainer>();

        return services;
    }

    /// <summary>
    /// Binds the grounding/retrieval seams (EfRetriever, RegistryAntiHallucinationValidator)
    /// to a READ-ONLY RootCauseDbContext built from <paramref name="readOnlyConnectionString"/>
    /// (the nexus1_explain login, H7), while every write seam keeps the default
    /// read-write context. Call after AddRootCauseInfrastructure and AddRootCauseExplain
    /// so these replacements win. The keyed context is disposed per request scope by
    /// the container. Recorded in ADR-034; the split is proven live via
    /// sys.dm_exec_sessions. If a write seam were ever handed this context, the
    /// write would fail loudly with a permission error rather than silently succeed.
    /// </summary>
    public static IServiceCollection AddRootCauseReadOnlyRetrieval(this IServiceCollection services, string readOnlyConnectionString)
    {
        services.AddKeyedScoped<RootCauseDbContext>("readonly", (_, _) =>
            new RootCauseDbContext(new DbContextOptionsBuilder<RootCauseDbContext>()
                .UseSqlServer(readOnlyConnectionString).Options));

        services.Replace(ServiceDescriptor.Scoped<IRetriever>(sp =>
            new EfRetriever(sp.GetRequiredKeyedService<RootCauseDbContext>("readonly"), sp.GetRequiredService<IEmbedder>())));

        services.Replace(ServiceDescriptor.Scoped<IAntiHallucinationValidator>(sp =>
            new RegistryAntiHallucinationValidator(sp.GetRequiredKeyedService<RootCauseDbContext>("readonly"))));

        return services;
    }
}
