using Microsoft.EntityFrameworkCore;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own C.13.5.2 query 4, adapted: DoseAlert JOIN
/// DoseLimit, JOIN AlertStatus WHERE Code IN ('OPEN','ACKNOWLEDGED'), LEFT
/// JOIN PersonDoseReading (via DoseAlert.PersonDoseReadingId) and LEFT JOIN
/// PersonDosimeterAssignment (via PersonDoseReading.PersonDosimeterAssignmentId)
/// for the person and dose value that produced the alert.
///
/// Deviation from the atlas's own literal query text: the atlas projects
/// Organization.Person.DisplayName directly, which assumes single-database
/// access. In this codebase Organization.Person is passport-only —
/// OrganizationDb is a different physical database than AlarmManagementDb
/// (ADR-024) — so there is no local Person table to join against. This
/// finder projects PersonDosimeterAssignment.PersonId (a plain int) instead,
/// matching how every prior sector in this codebase has handled an atlas
/// query that reaches across a database boundary its own domain model
/// doesn't enforce (see OpenDoseAlertDto's own doc comment).
/// </summary>
internal sealed class EfOpenDoseAlertsFinder(RadiationMonitoringDbContext dbContext) : IOpenDoseAlertsFinder
{
    private static readonly string[] OpenAlertStatusCodes = ["OPEN", "ACKNOWLEDGED"];

    public async Task<IReadOnlyList<OpenDoseAlertDto>> GetOpenDoseAlertsAsync(CancellationToken cancellationToken)
    {
        var query =
            from a in dbContext.DoseAlerts
            join s in dbContext.AlertStatuses on a.AlertStatusId equals s.Id
            where OpenAlertStatusCodes.Contains(s.Code)
            join l in dbContext.DoseLimits on a.DoseLimitId equals l.Id
            join r in dbContext.PersonDoseReadings on a.PersonDoseReadingId equals r.Id into readings
            from reading in readings.DefaultIfEmpty()
            join asg in dbContext.PersonDosimeterAssignments on reading!.PersonDosimeterAssignmentId equals asg.Id into assignments
            from assignment in assignments.DefaultIfEmpty()
            join u in dbContext.Set<CorePlatformEngineeringUnitReference>() on
                (reading != null ? (int?)reading.EngineeringUnitId : null) equals u.EngineeringUnitId into units
            from unit in units.DefaultIfEmpty()
            select new OpenDoseAlertDto(
                a.AlertAtUtc, a.Message, l.Code, assignment != null ? (int?)assignment.PersonId : null,
                reading != null ? (decimal?)reading.DoseValue : null, unit != null ? unit.Symbol : null);

        return await query.ToListAsync(cancellationToken);
    }
}
