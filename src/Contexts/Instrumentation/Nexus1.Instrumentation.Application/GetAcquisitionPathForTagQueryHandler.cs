using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

public sealed class GetAcquisitionPathForTagQueryHandler(IAcquisitionPathFinder finder)
    : IQueryHandler<GetAcquisitionPathForTagQuery, IReadOnlyList<AcquisitionPathDto>>
{
    public async Task<Result<IReadOnlyList<AcquisitionPathDto>>> Handle(
        GetAcquisitionPathForTagQuery query, CancellationToken cancellationToken)
    {
        var path = await finder.GetByTagAsync(query.Tag, cancellationToken);
        return Result<IReadOnlyList<AcquisitionPathDto>>.Success(path);
    }
}
