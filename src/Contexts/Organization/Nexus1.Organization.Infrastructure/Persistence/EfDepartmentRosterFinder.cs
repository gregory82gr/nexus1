using Microsoft.EntityFrameworkCore;
using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence;

internal sealed class EfDepartmentRosterFinder(OrganizationDbContext dbContext) : IDepartmentRosterFinder
{
    public async Task<IReadOnlyList<DepartmentRosterEntryDto>> GetRosterAsync(int departmentId, CancellationToken cancellationToken)
    {
        var id = new DepartmentId(departmentId);

        var rows = await (
            from assignment in dbContext.DepartmentAssignments
            where assignment.DepartmentId == id && assignment.EndDate == null
            join person in dbContext.People on assignment.PersonId equals person.Id
            orderby assignment.IsPrimary descending, person.DisplayName
            select new
            {
                person.Id,
                person.DisplayName,
                person.PersonnelNumber,
                person.ApplicationUserId,
                assignment.PositionId,
                assignment.StartDate,
                assignment.IsPrimary,
            })
            .ToListAsync(cancellationToken);

        // Positions resolved in a separate in-memory pass rather than an outer-joined
        // nullable-key join — avoids the PositionId?/PositionId type mismatch a LINQ
        // join clause would otherwise need to reconcile; the table is small.
        var positionsById = await dbContext.Positions
            .ToDictionaryAsync(p => p.Id, p => (p.Title, p.IsSafetyCritical), cancellationToken);

        return rows
            .Select(x => new DepartmentRosterEntryDto(
                x.Id.Value,
                x.DisplayName,
                x.PersonnelNumber,
                x.PositionId is { } posId && positionsById.TryGetValue(posId, out var pos) ? pos.Title : null,
                x.PositionId is { } posId2 && positionsById.TryGetValue(posId2, out var pos2) ? pos2.IsSafetyCritical : null,
                x.ApplicationUserId,
                x.StartDate,
                x.IsPrimary))
            .ToList();
    }
}
