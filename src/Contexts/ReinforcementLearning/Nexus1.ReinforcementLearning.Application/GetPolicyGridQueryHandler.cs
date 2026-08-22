using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class GetPolicyGridQueryHandler(IPolicyGridFinder finder)
    : IQueryHandler<GetPolicyGridQuery, IReadOnlyList<PolicyGridEntryDto>>
{
    public async Task<Result<IReadOnlyList<PolicyGridEntryDto>>> Handle(GetPolicyGridQuery query, CancellationToken cancellationToken)
    {
        var grid = await finder.GetPolicyGridAsync(query.PolicyId, cancellationToken);
        return Result<IReadOnlyList<PolicyGridEntryDto>>.Success(grid);
    }
}
