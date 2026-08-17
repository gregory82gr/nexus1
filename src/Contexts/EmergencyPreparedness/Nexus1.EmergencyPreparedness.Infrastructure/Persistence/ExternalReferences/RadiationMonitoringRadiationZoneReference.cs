using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto RadiationMonitoring.RadiationZone's
/// key column, used solely so EF Core can declare a genuine SQL FOREIGN KEY
/// from EmergencyPreparedness.AssemblyPoint/EvacuationRouteZone to
/// RadiationMonitoring.RadiationZone across bounded contexts that share
/// AlarmManagementDb (ADR-025).
///
/// This is NOT a reference to Nexus1.RadiationMonitoring.Domain —
/// EmergencyPreparedness Infrastructure cannot take a ProjectReference on
/// Nexus1.RadiationMonitoring.Domain or Nexus1.RadiationMonitoring.Infrastructure
/// (the dependency-law architecture test forbids one context's
/// Infrastructure referencing another context's Domain/Infrastructure
/// directly). Instead this type is a minimal, local stand-in mapped to the
/// SAME physical table RadiationMonitoring's own migration already created
/// (RadiationMonitoring.RadiationZone, key column RadiationZoneId, Code
/// NVARCHAR(80) per RadiationZoneConfiguration), marked ExcludeFromMigrations
/// so this DbContext never tries to create/drop/own it — only to declare a
/// real FK against it.
///
/// This is the first shadow entity in this codebase targeting a table from
/// a sector built within this same Phase 2 sequence (RadiationMonitoring,
/// sector 9) rather than a V1 or early-Phase-2 context — the technique is
/// unchanged, just a new target (ADR-025).
/// </summary>
internal sealed class RadiationMonitoringRadiationZoneReference
{
    public int RadiationZoneId { get; set; }

    /// <summary>Read-only projection of RadiationMonitoring.RadiationZone.Code. Never written by this context.</summary>
    public string Code { get; set; } = string.Empty;
}

internal sealed class RadiationMonitoringRadiationZoneReferenceConfiguration : IEntityTypeConfiguration<RadiationMonitoringRadiationZoneReference>
{
    public void Configure(EntityTypeBuilder<RadiationMonitoringRadiationZoneReference> builder)
    {
        builder.ToTable("RadiationZone", "RadiationMonitoring", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.RadiationZoneId);
        builder.Property(x => x.RadiationZoneId).HasColumnName("RadiationZoneId").ValueGeneratedNever();
        builder.Property(x => x.Code).HasColumnName("Code").HasMaxLength(80);
    }
}
