namespace Nexus1.Contracts.AlarmManagement;

/// <summary>
/// Adapted, not frozen, payload — see ADR-004's amendment. Carries only
/// what AlarmFlood actually has (Phase 1); the book's fuller contract
/// (SiteId/LineId, PolicyId, window end, alarm membership) is deferred.
/// Wire envelope/routing/naming conventions still match the book exactly
/// (ADR-008) — only this payload's field list is reduced.
/// </summary>
public sealed record AlarmFloodDetectedV1(long AlarmFloodId, int UnitId, DateTime StartedAtUtc);
