using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

public sealed class GetSitePlantHierarchyQueryHandler(ISitePlantHierarchyFinder finder)
    : IQueryHandler<GetSitePlantHierarchyQuery, SitePlantHierarchyDto>
{
    public async Task<Result<SitePlantHierarchyDto>> Handle(GetSitePlantHierarchyQuery query, CancellationToken cancellationToken)
    {
        var hierarchy = await finder.GetBySiteCodeAsync(query.SiteCode, cancellationToken);

        return hierarchy is null
            ? Result<SitePlantHierarchyDto>.Failure($"Site '{query.SiteCode}' does not exist.")
            : Result<SitePlantHierarchyDto>.Success(hierarchy);
    }
}
