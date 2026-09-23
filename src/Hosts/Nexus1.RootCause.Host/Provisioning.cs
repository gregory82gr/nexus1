using Microsoft.EntityFrameworkCore;
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
    }
}
