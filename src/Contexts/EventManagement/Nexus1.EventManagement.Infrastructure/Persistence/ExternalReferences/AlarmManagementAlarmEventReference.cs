using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.EventManagement.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto AlarmManagement.AlarmEvent's key
/// column, used solely so EF Core can declare a genuine SQL FOREIGN KEY from
/// EventManagement.EventAlarmLink to AlarmManagement.AlarmEvent — both share
/// AlarmManagementDb (ADR-022's persistence decision).
///
/// This is the FIRST-EVER shadow reference into AlarmManagement in this
/// codebase (every prior sector's shadow entities point at ReactorFleet,
/// CorePlatform or Instrumentation). Real table name (AlarmManagement.AlarmEvent)
/// and key type (BIGINT / long, AlarmEventId) confirmed directly by reading
/// Nexus1.AlarmManagement.Infrastructure's own AlarmEventConfiguration.cs and
/// Nexus1.AlarmManagement.Domain's AlarmEventId.cs, not assumed from the atlas
/// alone (ADR-022's own verification discipline).
///
/// NOT a reference to Nexus1.AlarmManagement.Domain — EventManagement
/// Infrastructure cannot take a ProjectReference on
/// Nexus1.AlarmManagement.Domain/.Infrastructure (dependency-law architecture
/// test). Marked ExcludeFromMigrations so this DbContext never tries to
/// create/drop/own the physical table — only to declare a real FK against it.
/// </summary>
internal sealed class AlarmManagementAlarmEventReference
{
    public long AlarmEventId { get; set; }
}

internal sealed class AlarmManagementAlarmEventReferenceConfiguration : IEntityTypeConfiguration<AlarmManagementAlarmEventReference>
{
    public void Configure(EntityTypeBuilder<AlarmManagementAlarmEventReference> builder)
    {
        builder.ToTable("AlarmEvent", "AlarmManagement", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.AlarmEventId);
        builder.Property(x => x.AlarmEventId).HasColumnName("AlarmEventId").ValueGeneratedNever();
    }
}
