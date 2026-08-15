namespace Nexus1.RootCause.Domain;

/// <summary>
/// The seed for the future RootCauseVerdictIssued.v1 integration event —
/// translation is Application/Host-layer work, not decided here (ADR-005).
/// </summary>
public sealed record RootCauseAnalysisClosed(RootCauseAnalysisId AnalysisId, string Verdict, DateTime ClosedAtUtc);
