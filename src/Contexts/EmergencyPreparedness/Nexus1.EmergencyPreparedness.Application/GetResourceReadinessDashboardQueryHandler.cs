using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

public sealed class GetResourceReadinessDashboardQueryHandler(IResourceReadinessDashboardFinder finder)
    : IQueryHandler<GetResourceReadinessDashboardQuery, IReadOnlyList<ResourceReadinessDashboardDto>>
{
    public async Task<Result<IReadOnlyList<ResourceReadinessDashboardDto>>> Handle(GetResourceReadinessDashboardQuery query, CancellationToken cancellationToken)
    {
        var dashboard = await finder.GetResourceReadinessDashboardAsync(cancellationToken);
        return Result<IReadOnlyList<ResourceReadinessDashboardDto>>.Success(dashboard);
    }
}
