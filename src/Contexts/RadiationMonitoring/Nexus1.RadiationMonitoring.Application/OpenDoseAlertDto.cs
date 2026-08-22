namespace Nexus1.RadiationMonitoring.Application;

/// <summary>
/// Atlas C.13.5.2 query 4, adapted: open dose alerts and the person
/// assignment that produced them. The atlas's own query projects
/// Organization.Person.DisplayName directly (single-database access); this
/// codebase treats Organization.Person as passport-only (OrganizationDb is
/// a different physical database than AlarmManagementDb, ADR-024), so
/// PersonId is projected instead of a display name — see
/// EfOpenDoseAlertsFinder's own doc comment for the full explanation.
/// </summary>
public sealed record OpenDoseAlertDto(DateTime AlertAtUtc, string Message, string LimitCode, int? PersonId, decimal? DoseValue, string? EngineeringUnitSymbol);
