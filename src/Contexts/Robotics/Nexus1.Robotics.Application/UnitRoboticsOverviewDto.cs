namespace Nexus1.Robotics.Application;

/// <summary>Shaped for the BFF's Robotics Fleet Overview / Mission Readiness screens (per unit).</summary>
public sealed record UnitRoboticsOverviewDto(
    int UnitId,
    IReadOnlyList<UnitRobotStatusDto> Robots,
    IReadOnlyList<UnitMissionDto> Missions);
