using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 3, verbatim: mission timeline for one mission, ordered by OccurredAtUtc.</summary>
public sealed record GetMissionTimelineQuery(long MissionId) : IQuery<IReadOnlyList<MissionTimelineEntryDto>>;
