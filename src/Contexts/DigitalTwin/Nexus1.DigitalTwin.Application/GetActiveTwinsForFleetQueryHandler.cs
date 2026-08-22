using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

public sealed class GetActiveTwinsForFleetQueryHandler(IActiveTwinFinder finder)
    : IQueryHandler<GetActiveTwinsForFleetQuery, IReadOnlyList<ActiveTwinDto>>
{
    public async Task<Result<IReadOnlyList<ActiveTwinDto>>> Handle(GetActiveTwinsForFleetQuery query, CancellationToken cancellationToken)
    {
        var twins = await finder.GetActiveTwinsAsync(cancellationToken);
        return Result<IReadOnlyList<ActiveTwinDto>>.Success(twins);
    }
}
