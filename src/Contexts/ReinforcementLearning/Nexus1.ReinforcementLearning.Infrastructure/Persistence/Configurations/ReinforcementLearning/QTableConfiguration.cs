using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>
/// Audit shape: CreatedAtUtc/CreatedBy/RowVersion only — no ModifiedAtUtc/
/// ModifiedBy/IsDeleted (verified against the real DDL). Not modeled in
/// Domain — EF shadow properties only (ADR-026). SnapshotAtUtc IS
/// domain-modeled (a required constructor param) despite also carrying a
/// SQL DEFAULT, matching every prior sector's own "SQL default exists but
/// Domain still always supplies it" pattern.
/// </summary>
public sealed class QTableConfiguration : IEntityTypeConfiguration<QTable>
{
    public void Configure(EntityTypeBuilder<QTable> builder)
    {
        builder.ToTable("QTable", "ReinforcementLearning", t => t.HasCheckConstraint(
            "CK_ReinforcementLearning_QTable_EntryCount", "[EntryCount] > 0"));
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_QTable");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new QTableId(value))
            .HasColumnName("QTableId")
            .ValueGeneratedNever();

        builder.Property(x => x.TrainingRunId)
            .HasConversion(id => id.Value, value => new TrainingRunId(value))
            .HasColumnName("TrainingRunId")
            .IsRequired();

        builder.Property(x => x.StateSpaceId)
            .HasConversion(id => id.Value, value => new StateSpaceId(value))
            .HasColumnName("StateSpaceId")
            .IsRequired();

        builder.Property(x => x.ActionSpaceId)
            .HasConversion(id => id.Value, value => new ActionSpaceId(value))
            .HasColumnName("ActionSpaceId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SnapshotAtUtc).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.EntryCount).IsRequired();
        builder.Property(x => x.IsFinal).IsRequired().HasDefaultValue(false);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_QTable_Code");

        builder.HasOne<TrainingRun>()
            .WithMany()
            .HasForeignKey(x => x.TrainingRunId)
            .HasConstraintName("FK_ReinforcementLearning_QTable_TrainingRun")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StateSpace>()
            .WithMany()
            .HasForeignKey(x => x.StateSpaceId)
            .HasConstraintName("FK_ReinforcementLearning_QTable_StateSpace")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionSpace>()
            .WithMany()
            .HasForeignKey(x => x.ActionSpaceId)
            .HasConstraintName("FK_ReinforcementLearning_QTable_ActionSpace")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
