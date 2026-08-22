using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto DigitalTwin.TwinModel's key column,
/// used solely so EF Core can declare a genuine SQL FOREIGN KEY from
/// EnvironmentModel.TwinModelId to DigitalTwin.TwinModel across bounded
/// contexts that share AlarmManagementDb (ADR-026).
///
/// This is the second shadow entity in this codebase targeting a table
/// built within this same Phase 2 sequence rather than a V1 or
/// early-Phase-2 context — the first was EmergencyPreparedness's own
/// RadiationMonitoringRadiationZoneReference (ADR-025). Same technique, new
/// target: DigitalTwin.TwinModel (key column TwinModelId, int identity;
/// Code NVARCHAR(80) — confirmed directly against
/// Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin.TwinModelConfiguration).
/// Only TwinModelId and Code are projected, matching
/// ReactorFleetUnitReference's own minimal-projection shape. Marked
/// ExcludeFromMigrations so this DbContext never tries to create/drop/own
/// the physical table — only to declare a real FK against it.
/// </summary>
internal sealed class DigitalTwinTwinModelReference
{
    public int TwinModelId { get; set; }

    /// <summary>Read-only projection of DigitalTwin.TwinModel.Code. Never written by this context.</summary>
    public string Code { get; set; } = string.Empty;
}

internal sealed class DigitalTwinTwinModelReferenceConfiguration : IEntityTypeConfiguration<DigitalTwinTwinModelReference>
{
    public void Configure(EntityTypeBuilder<DigitalTwinTwinModelReference> builder)
    {
        builder.ToTable("TwinModel", "DigitalTwin", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.TwinModelId);
        builder.Property(x => x.TwinModelId).HasColumnName("TwinModelId").ValueGeneratedNever();
        builder.Property(x => x.Code).HasColumnName("Code").HasMaxLength(80);
    }
}
