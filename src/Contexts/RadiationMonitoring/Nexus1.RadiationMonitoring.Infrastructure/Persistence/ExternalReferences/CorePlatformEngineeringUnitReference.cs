using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto CorePlatform.EngineeringUnit's key
/// column — same technique and rationale as <see cref="ReactorFleetUnitReference"/>,
/// used to declare RadiationMonitoring.RadiationReading/DoseLimit/
/// PersonDoseReading's real FK to CorePlatform.EngineeringUnit (ADR-024)
/// without an Infrastructure-to-Domain ProjectReference across contexts.
/// This sector is the first to need both the ReactorFleetUnitReference and
/// CorePlatformEngineeringUnitReference shadow-entity families in the same
/// DbContext (ADR-024).
/// </summary>
internal sealed class CorePlatformEngineeringUnitReference
{
    public int EngineeringUnitId { get; set; }

    /// <summary>Read-only projection of CorePlatform.EngineeringUnit.Symbol, needed by the atlas's own query 3 (engineering unit symbol). Never written by this context.</summary>
    public string Symbol { get; set; } = string.Empty;
}

internal sealed class CorePlatformEngineeringUnitReferenceConfiguration : IEntityTypeConfiguration<CorePlatformEngineeringUnitReference>
{
    public void Configure(EntityTypeBuilder<CorePlatformEngineeringUnitReference> builder)
    {
        builder.ToTable("EngineeringUnit", "CorePlatform", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.EngineeringUnitId);
        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId").ValueGeneratedNever();
        builder.Property(x => x.Symbol).HasColumnName("Symbol").HasMaxLength(20);
    }
}
