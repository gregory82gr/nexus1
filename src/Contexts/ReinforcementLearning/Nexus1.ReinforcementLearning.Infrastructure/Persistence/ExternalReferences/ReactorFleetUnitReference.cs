using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto ReactorFleet.Unit's key column, used
/// solely so EF Core can declare genuine SQL FOREIGN KEYs from
/// EnvironmentModel.UnitId, Experiment.UnitId, PolicyDeployment.UnitId and
/// AdvisorySession.UnitId to ReactorFleet.Unit across bounded contexts that
/// share AlarmManagementDb (ADR-026).
///
/// This is NOT a reference to Nexus1.ReactorFleet.Domain — this context's
/// Infrastructure cannot take a ProjectReference on Nexus1.ReactorFleet.Domain
/// or Nexus1.ReactorFleet.Infrastructure (the dependency-law architecture test
/// forbids one context's Infrastructure referencing another context's
/// Domain/Infrastructure directly). Instead this type is a minimal, local
/// stand-in mapped to the SAME physical table ReactorFleet's own migration
/// already created, marked ExcludeFromMigrations so this DbContext never
/// tries to create/drop/own it — only to declare a real FK against it. A
/// local duplicate of every other Phase 2 sector's own ReactorFleetUnitReference
/// — deliberate boilerplate, per ADR-020's own guidance: keep shadow entities
/// local to their own project.
/// </summary>
internal sealed class ReactorFleetUnitReference
{
    public int UnitId { get; set; }

    /// <summary>Read-only projection of ReactorFleet.Unit.Code. Never written by this context.</summary>
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
