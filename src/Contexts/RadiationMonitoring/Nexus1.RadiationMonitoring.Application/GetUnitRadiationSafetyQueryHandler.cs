using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

public sealed class GetUnitRadiationSafetyQueryHandler(
    ILatestReadingPerMonitorFinder readingFinder, IActiveRadiationZonesFinder zoneFinder)
    : IQueryHandler<GetUnitRadiationSafetyQuery, UnitRadiationSafetyDto>
{
    public async Task<Result<UnitRadiationSafetyDto>> Handle(GetUnitRadiationSafetyQuery query, CancellationToken cancellationToken)
    {
        var monitors = await readingFinder.GetLatestReadingsForUnitAsync(query.UnitId, cancellationToken);
        var zones = await zoneFinder.GetActiveRadiationZonesForUnitAsync(query.UnitId, cancellationToken);

        return Result<UnitRadiationSafetyDto>.Success(new UnitRadiationSafetyDto(query.UnitId, monitors, zones));
    }
}
