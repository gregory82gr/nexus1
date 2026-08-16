using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

public sealed class GetOpenSignalQualityEventsForUnitQueryHandler(IOpenSignalQualityEventFinder finder)
    : IQueryHandler<GetOpenSignalQualityEventsForUnitQuery, IReadOnlyList<OpenSignalQualityEventDto>>
{
    public async Task<Result<IReadOnlyList<OpenSignalQualityEventDto>>> Handle(
        GetOpenSignalQualityEventsForUnitQuery query, CancellationToken cancellationToken)
    {
        var events = await finder.GetOpenByUnitCodeAsync(query.UnitCode, cancellationToken);
        return Result<IReadOnlyList<OpenSignalQualityEventDto>>.Success(events);
    }
}
