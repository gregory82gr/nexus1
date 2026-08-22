using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

/// <summary>
/// Shared seed: one ReactorFleet.Unit row (the real cross-context FK target,
/// ADR-023), the eight Robotics lookup rows, and one RobotModel/Robot pair.
/// Every component test below builds on this same shape so the six
/// Application operations are exercised against a realistic, FK-consistent
/// graph rather than isolated rows — copying EventManagementSeedHelper's own
/// precedent pattern exactly (ADR-022/ADR-023).
/// </summary>
internal static class RoboticsSeedHelper
{
    public const string UnitCode = "NX1-U1";
    public const string RobotCode = "RBT-001";

    public sealed record SeedResult(
        int UnitId, int RobotTypeId, int RobotStatusAvailableId, int RobotStatusOtherId, int BatteryStatusId,
        int CommunicationStatusId, int MissionTypeId, int MissionStatusId, int MissionPriorityId,
        int ReadinessStatusBlockedId, int ReadinessStatusExpiredId, int ReadinessStatusOkId, int RobotModelId,
        int RobotId);

    public static async Task<SeedResult> SeedCoreAsync(
        ReactorFleetDbContext reactorFleetContext, RoboticsDbContext roboticsContext, DateTime nowUtc)
    {
        var unit = Unit.Create(new Nexus1.ReactorFleet.Domain.UnitId(1), UnitCode, "Unit 1");
        await reactorFleetContext.Units.AddAsync(unit);
        await reactorFleetContext.SaveChangesAsync();

        var robotType = RobotType.Create(new RobotTypeId(1), "INSPECTION", "Inspection Robot", nowUtc);
        var robotStatusAvailable = RobotStatus.Create(new RobotStatusId(1), "AVAILABLE", "Available", nowUtc);
        var robotStatusOther = RobotStatus.Create(new RobotStatusId(2), "MAINTENANCE", "In Maintenance", nowUtc);
        var batteryStatus = BatteryStatus.Create(new BatteryStatusId(1), "NOMINAL", "Nominal", nowUtc);
        var communicationStatus = CommunicationStatus.Create(new CommunicationStatusId(1), "CONNECTED", "Connected", nowUtc);
        var missionType = MissionType.Create(new MissionTypeId(1), "INSPECTION", "Inspection", nowUtc);
        var missionStatus = MissionStatus.Create(new MissionStatusId(1), "REQUESTED", "Requested", nowUtc);
        var missionPriority = MissionPriority.Create(new MissionPriorityId(1), "ROUTINE", "Routine", nowUtc);
        var readinessStatusBlocked = ReadinessStatus.Create(new ReadinessStatusId(1), "BLOCKED", "Blocked", nowUtc);
        var readinessStatusExpired = ReadinessStatus.Create(new ReadinessStatusId(2), "EXPIRED", "Expired", nowUtc);
        var readinessStatusOk = ReadinessStatus.Create(new ReadinessStatusId(3), "READY", "Ready", nowUtc);

        await roboticsContext.RobotTypes.AddAsync(robotType);
        await roboticsContext.RobotStatuses.AddAsync(robotStatusAvailable);
        await roboticsContext.RobotStatuses.AddAsync(robotStatusOther);
        await roboticsContext.BatteryStatuses.AddAsync(batteryStatus);
        await roboticsContext.CommunicationStatuses.AddAsync(communicationStatus);
        await roboticsContext.MissionTypes.AddAsync(missionType);
        await roboticsContext.MissionStatuses.AddAsync(missionStatus);
        await roboticsContext.MissionPriorities.AddAsync(missionPriority);
        await roboticsContext.ReadinessStatuses.AddAsync(readinessStatusBlocked);
        await roboticsContext.ReadinessStatuses.AddAsync(readinessStatusExpired);
        await roboticsContext.ReadinessStatuses.AddAsync(readinessStatusOk);
        await roboticsContext.SaveChangesAsync();

        var robotModel = RobotModel.Create(
            new RobotModelId(1), robotType.Id, "INSP-2000", "Acme Robotics", "Inspector 2000");
        await roboticsContext.RobotModels.AddAsync(robotModel);
        await roboticsContext.SaveChangesAsync();

        var robot = Robot.Create(
            new RobotId(1), robotModel.Id, robotStatusAvailable.Id, RobotCode, "Scout One", homeUnitId: unit.Id.Value);
        await roboticsContext.Robots.AddAsync(robot);
        await roboticsContext.SaveChangesAsync();

        return new SeedResult(
            unit.Id.Value, robotType.Id.Value, robotStatusAvailable.Id.Value, robotStatusOther.Id.Value,
            batteryStatus.Id.Value, communicationStatus.Id.Value, missionType.Id.Value, missionStatus.Id.Value,
            missionPriority.Id.Value, readinessStatusBlocked.Id.Value, readinessStatusExpired.Id.Value,
            readinessStatusOk.Id.Value, robotModel.Id.Value, robot.Id.Value);
    }
}
