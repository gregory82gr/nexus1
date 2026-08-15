using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence.Configurations.AlarmManagement;

public sealed class AlarmDefinitionConfiguration : IEntityTypeConfiguration<AlarmDefinition>
{
    public void Configure(EntityTypeBuilder<AlarmDefinition> builder)
    {
        builder.ToTable("AlarmDefinition", "AlarmManagement");
        builder.HasKey(x => x.Id).HasName("PK_AlarmManagement_AlarmDefinition");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AlarmDefinitionId(value))
            .HasColumnName("AlarmDefinitionId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId)
            .HasConversion(id => id.Value, value => new UnitId(value))
            .HasColumnName("UnitId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();

        // Atlas normalizes severity into its own AlarmSeverity lookup table
        // (ADR-004 deferred that; stored as its string name here instead).
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(x => x.ThresholdValue).HasColumnType("decimal(18,6)").IsRequired();

        builder.HasIndex(x => new { x.UnitId, x.Code }).IsUnique().HasDatabaseName("UX_AlarmManagement_AlarmDefinition_UnitId_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
