using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// One-time provisioning: populate <c>Corpus.EmbeddingJson</c> for chunks that
/// have none, using the injected embedder (nomic-embed-text via Ollama when the
/// Explain project is wired). This is a WRITE, so it runs under a write-capable
/// connection at provisioning time -- deliberately NOT the read-only
/// nexus1_explain login the runtime explain path uses (H7). It is LLM-free code:
/// it depends on the IEmbedder interface, not on any served-model package, so it
/// can live on the deterministic side; the actual embedding call happens inside
/// the injected implementation (ADR-033).
/// </summary>
public sealed class EmbeddingIngestor(RootCauseDbContext db, IEmbedder embedder)
{
    /// <summary>Embeds every corpus chunk missing an embedding; returns how many were populated.</summary>
    public async Task<int> IngestAsync(CancellationToken cancellationToken = default)
    {
        var pending = await db.CorpusChunks
            .Where(c => c.EmbeddingJson == null)
            .ToListAsync(cancellationToken);

        var populated = 0;
        foreach (var chunk in pending)
        {
            var vector = await embedder.EmbedAsync(chunk.Body, EmbedKind.Document, cancellationToken);
            if (vector is null)
            {
                // No embedding model available (NoOp embedder) -- leave null and
                // stop, rather than write an empty or fabricated vector.
                break;
            }

            chunk.EmbeddingJson = JsonSerializer.Serialize(vector);
            populated++;
        }

        if (populated > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return populated;
    }
}
