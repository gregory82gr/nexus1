using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>Full audit shape mapped as EF shadow properties only (ADR-026).</summary>
public sealed class RewardFunctionConfiguration : IEntityTypeConfiguration<RewardFunction>
{
    public void Configure(EntityTypeBuilder<RewardFunction> builder)
    {
        builder.ToTable("RewardFunction", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_RewardFunction");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RewardFunctionId(value))
            .HasColumnName("RewardFunctionId")
            .ValueGeneratedNever();

        builder.Property(x => x.RewardFunctionTypeId)
            .HasConversion(id => id.Value, value => new RewardFunctionTypeId(value))
            .HasColumnName("RewardFunctionTypeId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FormulaText).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ErrorWeight).HasColumnType("decimal(18,6)").IsRequired().HasDefaultValue(100.0m);
        builder.Property(x => x.MovePenalty).HasColumnType("decimal(18,6)").IsRequired().HasDefaultValue(0.3m);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_RewardFunction_Code");

        builder.HasOne<RewardFunctionType>()
            .WithMany()
            .HasForeignKey(x => x.RewardFunctionTypeId)
            .HasConstraintName("FK_ReinforcementLearning_RewardFunction_RewardFunctionType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
