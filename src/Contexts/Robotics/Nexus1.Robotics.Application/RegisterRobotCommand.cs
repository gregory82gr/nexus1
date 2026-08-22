using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

/// <summary>Robot's defining behavior (ADR-023): registers a new robot asset against a model and status.</summary>
public sealed record RegisterRobotCommand(
    int RobotModelId, int RobotStatusId, string Code, string Name, int? HomeUnitId = null,
    string? SerialNumber = null, DateTime? CommissionedAtUtc = null, DateTime? RetiredAtUtc = null,
    DateTime? LastSeenAtUtc = null)
    : ICommand<int>;
