using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

public sealed class GetEventTimelineQueryHandler(IEventTimelineFinder finder)
    : IQueryHandler<GetEventTimelineQuery, IReadOnlyList<EventTimelineEntryDto>>
{
    public async Task<Result<IReadOnlyList<EventTimelineEntryDto>>> Handle(GetEventTimelineQuery query, CancellationToken cancellationToken)
    {
        var timeline = await finder.GetTimelineAsync(query.OperationalEventId, cancellationToken);
        return Result<IReadOnlyList<EventTimelineEntryDto>>.Success(timeline);
    }
}
