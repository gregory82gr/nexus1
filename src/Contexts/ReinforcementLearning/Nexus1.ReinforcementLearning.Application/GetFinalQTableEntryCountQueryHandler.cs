using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class GetFinalQTableEntryCountQueryHandler(IFinalQTableEntryCountFinder finder)
    : IQueryHandler<GetFinalQTableEntryCountQuery, IReadOnlyList<FinalQTableEntryCountDto>>
{
    public async Task<Result<IReadOnlyList<FinalQTableEntryCountDto>>> Handle(GetFinalQTableEntryCountQuery query, CancellationToken cancellationToken)
    {
        var counts = await finder.GetFinalQTableEntryCountsAsync(cancellationToken);
        return Result<IReadOnlyList<FinalQTableEntryCountDto>>.Success(counts);
    }
}
