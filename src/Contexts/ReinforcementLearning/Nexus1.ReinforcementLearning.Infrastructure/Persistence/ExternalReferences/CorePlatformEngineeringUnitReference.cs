using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto CorePlatform.EngineeringUnit's key
/// column — same technique and rationale as every other Phase 2 sector's own
/// CorePlatformEngineeringUnitReference, used here to declare ActionSpace's
/// real FK to CorePlatform.EngineeringUnit (ADR-026) without an
/// Infrastructure-to-Domain ProjectReference across contexts.
/// </summary>
internal sealed class CorePlatformEngineeringUnitReference
{
    public int EngineeringUnitId { get; set; }

    /// <summary>Read-only projection of CorePlatform.EngineeringUnit.Symbol. Never written by this context.</summary>
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
