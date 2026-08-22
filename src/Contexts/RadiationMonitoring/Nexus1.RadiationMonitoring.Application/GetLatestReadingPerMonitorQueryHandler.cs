using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

public sealed class GetLatestReadingPerMonitorQueryHandler(ILatestReadingPerMonitorFinder finder)
    : IQueryHandler<GetLatestReadingPerMonitorQuery, IReadOnlyList<LatestRadiationReadingDto>>
{
    public async Task<Result<IReadOnlyList<LatestRadiationReadingDto>>> Handle(GetLatestReadingPerMonitorQuery query, CancellationToken cancellationToken)
    {
        var readings = await finder.GetLatestReadingsAsync(cancellationToken);
        return Result<IReadOnlyList<LatestRadiationReadingDto>>.Success(readings);
    }
}
