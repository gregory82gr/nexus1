using Microsoft.EntityFrameworkCore;
using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own C.3.8 query 2: login account (or person id) ->
/// person -> current department (DepartmentAssignment.EndDate IS NULL) ->
/// current team (TeamMembership.EndDate IS NULL). When a person holds
/// several open rows, the primary/lead one is preferred, then the most
/// recently started.
/// </summary>
internal sealed class EfPersonOrganizationContextFinder(OrganizationDbContext dbContext) : IPersonOrganizationContextFinder
{
    public async Task<PersonOrganizationContextDto?> ResolveAsync(
        int? personId, int? applicationUserId, CancellationToken cancellationToken)
    {
        IQueryable<Person> peopleQuery = dbContext.People;

        if (personId is { } id)
        {
            var typedId = new PersonId(id);
            peopleQuery = peopleQuery.Where(x => x.Id == typedId);
        }
        else if (applicationUserId is not null)
        {
            peopleQuery = peopleQuery.Where(x => x.ApplicationUserId == applicationUserId);
        }
        else
        {
            return null;
        }

        var person = await peopleQuery.SingleOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            return null;
        }

        var departmentName = await (
            from assignment in dbContext.DepartmentAssignments
            where assignment.PersonId == person.Id && assignment.EndDate == null
            join department in dbContext.Departments on assignment.DepartmentId equals department.Id
            orderby assignment.IsPrimary descending, assignment.StartDate descending
            select department.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var teamName = await (
            from membership in dbContext.TeamMemberships
            where membership.PersonId == person.Id && membership.EndDate == null
            join team in dbContext.Teams on membership.TeamId equals team.Id
            orderby membership.IsLead descending, membership.StartDate descending
            select team.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new PersonOrganizationContextDto(person.Id.Value, person.DisplayName, departmentName, teamName);
    }
}
