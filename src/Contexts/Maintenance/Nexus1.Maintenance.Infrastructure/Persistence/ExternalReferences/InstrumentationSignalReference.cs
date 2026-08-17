using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto Instrumentation.Signal's key
/// column — same technique and rationale as <see cref="ReactorFleetUnitReference"/>,
/// used to declare Maintenance.AssetConditionMeasurement.SignalId and
/// Maintenance.DegradationTrendPoint.SourceSignalId's real, nullable FK to
/// Instrumentation.Signal (ADR-021). Local duplicate of Instrumentation's/
/// DigitalTwin's own internal type — see the sibling reference's XML doc for
/// why duplication, not reuse, is the deliberate choice here.
/// </summary>
internal sealed class InstrumentationSignalReference
{
    public int SignalId { get; set; }

    /// <summary>Read-only projection of Instrumentation.Signal.Tag. Never written by this context.</summary>
    public string Tag { get; set; } = string.Empty;
}

internal sealed class InstrumentationSignalReferenceConfiguration : IEntityTypeConfiguration<InstrumentationSignalReference>
{
    public void Configure(EntityTypeBuilder<InstrumentationSignalReference> builder)
    {
        builder.ToTable("Signal", "Instrumentation", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.SignalId);
        builder.Property(x => x.SignalId).HasColumnName("SignalId").ValueGeneratedNever();
        builder.Property(x => x.Tag).HasColumnName("Tag").HasMaxLength(80);
    }
}
