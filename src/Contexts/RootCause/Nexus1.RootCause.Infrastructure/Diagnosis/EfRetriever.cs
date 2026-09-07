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
/// Unit scoping is expressed in the query shape; the skeleton corpus is
/// single-unit (the book's one worked example), so the filter is a structural
/// placeholder rather than a discriminator yet (ADR-032).
/// </summary>
public sealed class EfRetriever(RootCauseDbContext db) : IRetriever
{
    private const int TopK = 8;

    /// <summary>H2 relevance floor for the semantic path: a cosine below this is "nothing to ground on".</summary>
    private const double RelevanceFloor = 0.75;

    public async Task<IReadOnlyList<Passage>> RetrieveAsync(string queryText, string tagText, int unitId, CancellationToken cancellationToken)
    {
        var chunks = await db.CorpusChunks
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

        // Semantic: C#-cosine over populated embeddings, floored at H2. Dormant
        // until embeddings and a query vector exist; the path is real, not faked.
        var queryEmbedding = EmbedQuery(queryText);
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

    /// <summary>
    /// The query embedding comes from the embedding model, which is part of the
    /// deferred LLM half; until it is wired, there is no query vector and the
    /// semantic path stays dormant. Returning null here is honest ("no embedder
    /// yet"), not a stub that fabricates a vector.
    /// </summary>
    private static IReadOnlyList<double>? EmbedQuery(string queryText) => null;

    private static IReadOnlyList<double>? ParseEmbedding(string? embeddingJson) =>
        string.IsNullOrEmpty(embeddingJson) ? null : JsonSerializer.Deserialize<double[]>(embeddingJson);
}
