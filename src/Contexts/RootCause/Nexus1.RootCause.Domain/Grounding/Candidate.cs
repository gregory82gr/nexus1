namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// One ranked candidate considered in a DiagnosisRun (From Flood to Cause,
/// App C.2) -- the node-test output: role, weight, and coverage (share of the
/// flood explained), all derived by the graph walker (ADR-042). For EVT-2026-0418
/// the ruled-out FT-7 candidate is kept as a row with Role = "ruled-out", never
/// discarded -- the book's "a system that cannot show what it dismissed cannot be
/// audited". Weight is an attribution share, not a probability identified from
/// plant data (the book's own repeated caveat).
/// </summary>
public sealed class Candidate
{
    /// <summary>Settable, not init: the run-store assigns it after the parent DiagnosisRun's identity is generated.</summary>
    public long DiagnosisRunId { get; set; }

    public int ComponentId { get; init; }

    /// <summary>origin | downstream | parallel | contributing | independent | ruled-out (ADR-042).</summary>
    public required string Role { get; init; }

    /// <summary>Attribution share 0..1: the share of the flood this candidate newly explained in the greedy ranking (ADR-042).</summary>
    public double Weight { get; init; }

    /// <summary>Share of the flood this candidate explains (node test), nullable.</summary>
    public double? Coverage { get; init; }
}
