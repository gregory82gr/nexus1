using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

public sealed class GetOpenDivergencesQueryHandler(IOpenDivergenceFinder finder)
    : IQueryHandler<GetOpenDivergencesQuery, IReadOnlyList<OpenDivergenceDto>>
{
    public async Task<Result<IReadOnlyList<OpenDivergenceDto>>> Handle(GetOpenDivergencesQuery query, CancellationToken cancellationToken)
    {
        var divergences = await finder.GetOpenDivergencesAsync(cancellationToken);
        return Result<IReadOnlyList<OpenDivergenceDto>>.Success(divergences);
    }
}
