using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

public sealed class GetLatestStaffingGapsQueryHandler(IStaffingGapFinder finder)
    : IQueryHandler<GetLatestStaffingGapsQuery, IReadOnlyList<StaffingScenarioGapDto>>
{
    public async Task<Result<IReadOnlyList<StaffingScenarioGapDto>>> Handle(
        GetLatestStaffingGapsQuery query, CancellationToken cancellationToken)
    {
        var gaps = await finder.GetLatestGapsAsync(query.StaffingScenarioId, cancellationToken);
        return Result<IReadOnlyList<StaffingScenarioGapDto>>.Success(gaps);
    }
}
