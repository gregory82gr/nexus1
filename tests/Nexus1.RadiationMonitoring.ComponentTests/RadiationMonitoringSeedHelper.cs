using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

/// <summary>
/// Shared seed: one ReactorFleet.Unit row and one CorePlatform.EngineeringUnit
/// row (the sector's only two real cross-context FK targets, ADR-024), the
/// twelve RadiationMonitoring lookup rows, and one RadiationZone/
/// RadiationMonitor/Dosimeter/PersonDosimeterAssignment/DoseLimit graph.
/// Every component test below builds on this same shape so the six
/// Application operations are exercised against a realistic, FK-consistent
/// graph rather than isolated rows — copying RoboticsSeedHelper's own
/// precedent pattern exactly (ADR-023/ADR-024).
/// </summary>
internal static class RadiationMonitoringSeedHelper
{
    public const string UnitCode = "NX1-U1";
    public const string EngineeringUnitSymbol = "uSv/h";
    public const string RadiationZoneCode = "RZ-001";
    public const string RadiationMonitorCode = "RM-001";
    public const string DosimeterCode = "DOS-001";
    public const string DoseLimitCode = "ANNUAL-EFF";

    public sealed record SeedResult(
        int UnitId, int EngineeringUnitId, int RadiationZoneTypeId, int RadiationZoneStatusActiveId,
        int RadiationAreaClassificationId, int MonitorTypeId, int MonitorStatusId, int MeasurementTypeId,
        int MeasurementQualityId, int DoseTypeId, int DosimeterTypeId, int DosimeterStatusId, int LimitTypeId,
        int AlertStatusOpenId, int AlertStatusAcknowledgedId, int AlertStatusClosedId, int RadiationZoneId,
        int RadiationMonitorId, int DosimeterId, int PersonDosimeterAssignmentId, int DoseLimitId, int PersonId);

