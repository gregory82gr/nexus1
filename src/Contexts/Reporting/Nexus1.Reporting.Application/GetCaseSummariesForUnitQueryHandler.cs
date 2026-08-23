using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Reporting.Application;

public sealed class GetCaseSummariesForUnitQueryHandler(ICaseSummaryFinder finder)
    : IQueryHandler<GetCaseSummariesForUnitQuery, IReadOnlyList<CaseSummaryDto>>
{
    public async Task<Result<IReadOnlyList<CaseSummaryDto>>> Handle(GetCaseSummariesForUnitQuery query, CancellationToken cancellationToken)
    {
        var summaries = await finder.GetCaseSummariesForUnitAsync(query.UnitId, cancellationToken);
        return Result<IReadOnlyList<CaseSummaryDto>>.Success(summaries);
    }
}
