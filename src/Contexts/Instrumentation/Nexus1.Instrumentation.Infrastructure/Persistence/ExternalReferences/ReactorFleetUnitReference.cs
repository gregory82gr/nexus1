using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto ReactorFleet.Unit's key column,
/// used solely so EF Core can declare a genuine SQL FOREIGN KEY from
/// Instrumentation.Signal/DataAcquisitionNode to ReactorFleet.Unit across
/// bounded contexts that share AlarmManagementDb (ADR-019).
///
/// This is NOT a reference to Nexus1.ReactorFleet.Domain.Unit — Instrumentation
/// Infrastructure cannot take a ProjectReference on Nexus1.ReactorFleet.Domain
/// or Nexus1.ReactorFleet.Infrastructure (the dependency-law architecture test
/// forbids one context's Infrastructure referencing another context's
/// Domain/Infrastructure directly). Instead this type is a minimal, local
/// stand-in mapped to the SAME physical table ReactorFleet's own migration
/// already created, marked ExcludeFromMigrations so this DbContext never
/// tries to create/drop/own it — only to declare a real FK against it.
///
/// No precedent for this technique existed in the codebase before this
/// sector: ReactorFleet and CorePlatform do not yet reference each other
/// with a real FK anywhere (verified by grep before writing this), so this
/// is Instrumentation's own solution to ADR-019's "real FK, not
/// passport-only int" requirement, not a copy of an existing pattern.
/// </summary>
internal sealed class ReactorFleetUnitReference
{
    public int UnitId { get; set; }

    /// <summary>Read-only projection of ReactorFleet.Unit.Code, needed by the atlas's own C.5.8 verification queries (unit code -> signals). Never written by this context.</summary>
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
