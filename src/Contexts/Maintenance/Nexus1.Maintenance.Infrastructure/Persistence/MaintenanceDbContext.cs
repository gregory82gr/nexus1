using Microsoft.EntityFrameworkCore;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-021, following
/// ADR-006/ADR-015/ADR-019/ADR-020's precedent) but keeps its own migration
/// history and only owns Maintenance.* tables. The ExternalReferences types
/// (ReactorFleetUnitReference, InstrumentationSignalReference,
/// CorePlatformEngineeringUnitReference) are also part of this model — configured
/// via IEntityTypeConfiguration and picked up by ApplyConfigurationsFromAssembly
/// below — but are excluded from this context's migrations; they exist only
/// so FK relationships to tables owned by other contexts can be declared.
/// </summary>
public sealed class MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : DbContext(options)
{
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();

    public DbSet<AssetStatus> AssetStatuses => Set<AssetStatus>();

    public DbSet<AssetCriticality> AssetCriticalities => Set<AssetCriticality>();

    public DbSet<ConditionGrade> ConditionGrades => Set<ConditionGrade>();

    public DbSet<DegradationMechanism> DegradationMechanisms => Set<DegradationMechanism>();

    public DbSet<FindingSeverity> FindingSeverities => Set<FindingSeverity>();

    public DbSet<WorkOrderType> WorkOrderTypes => Set<WorkOrderType>();

    public DbSet<WorkOrderStatus> WorkOrderStatuses => Set<WorkOrderStatus>();

    public DbSet<WorkPriority> WorkPriorities => Set<WorkPriority>();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<AssetComponent> AssetComponents => Set<AssetComponent>();

    public DbSet<AssetCondition> AssetConditions => Set<AssetCondition>();

    public DbSet<AssetConditionMeasurement> AssetConditionMeasurements => Set<AssetConditionMeasurement>();

    public DbSet<DegradationRecord> DegradationRecords => Set<DegradationRecord>();

    public DbSet<DegradationTrendPoint> DegradationTrendPoints => Set<DegradationTrendPoint>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaintenanceDbContext).Assembly);
    }
}
