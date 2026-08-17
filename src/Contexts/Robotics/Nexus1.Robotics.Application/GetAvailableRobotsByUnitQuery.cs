using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 1, verbatim: robots with RobotStatus.Code = 'AVAILABLE', joined to ReactorFleet.Unit for the unit code.</summary>
public sealed record GetAvailableRobotsByUnitQuery : IQuery<IReadOnlyList<AvailableRobotDto>>;
