using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// Hybrid retrieval of Chapter 7 (ADR-032): exact lexical tag match plus, when
/// embeddings exist, a C#-cosine semantic rank -- scoped to the unit and floored
/// (H2) so that nothing below the relevance bar reaches the answer. Two skeleton
/// choices are deliberate and recorded in ADR-032:
///
/// 1. Cosine is computed in C# (<see cref="CosineSimilarity"/>), NOT SQL Server
///    2025's native VECTOR_DISTANCE, which the book's own listing uses but which
///    would force a major-version engine migration this tiny corpus does not
///    warrant. The math is proven with synthetic vectors; it simply has no real
///    inputs yet.
/// 2. The semantic path stays dormant until corpus embeddings are populated,
///    which needs the embedding model (nomic-embed-text via Ollama) -- part of
///    the deferred LLM half. The lexical path needs no model, so the engine can
///    ground on an exact tag today and light up the semantic path unchanged the
///    day embeddings arrive.
///
/// Unit scoping is now a real discriminator (ADR-037): with a second seeded
/// incident on its own unit, the retriever filters corpus chunks by unit so a run
/// can only ground on and cite its own incident's corpus. (Before ADR-037 the
/// corpus was single-unit and this filter was a structural placeholder.)
/// </summary>
public sealed class EfRetriever(RootCauseDbContext db, IEmbedder embedder) : IRetriever
{
    private const int TopK = 8;

    /// <summary>
    /// H2 relevance floor for the semantic path: a cosine below this is "nothing
    /// to ground on". Tuned to nomic-embed-text's cosine range with document/query
    /// task prefixes (ADR-033); a demonstrator value, not a production-calibrated
    /// one (that would come from the H9 evaluation set).
    /// </summary>
    private const double RelevanceFloor = 0.5;

    public async Task<IReadOnlyList<Passage>> RetrieveAsync(string queryText, string tagText, int unitId, CancellationToken cancellationToken)
    {
        // Unit scoping is now a real discriminator (ADR-037): a run grounds on and
        // cites only its own incident's corpus. Before the second incident existed,
        // the corpus was single-unit and this filter was a no-op placeholder.
        var chunks = await db.CorpusChunks
            .Where(c => c.UnitId == unitId)
            .OrderBy(c => c.TrustTier)
            .ThenBy(c => c.ChunkId)
            .ToListAsync(cancellationToken);

        var selected = new List<CorpusChunk>();
        var selectedIds = new HashSet<long>();

        // Lexical: exact tag match (the book's CONTAINS(Body, @tagText)).
        foreach (var chunk in chunks)
        {
            if (!string.IsNullOrEmpty(tagText) && chunk.Body.Contains(tagText, StringComparison.OrdinalIgnoreCase) && selectedIds.Add(chunk.ChunkId))
            {
                selected.Add(chunk);
            }
        }

        // Semantic: C#-cosine over populated embeddings, floored at H2. The query
        // vector comes from the injected embedder -- the NoOp default returns null
        // (dormant), the Ollama-backed one (Explain) returns a real vector.
        var queryEmbedding = await embedder.EmbedAsync(queryText, EmbedKind.Query, cancellationToken);
        if (queryEmbedding is not null)
        {
            var semantic = chunks
                .Select(c => (Chunk: c, Vector: ParseEmbedding(c.EmbeddingJson)))
                .Where(x => x.Vector is not null)
                .Select(x => (x.Chunk, Score: CosineSimilarity.Between(queryEmbedding, x.Vector!)))
                .Where(x => x.Score >= RelevanceFloor)
                .OrderByDescending(x => x.Score);

            foreach (var (chunk, _) in semantic)
            {
                if (selectedIds.Add(chunk.ChunkId))
                {
                    selected.Add(chunk);
                }
            }
        }

        return selected
            .Take(TopK)
            .Select(c => new Passage(c.ChunkId, c.Body, c.SourceLabel, c.TrustTier))
            .ToList();
    }

    private static IReadOnlyList<double>? ParseEmbedding(string? embeddingJson) =>
        string.IsNullOrEmpty(embeddingJson) ? null : JsonSerializer.Deserialize<double[]>(embeddingJson);
}
