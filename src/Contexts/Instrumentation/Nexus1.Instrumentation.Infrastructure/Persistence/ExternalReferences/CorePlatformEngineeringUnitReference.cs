using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto CorePlatform.EngineeringUnit's key
/// column — same technique and rationale as <see cref="ReactorFleetUnitReference"/>,
/// used to declare Instrumentation.Signal's real FK to
/// CorePlatform.EngineeringUnit (ADR-019) without an Infrastructure-to-Domain
/// ProjectReference across contexts.
/// </summary>
internal sealed class CorePlatformEngineeringUnitReference
{
    public int EngineeringUnitId { get; set; }

    /// <summary>Read-only projection of CorePlatform.EngineeringUnit.Symbol, needed by the atlas's own C.5.8 query 1 (unit_symbol). Never written by this context.</summary>
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
