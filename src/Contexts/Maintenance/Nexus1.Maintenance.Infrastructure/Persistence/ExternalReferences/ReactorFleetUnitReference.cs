using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto ReactorFleet.Unit's key column,
/// used solely so EF Core can declare a genuine SQL FOREIGN KEY from
/// Maintenance.Asset/Maintenance.WorkOrder to ReactorFleet.Unit across
/// bounded contexts that share AlarmManagementDb (ADR-021).
///
/// This is NOT a reference to Nexus1.ReactorFleet.Domain — Maintenance
/// Infrastructure cannot take a ProjectReference on
/// Nexus1.ReactorFleet.Domain or Nexus1.ReactorFleet.Infrastructure (the
/// dependency-law architecture test forbids one context's Infrastructure
/// referencing another context's Domain/Infrastructure directly). Instead
/// this type is a minimal, local stand-in mapped to the SAME physical
/// table ReactorFleet's own migration already created, marked
/// ExcludeFromMigrations so this DbContext never tries to create/drop/own
/// it — only to declare a real FK against it.
///
/// This is a local duplicate of Instrumentation's/DigitalTwin's own
/// ReactorFleetUnitReference (those types are internal to their own
/// Infrastructure projects, so they cannot be reused directly — deliberate
/// boilerplate, not an oversight, per ADR-020's own guidance, restated in
/// ADR-021: keep Maintenance's shadow entities local to Maintenance's own
/// project, do not reference DigitalTwin's Infrastructure project to reuse
/// its internal types).
/// </summary>
internal sealed class ReactorFleetUnitReference
{
    public int UnitId { get; set; }

    /// <summary>Read-only projection of ReactorFleet.Unit.Code, needed by the atlas's own C.9.5.2 queries 1 and 2 (UnitCode). Never written by this context.</summary>
    public string Code { get; set; } = string.Empty;
}

internal sealed class ReactorFleetUnitReferenceConfiguration : IEntityTypeConfiguration<ReactorFleetUnitReference>
{
    public void Configure(EntityTypeBuilder<ReactorFleetUnitReference> builder)
    {
        builder.ToTable("Unit", "ReactorFleet", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.UnitId);
        builder.Property(x => x.UnitId).HasColumnName("UnitId").ValueGeneratedNever();
        builder.Property(x => x.Code).HasColumnName("Code").HasMaxLength(80);
    }
}
