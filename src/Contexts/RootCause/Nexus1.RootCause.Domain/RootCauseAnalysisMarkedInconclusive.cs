namespace Nexus1.RootCause.Domain;

/// <summary>
/// Raised when a case is terminally marked Inconclusive (ADR-039) — the seed for the
/// RootCauseCaseInconclusive.v1 integration event, translated in the Application/Host
/// layer, the same split as RootCauseAnalysisClosed. Carries a reason, never a verdict.
/// </summary>
public sealed record RootCauseAnalysisMarkedInconclusive(RootCauseAnalysisId AnalysisId, string Reason, DateTime DecidedAtUtc);
