using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class GetPolicyEntryCountQueryHandler(IPolicyEntryCountFinder finder)
    : IQueryHandler<GetPolicyEntryCountQuery, IReadOnlyList<PolicyEntryCountDto>>
{
    public async Task<Result<IReadOnlyList<PolicyEntryCountDto>>> Handle(GetPolicyEntryCountQuery query, CancellationToken cancellationToken)
    {
        var counts = await finder.GetPolicyEntryCountsAsync(cancellationToken);
        return Result<IReadOnlyList<PolicyEntryCountDto>>.Success(counts);
    }
}
