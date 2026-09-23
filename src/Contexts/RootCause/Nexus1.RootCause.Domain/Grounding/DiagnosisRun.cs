namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// The engine's own deterministic run record (From Flood to Cause, App C.2) --
/// distinct from the human-owned RootCauseAnalysis aggregate, which stays
/// untouched (ADR-005 separates the engineered result from the human case;
/// ADR-032 keeps that separation for the skeleton). One incident, one run,
/// one verdict OR an abstention with a named reason -- never a silent verdict,
/// matching this project's Inconclusive-is-never-a-silent-Pass discipline.
/// </summary>
public sealed class DiagnosisRun
{
    public long DiagnosisRunId { get; init; }

    /// <summary>e.g. "EVT-2026-0418".</summary>
    public required string IncidentId { get; init; }

    public DateTime StartedAtUtc { get; init; }

    /// <summary>Origin component tag when a verdict is reached; null when abstained.</summary>
    public string? Verdict { get; set; }

    /// <summary>Non-null exactly when the run abstained (never both a verdict and a reason).</summary>
    public string? AbstainReason { get; set; }

    /// <summary>What the run could read -- recorded for audit (App C.2 CorpusVersion, H10).</summary>
    public required string CorpusVersion { get; init; }
}
