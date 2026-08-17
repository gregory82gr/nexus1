using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>
/// A physical robot asset (ADR-023). Carries its own lifecycle
/// (RobotStatusId) and model reference (RobotModelId -> RobotModel ->
/// RobotTypeId -> RobotType) as real internal invariants, both NOT NULL.
///
/// HomeUnitId is a real SQL FOREIGN KEY to ReactorFleet.Unit via the
/// ReactorFleetUnitReference shadow-entity technique (ADR-023), but nullable
/// — not every robot has a fixed home unit.
///
/// HomeDockingStationId is deliberately omitted entirely, not merely
/// downgraded to passport-only: its target table (the docking group) is out
/// of scope for this pass and does not exist in this codebase at all
/// (ADR-023).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only, same treatment as RobotModel/Mission (ADR-023).
/// </summary>
public sealed class Robot : Entity<RobotId>, IAggregateRoot
{
    private Robot(
        RobotId id, RobotModelId robotModelId, RobotStatusId robotStatusId, int? homeUnitId, string code,
        string name, string? serialNumber, DateTime? commissionedAtUtc, DateTime? retiredAtUtc, DateTime? lastSeenAtUtc)
        : base(id)
    {
        RobotModelId = robotModelId;
        RobotStatusId = robotStatusId;
        HomeUnitId = homeUnitId;
        Code = code;
        Name = name;
        SerialNumber = serialNumber;
        CommissionedAtUtc = commissionedAtUtc;
        RetiredAtUtc = retiredAtUtc;
        LastSeenAtUtc = lastSeenAtUtc;
    }

    public RobotModelId RobotModelId { get; }

    public RobotStatusId RobotStatusId { get; }

    /// <summary>Real FK to ReactorFleet.Unit (ADR-023), nullable — not every robot has a fixed home unit.</summary>
    public int? HomeUnitId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? SerialNumber { get; }

    public DateTime? CommissionedAtUtc { get; }

    public DateTime? RetiredAtUtc { get; }

    public DateTime? LastSeenAtUtc { get; }

    public static Robot Create(
        RobotId id, RobotModelId robotModelId, RobotStatusId robotStatusId, string code, string name,
        int? homeUnitId = null, string? serialNumber = null, DateTime? commissionedAtUtc = null,
        DateTime? retiredAtUtc = null, DateTime? lastSeenAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Robot code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Robot name must not be empty.", nameof(name));
        }

        return new Robot(
            id, robotModelId, robotStatusId, homeUnitId, code, name, serialNumber, commissionedAtUtc,
            retiredAtUtc, lastSeenAtUtc);
    }
}
