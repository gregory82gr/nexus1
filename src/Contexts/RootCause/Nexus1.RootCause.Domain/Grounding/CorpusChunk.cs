namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// The documentation corner (From Flood to Cause, App C.3 / Ch.7): one
/// retrievable chunk with a trust tier. Two deliberate skeleton choices,
/// recorded in ADR-032:
///
/// 1. SourceLabel is honest about what this corpus actually is -- the book's
///    own EVT-2026-0418 worked-example passages ("From Flood to Cause,
///    companion-book worked example, Ch.N"), never fabricated plant
///    procedures or vendor manuals this project does not have. The citation
///    line the (future) explain step emits will therefore cite the book, not
///    a fictional plant document.
/// 2. EmbeddingJson is the "plain column" holding a float[] as JSON. It is
///    LEFT NULL until the embedding model (nomic-embed-text via Ollama) is
///    available -- that model is part of the deferred LLM half. The lexical
///    retrieval path needs no embedding; the cosine path's math is unit-tested
///    with synthetic vectors, but real semantic ranking waits on this column
///    being populated. No native SQL Server VECTOR type is used (ADR-032).
/// </summary>
public sealed class CorpusChunk
{
    public long ChunkId { get; init; }

    public int DocId { get; init; }

    /// <summary>Trust tier (0 = highest), App C.3 TrustTier TINYINT.</summary>
    public byte TrustTier { get; init; }

    public required string Body { get; init; }

    /// <summary>Honest provenance of this chunk -- the book worked example, named as such.</summary>
    public required string SourceLabel { get; init; }

    /// <summary>float[] as JSON; null until the embedding model is available (deferred LLM half).</summary>
    public string? EmbeddingJson { get; init; }
}
