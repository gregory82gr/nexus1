namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>
/// Atlas verification query 4, adapted: resource readiness dashboard by
/// site and type, with each resource's latest ResourceReadinessCheck
/// status. The atlas's own query projects a site code directly
/// (single-database access); this codebase treats Organization.Site as
/// passport-only (OrganizationDb is a different physical database than
/// AlarmManagementDb, ADR-025), so SiteId is projected instead of a site
/// code — see EfResourceReadinessDashboardFinder's own doc comment for the
/// full explanation. ReadinessStatus is nullable — a resource with no
/// readiness check yet has never been assessed.
/// </summary>
public sealed record ResourceReadinessDashboardDto(int SiteId, string ResourceType, string? ReadinessStatus, int ResourceCount);
