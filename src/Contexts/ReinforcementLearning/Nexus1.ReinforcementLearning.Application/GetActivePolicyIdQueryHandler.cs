using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class GetActivePolicyIdQueryHandler(IActivePolicyFinder finder)
    : IQueryHandler<GetActivePolicyIdQuery, int?>
{
    public async Task<Result<int?>> Handle(GetActivePolicyIdQuery query, CancellationToken cancellationToken)
    {
        var policyId = await finder.GetActivePolicyIdAsync(cancellationToken);
        return Result<int?>.Success(policyId);
    }
}
