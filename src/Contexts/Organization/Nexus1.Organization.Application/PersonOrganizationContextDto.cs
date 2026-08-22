namespace Nexus1.Organization.Application;

public sealed record PersonOrganizationContextDto(int PersonId, string DisplayName, string? DepartmentName, string? TeamName);
