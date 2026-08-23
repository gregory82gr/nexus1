namespace Nexus1.Robotics.Application;

/// <summary>
/// Mission-summary level only — code, title, type/status/priority, timing.
/// Does NOT include per-mission readiness-item detail or event timeline
/// (GetBlockingReadinessFailuresQuery/GetMissionTimelineQuery, both scoped
/// by a specific MissionId) — those are mission-detail drill-down screens,
/// out of scope for a unit-level overview. See IUnitMissionsFinder's doc
/// comment.
/// </summary>
public sealed record UnitMissionDto(
    string MissionCode,
    string Title,
    string MissionType,
    string MissionStatus,
    string MissionPriority,
    DateTime RequestedAtUtc,
    DateTime? PlannedStartUtc,
    DateTime? PlannedEndUtc,
    DateTime? ActualStartUtc,
    DateTime? ActualEndUtc);
