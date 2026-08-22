using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

public sealed class GetSiteActivePlansQueryHandler(ISiteActivePlansFinder finder)
    : IQueryHandler<GetSiteActivePlansQuery, IReadOnlyList<ActiveEmergencyPlanDto>>
{
    public async Task<Result<IReadOnlyList<ActiveEmergencyPlanDto>>> Handle(GetSiteActivePlansQuery query, CancellationToken cancellationToken)
    {
        var plans = await finder.GetActivePlansAsync(query.SiteId, cancellationToken);
        return Result<IReadOnlyList<ActiveEmergencyPlanDto>>.Success(plans);
    }
}
