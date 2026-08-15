namespace Nexus1.RootCause.Domain;

public sealed record RootCauseAnalysisOpened(RootCauseAnalysisId AnalysisId, UnitId UnitId, AlarmFloodId AlarmFloodId, DateTime OpenedAtUtc);
