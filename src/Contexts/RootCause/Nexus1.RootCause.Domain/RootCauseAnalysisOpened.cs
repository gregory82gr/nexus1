namespace Nexus1.RootCause.Domain;

// AlarmFloodId is nullable (ADR-040): null for a provenance-originated case with no flood.
public sealed record RootCauseAnalysisOpened(RootCauseAnalysisId AnalysisId, UnitId UnitId, AlarmFloodId? AlarmFloodId, DateTime OpenedAtUtc);
