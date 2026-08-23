using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

public sealed class GetDepartmentRosterQueryHandler(IDepartmentRosterFinder finder)
    : IQueryHandler<GetDepartmentRosterQuery, IReadOnlyList<DepartmentRosterEntryDto>>
{
    public async Task<Result<IReadOnlyList<DepartmentRosterEntryDto>>> Handle(
        GetDepartmentRosterQuery query, CancellationToken cancellationToken)
    {
        var roster = await finder.GetRosterAsync(query.DepartmentId, cancellationToken);
        return Result<IReadOnlyList<DepartmentRosterEntryDto>>.Success(roster);
    }
}
