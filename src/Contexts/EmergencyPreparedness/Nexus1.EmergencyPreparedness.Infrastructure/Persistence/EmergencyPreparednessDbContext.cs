using Microsoft.EntityFrameworkCore;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-025, following
/// ADR-006/ADR-015/.../ADR-024's precedent) but keeps its own migration
/// history and only owns EmergencyPreparedness.* tables. The
/// ExternalReferences types (CorePlatformEngineeringUnitReference,
/// RadiationMonitoringRadiationZoneReference) are also part of this model —
/// configured via IEntityTypeConfiguration and picked up by
/// ApplyConfigurationsFromAssembly below — but are excluded from this
/// context's migrations; they exist only so FK relationships to tables
/// owned by CorePlatform/RadiationMonitoring can be declared.
/// </summary>
public sealed class EmergencyPreparednessDbContext(DbContextOptions<EmergencyPreparednessDbContext> options) : DbContext(options)
{
    public DbSet<PlanStatus> PlanStatuses => Set<PlanStatus>();

    public DbSet<RouteStatus> RouteStatuses => Set<RouteStatus>();

    public DbSet<ResourceType> ResourceTypes => Set<ResourceType>();

    public DbSet<ReadinessStatus> ReadinessStatuses => Set<ReadinessStatus>();

    public DbSet<ExerciseType> ExerciseTypes => Set<ExerciseType>();

    public DbSet<ExerciseStatus> ExerciseStatuses => Set<ExerciseStatus>();

    public DbSet<ObservationSeverity> ObservationSeverities => Set<ObservationSeverity>();

    public DbSet<ResourceStatus> ResourceStatuses => Set<ResourceStatus>();

    public DbSet<EmergencyPlan> EmergencyPlans => Set<EmergencyPlan>();

    public DbSet<EmergencyPlanRevision> EmergencyPlanRevisions => Set<EmergencyPlanRevision>();

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<ExerciseObservation> ExerciseObservations => Set<ExerciseObservation>();

    public DbSet<AssemblyPoint> AssemblyPoints => Set<AssemblyPoint>();

    public DbSet<EvacuationRoute> EvacuationRoutes => Set<EvacuationRoute>();

    public DbSet<EvacuationRouteZone> EvacuationRouteZones => Set<EvacuationRouteZone>();

    public DbSet<EmergencyResource> EmergencyResources => Set<EmergencyResource>();

    public DbSet<ResourceReadinessCheck> ResourceReadinessChecks => Set<ResourceReadinessCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmergencyPreparednessDbContext).Assembly);
    }
}
