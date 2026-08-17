using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

public sealed class GetEventWithAlarmsAndFloodQueryHandler(IEventWithAlarmsAndFloodFinder finder)
    : IQueryHandler<GetEventWithAlarmsAndFloodQuery, EventWithAlarmsAndFloodDto?>
{
    public async Task<Result<EventWithAlarmsAndFloodDto?>> Handle(GetEventWithAlarmsAndFloodQuery query, CancellationToken cancellationToken)
    {
        var dto = await finder.GetByEventCodeAsync(query.EventCode, cancellationToken);
        return Result<EventWithAlarmsAndFloodDto?>.Success(dto);
    }
}
