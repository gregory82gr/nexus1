namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// Cosine similarity computed in C# (ADR-032) -- deliberately NOT SQL Server
/// 2025's native VECTOR_DISTANCE, which the walking skeleton's tiny corpus
/// does not warrant and which would force a major-version engine migration.
/// Pure and side-effect-free, so it is unit-tested directly with synthetic
/// vectors. The real corpus embeddings it would rank over stay unpopulated
/// until the embedding model (nomic-embed-text via Ollama) is available -- so
/// this is proven math waiting on real inputs, not a live retrieval path yet.
/// </summary>
public static class CosineSimilarity
{
    /// <summary>Cosine of the angle between two equal-length vectors; 0 for a zero vector or a length mismatch.</summary>
    public static double Between(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        if (a.Count == 0 || a.Count != b.Count)
        {
            return 0d;
        }

        double dot = 0d, normA = 0d, normB = 0d;
        for (var i = 0; i < a.Count; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0d || normB == 0d)
        {
            return 0d;
        }

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
