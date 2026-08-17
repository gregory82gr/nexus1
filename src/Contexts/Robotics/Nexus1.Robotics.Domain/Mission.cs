using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>
/// The dispatch header for a robotics mission (ADR-023). UnitId is a real
/// SQL FOREIGN KEY to ReactorFleet.Unit via the ReactorFleetUnitReference
/// shadow-entity technique. MissionTypeId/MissionStatusId/MissionPriorityId
/// are all real internal FKs and NOT NULL, matching the atlas's own
/// three-way lookup chain.
///
/// RequestedByUserId/ApprovedByUserId are passport-only —
/// Security.ApplicationUser lives in SecurityDb, a different physical
/// database than this sector's chosen home (AlarmManagementDb); no
/// enforced FK.
///
/// PlannedEndUtc >= PlannedStartUtc and ActualEndUtc >= ActualStartUtc (when
/// both present) are enforced as SQL CHECK constraints at the EF
/// configuration layer, not in Domain (ADR-023).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only, same treatment as RobotModel/Robot (ADR-023).
/// </summary>
public sealed class Mission : Entity<MissionId>, IAggregateRoot
{
    private Mission(
        MissionId id, int unitId, MissionTypeId missionTypeId, MissionStatusId missionStatusId,
        MissionPriorityId missionPriorityId, string code, string title, string? objective, DateTime requestedAtUtc,
        DateTime? plannedStartUtc, DateTime? plannedEndUtc, DateTime? actualStartUtc, DateTime? actualEndUtc,
        int? requestedByUserId, int? approvedByUserId)
        : base(id)
    {
        UnitId = unitId;
        MissionTypeId = missionTypeId;
        MissionStatusId = missionStatusId;
        MissionPriorityId = missionPriorityId;
        Code = code;
        Title = title;
        Objective = objective;
        RequestedAtUtc = requestedAtUtc;
        PlannedStartUtc = plannedStartUtc;
        PlannedEndUtc = plannedEndUtc;
        ActualStartUtc = actualStartUtc;
        ActualEndUtc = actualEndUtc;
        RequestedByUserId = requestedByUserId;
        ApprovedByUserId = approvedByUserId;
    }

    /// <summary>Real FK to ReactorFleet.Unit (ADR-023).</summary>
    public int UnitId { get; }

    public MissionTypeId MissionTypeId { get; }

    public MissionStatusId MissionStatusId { get; }

    public MissionPriorityId MissionPriorityId { get; }

    public string Code { get; }

    public string Title { get; }

    public string? Objective { get; }

    public DateTime RequestedAtUtc { get; }

    public DateTime? PlannedStartUtc { get; }

    public DateTime? PlannedEndUtc { get; }

    public DateTime? ActualStartUtc { get; }

    public DateTime? ActualEndUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-023).</summary>
    public int? RequestedByUserId { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-023).</summary>
    public int? ApprovedByUserId { get; }

    public static Mission Create(
        MissionId id, int unitId, MissionTypeId missionTypeId, MissionStatusId missionStatusId,
        MissionPriorityId missionPriorityId, string code, string title, DateTime requestedAtUtc,
        string? objective = null, DateTime? plannedStartUtc = null, DateTime? plannedEndUtc = null,
        DateTime? actualStartUtc = null, DateTime? actualEndUtc = null, int? requestedByUserId = null,
        int? approvedByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Mission code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Mission title must not be empty.", nameof(title));
        }

        return new Mission(
            id, unitId, missionTypeId, missionStatusId, missionPriorityId, code, title, objective, requestedAtUtc,
            plannedStartUtc, plannedEndUtc, actualStartUtc, actualEndUtc, requestedByUserId, approvedByUserId);
    }
}
