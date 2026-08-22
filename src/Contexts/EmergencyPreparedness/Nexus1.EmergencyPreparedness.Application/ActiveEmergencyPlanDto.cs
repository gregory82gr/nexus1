namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>
/// Atlas verification query 1, adapted: one site's active plans and current
/// revision count. The atlas's own query projects a site code directly
/// (single-database access); this codebase treats Organization.Site as
/// passport-only (OrganizationDb is a different physical database than
/// AlarmManagementDb, ADR-025), so SiteId is projected instead of a site
/// code — see EfSiteActivePlansFinder's own doc comment for the full
/// explanation.
/// </summary>
public sealed record ActiveEmergencyPlanDto(string PlanCode, string PlanStatus, int CurrentRevisionNumber, int RevisionRowCount);
