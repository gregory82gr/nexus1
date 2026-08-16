using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

/// <summary>Atlas C.3.8 query 2, verbatim (login account -> person -> current department -> current team). Exactly one of PersonId/ApplicationUserId must be provided.</summary>
public sealed record ResolvePersonOrganizationContextQuery(int? PersonId = null, int? ApplicationUserId = null) : IQuery<PersonOrganizationContextDto>;
