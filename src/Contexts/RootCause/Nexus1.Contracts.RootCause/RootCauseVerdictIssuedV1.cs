namespace Nexus1.Contracts.RootCause;

/// <summary>
/// Adapted, not frozen, payload — see ADR-005's amendment. Carries only
/// what RootCauseAnalysis actually has; not yet published anywhere (no
/// consumer exists in Phase 1 — Audit/Compliance/Reporting don't have
/// projects yet, CLAUDE.md §2).
/// </summary>
public sealed record RootCauseVerdictIssuedV1(long AnalysisId, int UnitId, long AlarmFloodId, string Verdict, DateTime IssuedAtUtc);
