namespace Nexus1.Organization.Application;

/// <summary>
/// Matches the atlas's own C.3.8 query 2: resolve a login account (or
/// person id directly) to person, current department, and current team —
/// via DepartmentAssignment/TeamMembership rows with EndDate IS NULL.
/// </summary>
public interface IPersonOrganizationContextFinder
{
    Task<PersonOrganizationContextDto?> ResolveAsync(int? personId, int? applicationUserId, CancellationToken cancellationToken);
}
