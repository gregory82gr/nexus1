using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.EventManagement.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto AlarmManagement.AlarmFlood's key
/// column, used solely so EF Core can declare a genuine SQL FOREIGN KEY from
/// EventManagement.EventFloodLink to AlarmManagement.AlarmFlood — both share
/// AlarmManagementDb (ADR-022's persistence decision).
///
/// The second first-ever shadow reference into AlarmManagement in this
/// codebase (alongside AlarmManagementAlarmEventReference). Real table name
/// (AlarmManagement.AlarmFlood) and key type (BIGINT / long, AlarmFloodId)
/// confirmed directly by reading Nexus1.AlarmManagement.Infrastructure's own
/// AlarmFloodConfiguration.cs and Nexus1.AlarmManagement.Domain's
/// AlarmFloodId.cs, not assumed from the atlas alone (ADR-022).
///
/// NOT a reference to Nexus1.AlarmManagement.Domain — same dependency-law
/// reasoning as AlarmManagementAlarmEventReference. Marked
/// ExcludeFromMigrations so this DbContext never tries to create/drop/own
/// the physical table — only to declare a real FK against it.
/// </summary>
internal sealed class AlarmManagementAlarmFloodReference
{
    public long AlarmFloodId { get; set; }
}

internal sealed class AlarmManagementAlarmFloodReferenceConfiguration : IEntityTypeConfiguration<AlarmManagementAlarmFloodReference>
{
    public void Configure(EntityTypeBuilder<AlarmManagementAlarmFloodReference> builder)
    {
        builder.ToTable("AlarmFlood", "AlarmManagement", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.AlarmFloodId);
        builder.Property(x => x.AlarmFloodId).HasColumnName("AlarmFloodId").ValueGeneratedNever();
    }
}
