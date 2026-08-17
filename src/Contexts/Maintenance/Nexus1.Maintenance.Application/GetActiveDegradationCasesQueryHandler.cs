using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

public sealed class GetActiveDegradationCasesQueryHandler(IActiveDegradationCasesFinder finder)
    : IQueryHandler<GetActiveDegradationCasesQuery, IReadOnlyList<ActiveDegradationCaseDto>>
{
    public async Task<Result<IReadOnlyList<ActiveDegradationCaseDto>>> Handle(GetActiveDegradationCasesQuery query, CancellationToken cancellationToken)
    {
        var cases = await finder.GetActiveDegradationCasesAsync(cancellationToken);
        return Result<IReadOnlyList<ActiveDegradationCaseDto>>.Success(cases);
    }
}
