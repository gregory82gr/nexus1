using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

public sealed record GetUnitRoboticsOverviewQuery(int UnitId) : IQuery<UnitRoboticsOverviewDto>;
