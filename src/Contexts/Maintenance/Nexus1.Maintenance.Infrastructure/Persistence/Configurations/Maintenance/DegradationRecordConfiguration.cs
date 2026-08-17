using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>
/// Same shadow-property audit treatment as AssetConfiguration for the full
/// audit-column shape (CreatedBy, ModifiedAtUtc, ModifiedBy, RowVersion).
/// DetectedAtUtc is the real business timestamp and is domain-modeled;
/// CreatedAtUtc is pure row-insertion bookkeeping and is mapped as a shadow
/// column only. IsActive/ClosedAtUtc ARE domain-modeled — DegradationRecord's
/// defining open/close lifecycle (ADR-021).
/// </summary>
public sealed class DegradationRecordConfiguration : IEntityTypeConfiguration<DegradationRecord>
{
    public void Configure(EntityTypeBuilder<DegradationRecord> builder)
    {
        builder.ToTable("DegradationRecord", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_DegradationRecord");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DegradationRecordId(value))
            .HasColumnName("DegradationRecordId")
            .ValueGeneratedNever();

        builder.Property(x => x.AssetId)
            .HasConversion(id => id.Value, value => new AssetId(value))
            .HasColumnName("AssetId")
            .IsRequired();

        builder.Property(x => x.AssetComponentId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new AssetComponentId(value.Value) : (AssetComponentId?)null)
            .HasColumnName("AssetComponentId");

        builder.Property(x => x.DegradationMechanismId)
            .HasConversion(id => id.Value, value => new DegradationMechanismId(value))
            .HasColumnName("DegradationMechanismId")
            .IsRequired();

        builder.Property(x => x.FindingSeverityId)
            .HasConversion(id => id.Value, value => new FindingSeverityId(value))
            .HasColumnName("FindingSeverityId")
            .IsRequired();

        builder.Property(x => x.ConditionGradeId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new ConditionGradeId(value.Value) : (ConditionGradeId?)null)
            .HasColumnName("ConditionGradeId");

        builder.Property(x => x.DetectedAtUtc).HasColumnName("DetectedAtUtc").IsRequired();
        builder.Property(x => x.DetectedByUserId).HasColumnName("DetectedByUserId");
        builder.Property(x => x.Description).HasColumnName("Description").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.EstimatedRatePerYear).HasColumnName("EstimatedRatePerYear").HasColumnType("decimal(12,6)");
        builder.Property(x => x.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(x => x.ClosedAtUtc).HasColumnName("ClosedAtUtc");

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .HasConstraintName("FK_Maintenance_DegradationRecord_Asset")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AssetComponent>()
            .WithMany()
            .HasForeignKey(x => x.AssetComponentId)
            .HasConstraintName("FK_Maintenance_DegradationRecord_AssetComponent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DegradationMechanism>()
            .WithMany()
            .HasForeignKey(x => x.DegradationMechanismId)
            .HasConstraintName("FK_Maintenance_DegradationRecord_DegradationMechanism")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FindingSeverity>()
            .WithMany()
            .HasForeignKey(x => x.FindingSeverityId)
            .HasConstraintName("FK_Maintenance_DegradationRecord_FindingSeverity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ConditionGrade>()
            .WithMany()
            .HasForeignKey(x => x.ConditionGradeId)
            .HasConstraintName("FK_Maintenance_DegradationRecord_ConditionGrade")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
