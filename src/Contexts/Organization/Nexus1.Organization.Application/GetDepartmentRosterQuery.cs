using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

public sealed record GetDepartmentRosterQuery(int DepartmentId) : IQuery<IReadOnlyList<DepartmentRosterEntryDto>>;
