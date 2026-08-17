using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 4, verbatim: MissionReadinessItem rows where IsBlocking = 1 and status is BLOCKED/EXPIRED.</summary>
public sealed record GetBlockingReadinessFailuresQuery(long MissionId) : IQuery<IReadOnlyList<ReadinessFailureDto>>;
