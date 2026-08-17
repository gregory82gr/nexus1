using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

public sealed class GetMonitorsWithCalibrationDueQueryHandler(IMonitorsWithCalibrationDueFinder finder)
    : IQueryHandler<GetMonitorsWithCalibrationDueQuery, IReadOnlyList<MonitorCalibrationDueDto>>
{
    public async Task<Result<IReadOnlyList<MonitorCalibrationDueDto>>> Handle(GetMonitorsWithCalibrationDueQuery query, CancellationToken cancellationToken)
    {
        var monitors = await finder.GetMonitorsWithCalibrationDueAsync(cancellationToken);
        return Result<IReadOnlyList<MonitorCalibrationDueDto>>.Success(monitors);
    }
}
