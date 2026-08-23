using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

public sealed class GetUnitSignalReadingsQueryHandler(IActiveHistorizedSignalFinder finder)
    : IQueryHandler<GetUnitSignalReadingsQuery, IReadOnlyList<UnitSignalReadingDto>>
{
    public async Task<Result<IReadOnlyList<UnitSignalReadingDto>>> Handle(
        GetUnitSignalReadingsQuery query, CancellationToken cancellationToken)
    {
        var readings = await finder.GetSignalReadingsForUnitAsync(query.UnitId, cancellationToken);
        return Result<IReadOnlyList<UnitSignalReadingDto>>.Success(readings);
    }
}
