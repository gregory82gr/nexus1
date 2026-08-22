using Microsoft.EntityFrameworkCore;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-019, following
/// ADR-006/ADR-015's precedent) but keeps its own migration history and
/// only owns Instrumentation.* tables. The ExternalReferences types
/// (ReactorFleetUnitReference, CorePlatformEngineeringUnitReference) are
/// also part of this model — configured via IEntityTypeConfiguration and
/// picked up by ApplyConfigurationsFromAssembly below — but are excluded
/// from this context's migrations; they exist only so FK relationships to
/// tables owned by other contexts can be declared.
/// </summary>
public sealed class InstrumentationDbContext(DbContextOptions<InstrumentationDbContext> options) : DbContext(options)
{
    public DbSet<SignalType> SignalTypes => Set<SignalType>();

    public DbSet<SignalCategory> SignalCategories => Set<SignalCategory>();

    public DbSet<SignalRole> SignalRoles => Set<SignalRole>();

    public DbSet<SamplingMode> SamplingModes => Set<SamplingMode>();

    public DbSet<HistorianRetentionClass> HistorianRetentionClasses => Set<HistorianRetentionClass>();

    public DbSet<SignalQuality> SignalQualities => Set<SignalQuality>();

    public DbSet<MeasurementSource> MeasurementSources => Set<MeasurementSource>();

    public DbSet<ChannelStatus> ChannelStatuses => Set<ChannelStatus>();

    public DbSet<DataAcquisitionNode> DataAcquisitionNodes => Set<DataAcquisitionNode>();

    public DbSet<AcquisitionConnection> AcquisitionConnections => Set<AcquisitionConnection>();

    public DbSet<AcquisitionPoint> AcquisitionPoints => Set<AcquisitionPoint>();

    public DbSet<Signal> Signals => Set<Signal>();

    public DbSet<SignalMapping> SignalMappings => Set<SignalMapping>();

    public DbSet<Measurement> Measurements => Set<Measurement>();

    public DbSet<SignalQualityEvent> SignalQualityEvents => Set<SignalQualityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InstrumentationDbContext).Assembly);
    }
}
