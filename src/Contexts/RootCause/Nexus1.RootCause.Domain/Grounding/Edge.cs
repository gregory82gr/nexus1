namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// One engineered causal edge (From Flood to Cause, App C.1 / Ch.2). Kind
/// records provenance: backbone (fault-tree), learned (data-informed weight),
/// artefact (EMI-class), rejected (ruled-out/absent). The delay window is the
/// modelled propagation time; the walker follows an edge only when the observed
/// sequence fits [DelayMinSeconds, DelayMaxSeconds].
///
/// For EVT-2026-0418 the console reproduces point delays (36s, 18s, ...), so the
/// skeleton seeds Min == Max == the modelled point delay -- an exact window, not
/// an invented tolerance band (ADR-032). A rejected edge carries no meaningful
/// window and is never walked.
/// </summary>
public sealed class Edge
{
    public int EdgeId { get; init; }

    public int FromComponentId { get; init; }

    public int ToComponentId { get; init; }

    /// <summary>backbone | learned | artefact | rejected (App C.1 Kind).</summary>
    public required string Kind { get; init; }

    public int DelayMinSeconds { get; init; }

    public int DelayMaxSeconds { get; init; }

    /// <summary>Fault-tree / document provenance for the edge (App C.1 SourceRef).</summary>
    public string? SourceRef { get; init; }
}
