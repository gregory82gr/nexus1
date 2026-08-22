namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 1, verbatim: robots currently available by unit.</summary>
public sealed record AvailableRobotDto(string UnitCode, string RobotCode, string Name, string Status);
