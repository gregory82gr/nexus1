namespace Nexus1.Robotics.Application;

/// <summary>
/// New finder — no existing Application-layer query lists missions at all;
/// GetMissionTimelineQuery/GetBlockingReadinessFailuresQuery are both scoped
/// to one already-known MissionId, not a unit. Added for the BFF's Mission
/// Readiness screen at mission-summary level (see UnitMissionDto's doc
/// comment for the named boundary).
/// </summary>
public interface IUnitMissionsFinder
{
    Task<IReadOnlyList<UnitMissionDto>> GetMissionsForUnitAsync(int unitId, CancellationToken cancellationToken);
}
