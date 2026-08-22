using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

public sealed class GetBlockingReadinessFailuresQueryHandler(IBlockingReadinessFailuresFinder finder)
    : IQueryHandler<GetBlockingReadinessFailuresQuery, IReadOnlyList<ReadinessFailureDto>>
{
    public async Task<Result<IReadOnlyList<ReadinessFailureDto>>> Handle(GetBlockingReadinessFailuresQuery query, CancellationToken cancellationToken)
    {
        var failures = await finder.GetBlockingFailuresAsync(query.MissionId, cancellationToken);
        return Result<IReadOnlyList<ReadinessFailureDto>>.Success(failures);
    }
}
