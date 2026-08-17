using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>
/// Full audit shape mapped as EF shadow properties only, same treatment as
/// RadiationZone/RadiationMonitor/Dosimeter (ADR-024). EngineeringUnitId
/// carries a real FK to CorePlatform.EngineeringUnit via
/// CorePlatformEngineeringUnitReference, named FK_DoseLimit_EngineeringUnit
/// verbatim per ADR-024's own evidence-required section.
/// </summary>
public sealed class DoseLimitConfiguration : IEntityTypeConfiguration<DoseLimit>
{
    public void Configure(EntityTypeBuilder<DoseLimit> builder)
    {
        builder.ToTable("DoseLimit", "RadiationMonitoring", t =>
        {
            t.HasCheckConstraint("CK_RadiationMonitoring_DoseLimit_LimitValue", "[LimitValue] >= 0");
            t.HasCheckConstraint("CK_RadiationMonitoring_DoseLimit_PeriodDays", "[PeriodDays] > 0");
        });
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_DoseLimit");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DoseLimitId(value))
            .HasColumnName("DoseLimitId")
            .ValueGeneratedNever();

        builder.Property(x => x.DoseTypeId)
            .HasConversion(id => id.Value, value => new DoseTypeId(value))
            .HasColumnName("DoseTypeId")
            .IsRequired();

        builder.Property(x => x.LimitTypeId)
            .HasConversion(id => id.Value, value => new LimitTypeId(value))
            .HasColumnName("LimitTypeId")
            .IsRequired();

        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId").IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LimitValue).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.PeriodDays).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_RadiationMonitoring_DoseLimit_Code");

        builder.HasOne<DoseType>()
            .WithMany()
            .HasForeignKey(x => x.DoseTypeId)
            .HasConstraintName("FK_RadiationMonitoring_DoseLimit_DoseType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LimitType>()
            .WithMany()
            .HasForeignKey(x => x.LimitTypeId)
            .HasConstraintName("FK_RadiationMonitoring_DoseLimit_LimitType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_DoseLimit_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
