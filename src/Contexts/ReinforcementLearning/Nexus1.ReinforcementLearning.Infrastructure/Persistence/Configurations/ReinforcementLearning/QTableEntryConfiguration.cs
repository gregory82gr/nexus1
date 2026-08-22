using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>No audit columns at all — the real DDL gives this table none (ADR-026). Bigint identity key.</summary>
public sealed class QTableEntryConfiguration : IEntityTypeConfiguration<QTableEntry>
{
    public void Configure(EntityTypeBuilder<QTableEntry> builder)
    {
        builder.ToTable("QTableEntry", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_QTableEntry");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new QTableEntryId(value))
            .HasColumnName("QTableEntryId")
            .ValueGeneratedNever();

        builder.Property(x => x.QTableId)
            .HasConversion(id => id.Value, value => new QTableId(value))
            .HasColumnName("QTableId")
            .IsRequired();

        builder.Property(x => x.StateDefinitionId)
            .HasConversion(id => id.Value, value => new StateDefinitionId(value))
            .HasColumnName("StateDefinitionId")
            .IsRequired();

        builder.Property(x => x.ActionDefinitionId)
            .HasConversion(id => id.Value, value => new ActionDefinitionId(value))
            .HasColumnName("ActionDefinitionId")
            .IsRequired();

        builder.Property(x => x.QValue).HasColumnType("decimal(20,10)").IsRequired();
        builder.Property(x => x.VisitCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.LastUpdatedAtUtc);

        builder.HasIndex(x => new { x.QTableId, x.StateDefinitionId, x.ActionDefinitionId })
            .IsUnique()
            .HasDatabaseName("UQ_ReinforcementLearning_QTableEntry_QTable_State_Action");

        builder.HasOne<QTable>()
            .WithMany()
            .HasForeignKey(x => x.QTableId)
            .HasConstraintName("FK_ReinforcementLearning_QTableEntry_QTable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StateDefinition>()
            .WithMany()
            .HasForeignKey(x => x.StateDefinitionId)
            .HasConstraintName("FK_ReinforcementLearning_QTableEntry_StateDefinition")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ActionDefinitionId)
            .HasConstraintName("FK_ReinforcementLearning_QTableEntry_ActionDefinition")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
