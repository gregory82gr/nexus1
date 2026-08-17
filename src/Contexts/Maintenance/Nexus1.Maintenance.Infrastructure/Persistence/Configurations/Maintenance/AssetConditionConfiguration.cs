using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>
/// The atlas DDL (C.9.4.3) gives this table only CreatedAtUtc + CreatedBy
/// (no ModifiedAtUtc/ModifiedBy/IsDeleted/RowVersion), verified directly
/// against the DDL. AssessedAtUtc is the real business timestamp and is
/// domain-modeled; CreatedAtUtc is pure row-insertion bookkeeping with a SQL
/// DEFAULT and is mapped as a shadow column only, mirroring TwinSnapshot's
/// own SnapshotAtUtc/CreatedAtUtc split. CreatedBy has no SQL DEFAULT in the
/// atlas, so it gets the same HasDefaultValueSql("N'system'") shadow
/// treatment as AssetConfiguration's CreatedBy/ModifiedBy.
/// </summary>
public sealed class AssetConditionConfiguration : IEntityTypeConfiguration<AssetCondition>
{
    public void Configure(EntityTypeBuilder<AssetCondition> builder)
    {
        builder.ToTable("AssetCondition", "Maintenance", tb => tb.HasCheckConstraint(
            "CK_Maintenance_AssetCondition_HealthScore",
            "[HealthScorePercent] IS NULL OR ([HealthScorePercent] >= 0 AND [HealthScorePercent] <= 100)"));
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_AssetCondition");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AssetConditionId(value))
            .HasColumnName("AssetConditionId")
            .ValueGeneratedNever();

        builder.Property(x => x.AssetId)
            .HasConversion(id => id.Value, value => new AssetId(value))
            .HasColumnName("AssetId")
            .IsRequired();

        builder.Property(x => x.ConditionGradeId)
            .HasConversion(id => id.Value, value => new ConditionGradeId(value))
            .HasColumnName("ConditionGradeId")
            .IsRequired();

        builder.Property(x => x.AssessedAtUtc).HasColumnName("AssessedAtUtc").IsRequired();
        builder.Property(x => x.AssessedByUserId).HasColumnName("AssessedByUserId");
        builder.Property(x => x.HealthScorePercent).HasColumnName("HealthScorePercent").HasColumnType("decimal(5,2)");
        builder.Property(x => x.RemainingUsefulLifeDays).HasColumnName("RemainingUsefulLifeDays");
        builder.Property(x => x.Basis).HasColumnName("Basis").HasMaxLength(500);
        builder.Property(x => x.Notes).HasColumnName("Notes").HasMaxLength(2000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .HasConstraintName("FK_Maintenance_AssetCondition_Asset")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ConditionGrade>()
            .WithMany()
            .HasForeignKey(x => x.ConditionGradeId)
            .HasConstraintName("FK_Maintenance_AssetCondition_ConditionGrade")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
