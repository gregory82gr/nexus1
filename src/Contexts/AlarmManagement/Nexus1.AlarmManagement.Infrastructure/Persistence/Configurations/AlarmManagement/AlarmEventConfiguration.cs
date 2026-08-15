using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence.Configurations.AlarmManagement;

/// <summary>
/// Column shape matches the atlas's real AlarmEvent table, not the book's
/// simplified example (ADR-004). No FK to AlarmDefinition is enforced at the
/// database level beyond the passport column, matching UnitPowerSnapshot's
/// treatment in ReactorFleet — an append-only fact table.
/// </summary>
public sealed class AlarmEventConfiguration : IEntityTypeConfiguration<AlarmEvent>
{
    public void Configure(EntityTypeBuilder<AlarmEvent> builder)
    {
        builder.ToTable("AlarmEvent", "AlarmManagement");
        builder.HasKey(x => x.Id).HasName("PK_AlarmManagement_AlarmEvent");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AlarmEventId(value))
            .HasColumnName("AlarmEventId")
            .ValueGeneratedNever();

        builder.Property(x => x.AlarmDefinitionId)
            .HasConversion(id => id.Value, value => new AlarmDefinitionId(value))
            .HasColumnName("AlarmDefinitionId")
            .IsRequired();

        builder.Property(x => x.UnitId)
            .HasConversion(id => id.Value, value => new UnitId(value))
            .HasColumnName("UnitId")
            .IsRequired();

        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.State).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(x => x.RaisedAtUtc).IsRequired();
        builder.Property(x => x.SourceValue).HasColumnType("decimal(18,6)");
        builder.Property(x => x.ThresholdValue).HasColumnType("decimal(18,6)");
        builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();

        builder.Property(x => x.AcknowledgedBy)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : (UserId?)null)
            .HasColumnName("AcknowledgedByUserId");

        builder.Property(x => x.AcknowledgedAtUtc);

        builder.HasIndex(x => new { x.UnitId, x.RaisedAtUtc }).HasDatabaseName("IX_AlarmManagement_AlarmEvent_UnitId_RaisedAtUtc");
        builder.HasIndex(x => new { x.AlarmDefinitionId, x.RaisedAtUtc }).HasDatabaseName("IX_AlarmManagement_AlarmEvent_AlarmDefinitionId_RaisedAtUtc");

        builder.Ignore(x => x.DomainEvents);
    }
}
