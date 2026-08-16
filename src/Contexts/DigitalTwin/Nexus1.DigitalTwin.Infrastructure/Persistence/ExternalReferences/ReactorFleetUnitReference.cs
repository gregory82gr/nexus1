using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto ReactorFleet.Unit's key column,
/// used solely so EF Core can declare a genuine SQL FOREIGN KEY from
/// DigitalTwin.TwinModel to ReactorFleet.Unit across bounded contexts that
/// share AlarmManagementDb (ADR-020).
///
/// This is NOT a reference to Nexus1.ReactorFleet.Domain.Unit —
/// DigitalTwin Infrastructure cannot take a ProjectReference on
/// Nexus1.ReactorFleet.Domain or Nexus1.ReactorFleet.Infrastructure (the
/// dependency-law architecture test forbids one context's Infrastructure
/// referencing another context's Domain/Infrastructure directly). Instead
/// this type is a minimal, local stand-in mapped to the SAME physical
/// table ReactorFleet's own migration already created, marked
/// ExcludeFromMigrations so this DbContext never tries to create/drop/own
/// it — only to declare a real FK against it.
///
/// This is a local duplicate of Instrumentation's own
/// ReactorFleetUnitReference (that type is internal to
/// Nexus1.Instrumentation.Infrastructure, so it cannot be reused directly
/// — deliberate boilerplate, not an oversight, per ADR-020's own guidance:
/// keep DigitalTwin's shadow entities local to DigitalTwin's own project).
/// </summary>
internal sealed class ReactorFleetUnitReference
{
    public int UnitId { get; set; }

    /// <summary>Read-only projection of ReactorFleet.Unit.Code, needed by the atlas's own C.6.8 query 1 (UnitCode). Never written by this context.</summary>
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
