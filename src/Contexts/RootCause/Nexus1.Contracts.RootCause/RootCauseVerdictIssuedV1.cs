namespace Nexus1.Contracts.RootCause;

/// <summary>
/// Adapted, not frozen, payload — see ADR-005's amendment. Carries only
/// what RootCauseAnalysis actually has. Published by
/// CloseAnalysisCommandHandler via RootCause's own outbox (ADR-010) —
/// consumed by Audit's inbox as of step 8.
/// </summary>
public sealed record RootCauseVerdictIssuedV1(long AnalysisId, int UnitId, long AlarmFloodId, string Verdict, DateTime IssuedAtUtc);
