using Microsoft.EntityFrameworkCore;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-020, following
/// ADR-006/ADR-015/ADR-019's precedent) but keeps its own migration history
/// and only owns DigitalTwin.* tables. The ExternalReferences types
/// (ReactorFleetUnitReference, CorePlatformEngineeringUnitReference,
/// InstrumentationSignalReference) are also part of this model — configured
/// via IEntityTypeConfiguration and picked up by ApplyConfigurationsFromAssembly
/// below — but are excluded from this context's migrations; they exist only
/// so FK relationships to tables owned by other contexts can be declared.
/// </summary>
public sealed class DigitalTwinDbContext(DbContextOptions<DigitalTwinDbContext> options) : DbContext(options)
{
    public DbSet<TwinModelType> TwinModelTypes => Set<TwinModelType>();

    public DbSet<TwinModelStatus> TwinModelStatuses => Set<TwinModelStatus>();

    public DbSet<TwinFidelityLevel> TwinFidelityLevels => Set<TwinFidelityLevel>();

    public DbSet<ModelVariableType> ModelVariableTypes => Set<ModelVariableType>();

    public DbSet<SolverType> SolverTypes => Set<SolverType>();

    public DbSet<ValidationStatus> ValidationStatuses => Set<ValidationStatus>();

    public DbSet<BindingRole> BindingRoles => Set<BindingRole>();

    public DbSet<BindingStatus> BindingStatuses => Set<BindingStatus>();

    public DbSet<SnapshotReason> SnapshotReasons => Set<SnapshotReason>();

    public DbSet<DivergenceSeverity> DivergenceSeverities => Set<DivergenceSeverity>();

    public DbSet<DivergenceStatus> DivergenceStatuses => Set<DivergenceStatus>();

    public DbSet<TwinModel> TwinModels => Set<TwinModel>();

    public DbSet<TwinModelVersion> TwinModelVersions => Set<TwinModelVersion>();

    public DbSet<TwinVariable> TwinVariables => Set<TwinVariable>();

    public DbSet<SignalBinding> SignalBindings => Set<SignalBinding>();

    public DbSet<TwinRuntimeSession> TwinRuntimeSessions => Set<TwinRuntimeSession>();

    public DbSet<TwinSnapshot> TwinSnapshots => Set<TwinSnapshot>();

    public DbSet<TwinSnapshotValue> TwinSnapshotValues => Set<TwinSnapshotValue>();

    public DbSet<TwinDivergence> TwinDivergences => Set<TwinDivergence>();

    public DbSet<TwinDivergenceReview> TwinDivergenceReviews => Set<TwinDivergenceReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DigitalTwinDbContext).Assembly);
    }
}
