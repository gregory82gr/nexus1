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
