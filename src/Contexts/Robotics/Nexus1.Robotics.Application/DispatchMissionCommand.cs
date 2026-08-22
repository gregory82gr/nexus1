using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

/// <summary>Mission's defining behavior (ADR-023): dispatches a new mission against a unit, type, status and priority.</summary>
public sealed record DispatchMissionCommand(
    int UnitId, int MissionTypeId, int MissionStatusId, int MissionPriorityId, string Code, string Title,
    DateTime RequestedAtUtc, string? Objective = null, DateTime? PlannedStartUtc = null,
    DateTime? PlannedEndUtc = null, DateTime? ActualStartUtc = null, DateTime? ActualEndUtc = null,
    int? RequestedByUserId = null, int? ApprovedByUserId = null)
    : ICommand<long>;
