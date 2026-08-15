namespace Nexus1.Reporting.Domain;

/// <summary>Wraps the source AnalysisId directly — this project's natural case identity, no separate RootCauseCaseId/VerdictId split to key by (ADR-012).</summary>
public readonly record struct RootCauseCaseSummaryId(long Value);
