using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 2, verbatim: one row per robot, most recent RobotHealthSnapshot, with battery/communication status codes.</summary>
public sealed record GetLatestHealthSnapshotPerRobotQuery : IQuery<IReadOnlyList<RobotHealthSnapshotDto>>;
