using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Contracts.RootCause;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Host;

/// <summary>
/// Explicit, one-off dev provisioning for the EVT-2026-0418 fixture (ADR-034),
/// invoked with <c>dotnet run -- provision</c> -- NOT part of normal startup and
/// NOT on the request path. It seeds the grounding rows and populates corpus
/// embeddings (via the served embedder), then exits. It does no DDL: schema
/// migrations remain the developer's own <c>dotnet ef database update</c> step
/// (the host's nexus1_app login cannot create tables). Idempotent: seeding
/// no-ops when rows exist, ingest only fills missing embeddings. See
/// docs/runbooks/local-rootcause-diagnosis-provisioning.md.
/// </summary>
public static class Provisioning
{
    public static async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RootCauseDbContext>();

        var alreadyMigrated = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).Any();
        if (!alreadyMigrated)
        {
            logger.LogWarning("RootCauseDb has no applied migrations. Run 'dotnet ef database update' (as the developer) before provisioning.");
        }

        await GroundingSeed.SeedAsync(db, cancellationToken);
        var componentCount = await db.Components.CountAsync(cancellationToken);
        var corpusCount = await db.CorpusChunks.CountAsync(cancellationToken);
        logger.LogInformation("Seed complete: {Components} components, {Corpus} corpus chunks.", componentCount, corpusCount);

        var ingestor = scope.ServiceProvider.GetRequiredService<EmbeddingIngestor>();
        var embedded = await ingestor.IngestAsync(cancellationToken);
        var remaining = await db.CorpusChunks.CountAsync(c => c.EmbeddingJson == null, cancellationToken);
        logger.LogInformation("Embedding ingest complete: {Embedded} newly embedded, {Remaining} still missing an embedding.", embedded, remaining);

        if (remaining > 0)
        {
            logger.LogWarning("Some corpus chunks still lack embeddings -- is Ollama running with the embedding model? The semantic retrieval path will stay dormant for those.");
        }

        // Same scoped DbContext as `db`, so the aggregate + outbox rows save together.
        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();
        await ProvisionQaEscapeCaseAsync(db, outbox, logger, cancellationToken);
    }

    // ----- EVT-2026-0420, the QA-escape (ADR-040) -----

    /// <summary>
    /// Provisions the EVT-2026-0420 case (ADR-040): a provenance-originated RootCauseAnalysis
    /// (no flood) recorded as Inconclusive with a narrative reason -- never a synthesized
    /// verdict, because the engine genuinely cannot solve it. The identity and construction
    /// come from <see cref="Evt20260420QaEscape"/>, shared with the H9 harness (ADR-041), so the
    /// harness asserts exactly what is provisioned here. Idempotent: skips if the fixed case
    /// already exists. Enqueues CaseOpened (null flood) + CaseInconclusive to the outbox so
    /// Reporting projects it; the running host's outbox relay publishes them.
    /// </summary>
    private static async Task ProvisionQaEscapeCaseAsync(RootCauseDbContext db, IOutboxWriter outbox, ILogger logger, CancellationToken cancellationToken)
    {
        var id = new RootCauseAnalysisId(Evt20260420QaEscape.AnalysisId);
        if (await db.RootCauseAnalyses.AnyAsync(a => a.Id == id, cancellationToken))
        {
            logger.LogInformation("EVT-2026-0420 QA-escape case already provisioned (AnalysisId {AnalysisId}).", Evt20260420QaEscape.AnalysisId);
            return;
        }

        await db.RootCauseAnalyses.AddAsync(Evt20260420QaEscape.BuildCase(), cancellationToken);

        // Both integration events, so Reporting creates the row then finalizes it as
        // Inconclusive (routing keys/event types per ADR-008; AlarmFloodId null, ADR-040).
        outbox.Enqueue(
            "nexus1.root-cause.root-cause-case-opened.v1", schemaVersion: 1, "root-cause.root-cause-case-opened.v1", Evt20260420QaEscape.OpenedAtUtc,
            new RootCauseCaseOpenedV1(Evt20260420QaEscape.AnalysisId, Evt20260420QaEscape.UnitId, AlarmFloodId: null, Evt20260420QaEscape.OpenedAtUtc));
        outbox.Enqueue(
            "nexus1.root-cause.root-cause-case-inconclusive.v1", schemaVersion: 1, "root-cause.root-cause-case-inconclusive.v1", Evt20260420QaEscape.OpenedAtUtc,
            new RootCauseCaseInconclusiveV1(Evt20260420QaEscape.AnalysisId, Evt20260420QaEscape.UnitId, AlarmFloodId: null, Evt20260420QaEscape.Reason, Evt20260420QaEscape.OpenedAtUtc));

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Provisioned EVT-2026-0420 QA-escape case as Inconclusive (AnalysisId {AnalysisId}, unit {UnitId}).", Evt20260420QaEscape.AnalysisId, Evt20260420QaEscape.UnitId);
    }
}