    public static async Task<SeedResult> SeedCoreAsync(
        ReactorFleetDbContext reactorFleetContext, CorePlatformDbContext corePlatformContext,
        RadiationMonitoringDbContext radiationMonitoringContext, DateTime nowUtc)
    {
        var unit = Unit.Create(new Nexus1.ReactorFleet.Domain.UnitId(1), UnitCode, "Unit 1");
        await reactorFleetContext.Units.AddAsync(unit);
        await reactorFleetContext.SaveChangesAsync();

        var engineeringUnit = EngineeringUnit.Create(
            new EngineeringUnitId(1), EngineeringUnitSymbol, "Microsievert per hour", EngineeringQuantityType.RadiationDoseRate, nowUtc);
        await corePlatformContext.EngineeringUnits.AddAsync(engineeringUnit);
        await corePlatformContext.SaveChangesAsync();

        var radiationZoneType = RadiationZoneType.Create(new RadiationZoneTypeId(1), "CONTROLLED", "Controlled Area", nowUtc);
        var radiationZoneStatusActive = RadiationZoneStatus.Create(new RadiationZoneStatusId(1), "ACTIVE", "Active", nowUtc);
        var radiationAreaClassification = RadiationAreaClassification.Create(new RadiationAreaClassificationId(1), "HIGH", "High Radiation Area", nowUtc);
        var monitorType = MonitorType.Create(new MonitorTypeId(1), "AREA_GAMMA", "Area Gamma Monitor", nowUtc);
        var monitorStatus = MonitorStatus.Create(new MonitorStatusId(1), "IN_SERVICE", "In Service", nowUtc);
        var measurementType = MeasurementType.Create(new MeasurementTypeId(1), "DOSE_RATE", "Dose Rate", nowUtc);
        var measurementQuality = MeasurementQuality.Create(new MeasurementQualityId(1), "VALID", "Valid", nowUtc);
        var doseType = DoseType.Create(new DoseTypeId(1), "EFFECTIVE", "Effective Dose", nowUtc);
        var dosimeterType = DosimeterType.Create(new DosimeterTypeId(1), "TLD", "Thermoluminescent Dosimeter", nowUtc);
        var dosimeterStatus = DosimeterStatus.Create(new DosimeterStatusId(1), "ISSUED", "Issued", nowUtc);
        var limitType = LimitType.Create(new LimitTypeId(1), "ANNUAL", "Annual Limit", nowUtc);
        var alertStatusOpen = AlertStatus.Create(new AlertStatusId(1), "OPEN", "Open", nowUtc);
        var alertStatusAcknowledged = AlertStatus.Create(new AlertStatusId(2), "ACKNOWLEDGED", "Acknowledged", nowUtc);
        var alertStatusClosed = AlertStatus.Create(new AlertStatusId(3), "CLOSED", "Closed", nowUtc);

        await radiationMonitoringContext.RadiationZoneTypes.AddAsync(radiationZoneType);
        await radiationMonitoringContext.RadiationZoneStatuses.AddAsync(radiationZoneStatusActive);
        await radiationMonitoringContext.RadiationAreaClassifications.AddAsync(radiationAreaClassification);
        await radiationMonitoringContext.MonitorTypes.AddAsync(monitorType);
        await radiationMonitoringContext.MonitorStatuses.AddAsync(monitorStatus);
        await radiationMonitoringContext.MeasurementTypes.AddAsync(measurementType);
        await radiationMonitoringContext.MeasurementQualities.AddAsync(measurementQuality);
        await radiationMonitoringContext.DoseTypes.AddAsync(doseType);
        await radiationMonitoringContext.DosimeterTypes.AddAsync(dosimeterType);
        await radiationMonitoringContext.DosimeterStatuses.AddAsync(dosimeterStatus);
        await radiationMonitoringContext.LimitTypes.AddAsync(limitType);
        await radiationMonitoringContext.AlertStatuses.AddAsync(alertStatusOpen);
        await radiationMonitoringContext.AlertStatuses.AddAsync(alertStatusAcknowledged);
        await radiationMonitoringContext.AlertStatuses.AddAsync(alertStatusClosed);
        await radiationMonitoringContext.SaveChangesAsync();

        var radiationZone = RadiationZone.Create(
            new RadiationZoneId(1), radiationZoneType.Id, radiationZoneStatusActive.Id, radiationAreaClassification.Id,
            RadiationZoneCode, "Reactor Building Zone 1", unitId: unit.Id.Value);
        await radiationMonitoringContext.RadiationZones.AddAsync(radiationZone);
        await radiationMonitoringContext.SaveChangesAsync();

        var radiationMonitor = RadiationMonitor.Create(
            new RadiationMonitorId(1), monitorType.Id, monitorStatus.Id, RadiationMonitorCode, "Area Monitor 1",
            unitId: unit.Id.Value, radiationZoneId: radiationZone.Id);
        await radiationMonitoringContext.RadiationMonitors.AddAsync(radiationMonitor);
        await radiationMonitoringContext.SaveChangesAsync();

        var dosimeter = Dosimeter.Create(new DosimeterId(1), dosimeterType.Id, dosimeterStatus.Id, DosimeterCode);
        await radiationMonitoringContext.Dosimeters.AddAsync(dosimeter);
        await radiationMonitoringContext.SaveChangesAsync();

        const int personId = 501;
        var assignment = PersonDosimeterAssignment.Create(
            new PersonDosimeterAssignmentId(1), personId, dosimeter.Id, nowUtc.AddDays(-30));
        await radiationMonitoringContext.PersonDosimeterAssignments.AddAsync(assignment);
        await radiationMonitoringContext.SaveChangesAsync();

        var doseLimit = DoseLimit.Create(
            new DoseLimitId(1), doseType.Id, limitType.Id, engineeringUnit.Id.Value, DoseLimitCode,
            "Annual Effective Dose Limit", 20m, 365);
        await radiationMonitoringContext.DoseLimits.AddAsync(doseLimit);
        await radiationMonitoringContext.SaveChangesAsync();

        return new SeedResult(
            unit.Id.Value, engineeringUnit.Id.Value, radiationZoneType.Id.Value, radiationZoneStatusActive.Id.Value,
            radiationAreaClassification.Id.Value, monitorType.Id.Value, monitorStatus.Id.Value,
            measurementType.Id.Value, measurementQuality.Id.Value, doseType.Id.Value, dosimeterType.Id.Value,
            dosimeterStatus.Id.Value, limitType.Id.Value, alertStatusOpen.Id.Value, alertStatusAcknowledged.Id.Value,
            alertStatusClosed.Id.Value, radiationZone.Id.Value, radiationMonitor.Id.Value, dosimeter.Id.Value,
            assignment.Id.Value, doseLimit.Id.Value, personId);
    }
}
