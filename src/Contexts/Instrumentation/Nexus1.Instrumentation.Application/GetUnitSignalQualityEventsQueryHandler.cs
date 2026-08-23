using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

public sealed class GetUnitSignalQualityEventsQueryHandler(IOpenSignalQualityEventFinder finder)
    : IQueryHandler<GetUnitSignalQualityEventsQuery, IReadOnlyList<OpenSignalQualityEventDto>>
{
    public async Task<Result<IReadOnlyList<OpenSignalQualityEventDto>>> Handle(
        GetUnitSignalQualityEventsQuery query, CancellationToken cancellationToken)
    {
        var events = await finder.GetOpenByUnitIdAsync(query.UnitId, cancellationToken);
        return Result<IReadOnlyList<OpenSignalQualityEventDto>>.Success(events);
    }
}
