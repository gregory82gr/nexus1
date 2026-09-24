namespace Nexus1.Contracts.RootCause;

/// <summary>
/// Adapted, not frozen, payload (ADR-039) — same reduction pattern as
/// RootCauseVerdictIssuedV1 and RootCauseCaseOpenedV1: carries only what the aggregate
/// has. Deliberately has NO Verdict field — it carries the Reason instead, so a case
/// that reached no conclusion is encoded structurally rather than as an empty verdict
/// (this project's "Inconclusive is never a silent Pass"). Published by
/// MarkAnalysisInconclusiveCommandHandler via RootCause's existing outbox; consumed by
/// Reporting's projection. Audit and Compliance bind the verdict routing key only, so
/// they never receive this (a stated, reversible deferral — ADR-039).
/// </summary>
public sealed record RootCauseCaseInconclusiveV1(long AnalysisId, int UnitId, long AlarmFloodId, string Reason, DateTime DecidedAtUtc);
