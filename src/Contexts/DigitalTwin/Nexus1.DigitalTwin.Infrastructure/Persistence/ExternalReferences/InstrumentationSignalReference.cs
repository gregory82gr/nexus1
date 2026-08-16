using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto Instrumentation.Signal's key
/// column — same technique and rationale as <see cref="ReactorFleetUnitReference"/>,
/// used to declare DigitalTwin.SignalBinding.SignalId and
/// DigitalTwin.TwinDivergence.SignalId's real FK to Instrumentation.Signal
/// (ADR-020). This is the fourth use of the ExcludeFromMigrations
/// shadow-entity technique in this codebase (ReactorFleetUnitReference and
/// CorePlatformEngineeringUnitReference already exist in both
/// Instrumentation and here; this one is new to DigitalTwin) — an
/// established, reusable pattern rather than a one-off, per ADR-020.
///
/// Tag is exposed (not just SignalId) because ADR-020's own
/// TraceModelVariableToSignalQuery (atlas C.6.8 query 2) needs the real
/// signal tag, not just its id.
/// </summary>
internal sealed class InstrumentationSignalReference
{
    public int SignalId { get; set; }

    /// <summary>Read-only projection of Instrumentation.Signal.Tag, needed by TraceModelVariableToSignalQuery (atlas C.6.8 query 2). Never written by this context.</summary>
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
