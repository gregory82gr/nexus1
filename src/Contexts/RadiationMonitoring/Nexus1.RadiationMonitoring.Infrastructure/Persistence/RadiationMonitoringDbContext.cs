using Microsoft.EntityFrameworkCore;
using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-024, following
/// ADR-006/ADR-015/ADR-019/ADR-020/ADR-021/ADR-022/ADR-023's precedent) but
/// keeps its own migration history and only owns RadiationMonitoring.*
/// tables. The ExternalReferences types (ReactorFleetUnitReference,
/// CorePlatformEngineeringUnitReference) are also part of this model —
/// configured via IEntityTypeConfiguration and picked up by
/// ApplyConfigurationsFromAssembly below — but are excluded from this
/// context's migrations; they exist only so FK relationships to tables
/// owned by ReactorFleet/CorePlatform can be declared. This is the first
/// sector to need both shadow-entity families in the same DbContext
/// (ADR-024).
/// </summary>
public sealed class RadiationMonitoringDbContext(DbContextOptions<RadiationMonitoringDbContext> options) : DbContext(options)
{
    public DbSet<RadiationZoneType> RadiationZoneTypes => Set<RadiationZoneType>();

    public DbSet<RadiationZoneStatus> RadiationZoneStatuses => Set<RadiationZoneStatus>();

    public DbSet<RadiationAreaClassification> RadiationAreaClassifications => Set<RadiationAreaClassification>();

    public DbSet<MonitorType> MonitorTypes => Set<MonitorType>();

    public DbSet<MonitorStatus> MonitorStatuses => Set<MonitorStatus>();

    public DbSet<MeasurementType> MeasurementTypes => Set<MeasurementType>();

    public DbSet<MeasurementQuality> MeasurementQualities => Set<MeasurementQuality>();

    public DbSet<DoseType> DoseTypes => Set<DoseType>();

    public DbSet<DosimeterType> DosimeterTypes => Set<DosimeterType>();

    public DbSet<DosimeterStatus> DosimeterStatuses => Set<DosimeterStatus>();

    public DbSet<LimitType> LimitTypes => Set<LimitType>();

    public DbSet<AlertStatus> AlertStatuses => Set<AlertStatus>();

    public DbSet<RadiationZone> RadiationZones => Set<RadiationZone>();

    public DbSet<RadiationMonitor> RadiationMonitors => Set<RadiationMonitor>();

    public DbSet<RadiationReading> RadiationReadings => Set<RadiationReading>();

    public DbSet<Dosimeter> Dosimeters => Set<Dosimeter>();

    public DbSet<PersonDosimeterAssignment> PersonDosimeterAssignments => Set<PersonDosimeterAssignment>();

    public DbSet<PersonDoseReading> PersonDoseReadings => Set<PersonDoseReading>();

    public DbSet<DoseLimit> DoseLimits => Set<DoseLimit>();

    public DbSet<DoseAlert> DoseAlerts => Set<DoseAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RadiationMonitoringDbContext).Assembly);
    }
}
