namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// One ranked candidate considered in a DiagnosisRun (From Flood to Cause,
/// App C.2) -- the node-test output: role, illustrative weight, and coverage
/// (share of the flood explained). For EVT-2026-0418 the ruled-out FT-7
/// candidate is kept as a row with Role = "ruled-out", never discarded -- the
/// book's "a system that cannot show what it dismissed cannot be audited".
/// Weight is an illustrative figure derived from edge weights, not a
/// probability identified from plant data (the book's own repeated caveat).
/// </summary>
public sealed class Candidate
{
    /// <summary>Settable, not init: the run-store assigns it after the parent DiagnosisRun's identity is generated.</summary>
    public long DiagnosisRunId { get; set; }

    public int ComponentId { get; init; }

    /// <summary>origin | proximate | parallel | contributing | ruled-out.</summary>
    public required string Role { get; init; }

    /// <summary>Illustrative causal weight 0..1, derived from edge weights.</summary>
    public double Weight { get; init; }

    /// <summary>Share of the flood this candidate explains (node test), nullable.</summary>
    public double? Coverage { get; init; }
}
