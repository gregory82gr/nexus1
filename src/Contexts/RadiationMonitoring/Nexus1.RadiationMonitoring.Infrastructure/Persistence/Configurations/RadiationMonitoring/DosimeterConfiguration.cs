using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>Full audit shape mapped as EF shadow properties only, same treatment as RadiationZone/RadiationMonitor (ADR-024).</summary>
public sealed class DosimeterConfiguration : IEntityTypeConfiguration<Dosimeter>
{
    public void Configure(EntityTypeBuilder<Dosimeter> builder)
    {
        builder.ToTable("Dosimeter", "RadiationMonitoring");
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_Dosimeter");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DosimeterId(value))
            .HasColumnName("DosimeterId")
            .ValueGeneratedNever();

        builder.Property(x => x.DosimeterTypeId)
            .HasConversion(id => id.Value, value => new DosimeterTypeId(value))
            .HasColumnName("DosimeterTypeId")
            .IsRequired();

        builder.Property(x => x.DosimeterStatusId)
            .HasConversion(id => id.Value, value => new DosimeterStatusId(value))
            .HasColumnName("DosimeterStatusId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SerialNumber).HasMaxLength(120);
        builder.Property(x => x.CalibrationDueAtUtc);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_RadiationMonitoring_Dosimeter_Code");

        builder.HasOne<DosimeterType>()
            .WithMany()
            .HasForeignKey(x => x.DosimeterTypeId)
            .HasConstraintName("FK_RadiationMonitoring_Dosimeter_DosimeterType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DosimeterStatus>()
            .WithMany()
            .HasForeignKey(x => x.DosimeterStatusId)
            .HasConstraintName("FK_RadiationMonitoring_Dosimeter_DosimeterStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
