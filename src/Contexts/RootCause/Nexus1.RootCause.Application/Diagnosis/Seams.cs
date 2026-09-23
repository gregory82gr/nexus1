namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// The four seams of the One-Truth Pipeline (From Flood to Cause, Listing 9.1).
/// Each produces evidence; none decides alone; the runner composes them. All
/// four are LLM-free -- the served model sits behind a fifth, deferred stage
/// (generation), never behind these.
/// </summary>
public interface IGraphWalker
{
    /// <summary>Node test: from every alarmed node, walk backbone edges within delay windows, rank by coverage.</summary>
    Task<GraphWalkResult> WalkAsync(IncidentContext ctx, CancellationToken cancellationToken);
}

public interface ITelemetryCorroborator
{
    /// <summary>Confirm the origin hypothesis's cascade fits the modelled delay windows against the historian.</summary>
    Task<Corroboration> CheckAsync(IncidentContext ctx, int originComponentId, CancellationToken cancellationToken);
}

public interface IRetriever
{
    /// <summary>Hybrid retrieval: exact lexical tag match plus (when embeddings exist) C#-cosine semantic ranking, scoped to the unit.</summary>
    Task<IReadOnlyList<Passage>> RetrieveAsync(string queryText, string tagText, int unitId, CancellationToken cancellationToken);
}

public interface IAntiHallucinationValidator
{
    /// <summary>H3: every named entity exists in the component registry. H4: every claim cites a retrieved passage. One miss abstains.</summary>
    Task<Validation> ValidateAsync(DraftAnswer answer, IReadOnlyList<Passage> retrieved, CancellationToken cancellationToken);
}

/// <summary>Seals a run's payload into the tamper-evident audit chain (H10) and returns the new head hash.</summary>
public interface IAuditChainWriter
{
    Task<string> AppendAsync(string payload, CancellationToken cancellationToken);
}

/// <summary>
/// The fifth stage -- natural-language generation (H1/H5/H6). Given the origin
/// the engine decided and the passages it retrieved (the Unified Context), it
/// returns a structured draft answer OR an abstention. This is the ONE seam the
/// served model sits behind; every implementation that talks to a model lives in
/// Nexus1.RootCause.Explain, never in the deterministic engine. The runner treats
/// a fixture implementation and the real model identically -- the draft is
/// validated by H3/H4 either way, and a generation-time abstention (model said so,
/// or the output failed to parse) is a real abstention, never a salvage attempt.
/// </summary>
public interface IExplainer
{
    Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken);
}

/// <summary>Whether text is being embedded as a stored document or as a search query -- asymmetric embedders (e.g. nomic-embed-text) use a different task prefix for each.</summary>
public enum EmbedKind
{
    Document,
    Query,
}

/// <summary>
/// Embeds text into a vector (nomic-embed-text via Ollama, in the Explain
/// project). The interface is LLM-free so the deterministic retriever and the
/// ingest routine can depend on it without pulling in a served-model package;
/// the default <c>NoOp</c> implementation returns null so the semantic path stays
/// dormant until the real embedder is wired.
/// </summary>
public interface IEmbedder
{
    /// <summary>The embedding vector for <paramref name="text"/>, or null when no embedding model is configured. <paramref name="kind"/> lets an asymmetric model apply the right document/query prefix.</summary>
    Task<IReadOnlyList<double>?> EmbedAsync(string text, EmbedKind kind, CancellationToken cancellationToken);
}
