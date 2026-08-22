using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

public sealed class GetMissionTimelineQueryHandler(IMissionTimelineFinder finder)
    : IQueryHandler<GetMissionTimelineQuery, IReadOnlyList<MissionTimelineEntryDto>>
{
    public async Task<Result<IReadOnlyList<MissionTimelineEntryDto>>> Handle(GetMissionTimelineQuery query, CancellationToken cancellationToken)
    {
        var timeline = await finder.GetTimelineAsync(query.MissionId, cancellationToken);
        return Result<IReadOnlyList<MissionTimelineEntryDto>>.Success(timeline);
    }
}
