namespace Nexus1.Contracts.RootCause;

/// <summary>
/// Adapted, not frozen, payload — same reduction pattern ADR-005 already
/// applied to RootCauseVerdictIssuedV1: carries only what RootCauseAnalysis
/// actually has at open time (no SiteId/LineId/InitialEvidenceCount — this
/// domain model has none of those, and Open() takes no evidence). Published
/// by both OpenAnalysisCommandHandler and AlarmFloodMessageHandler's inline
/// open (ADR-012) — RootCause's existing outbox, a second message type, not
/// a second outbox.
/// </summary>
public sealed record RootCauseCaseOpenedV1(long AnalysisId, int UnitId, long AlarmFloodId, DateTime OpenedAtUtc);
