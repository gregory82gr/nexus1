using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>
/// Full audit shape mapped as EF shadow properties only, same treatment as
/// RadiationZoneConfiguration (ADR-024). UnitId carries a real FK to
/// ReactorFleet.Unit via ReactorFleetUnitReference, named FK_RadiationMonitor_Unit
/// verbatim per ADR-024's own evidence-required section. EquipmentId is
/// deliberately not mapped to any FK — its target table does not exist in
/// this pass (ADR-024). RadiationZoneId is a real internal FK, nullable.
/// </summary>
public sealed class RadiationMonitorConfiguration : IEntityTypeConfiguration<RadiationMonitor>
{
    public void Configure(EntityTypeBuilder<RadiationMonitor> builder)
    {
        builder.ToTable("RadiationMonitor", "RadiationMonitoring");
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_RadiationMonitor");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RadiationMonitorId(value))
            .HasColumnName("RadiationMonitorId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId");
        builder.Property(x => x.EquipmentId).HasColumnName("EquipmentId");

        builder.Property(x => x.RadiationZoneId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new RadiationZoneId(value.Value) : (RadiationZoneId?)null)
            .HasColumnName("RadiationZoneId");

        builder.Property(x => x.MonitorTypeId)
            .HasConversion(id => id.Value, value => new MonitorTypeId(value))
            .HasColumnName("MonitorTypeId")
            .IsRequired();

        builder.Property(x => x.MonitorStatusId)
            .HasConversion(id => id.Value, value => new MonitorStatusId(value))
            .HasColumnName("MonitorStatusId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SerialNumber).HasMaxLength(120);
        builder.Property(x => x.InstalledAtUtc);
        builder.Property(x => x.CalibrationDueAtUtc);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_RadiationMonitoring_RadiationMonitor_Code");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_RadiationMonitor_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RadiationZone>()
            .WithMany()
            .HasForeignKey(x => x.RadiationZoneId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationMonitor_RadiationZone")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MonitorType>()
            .WithMany()
            .HasForeignKey(x => x.MonitorTypeId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationMonitor_MonitorType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MonitorStatus>()
            .WithMany()
            .HasForeignKey(x => x.MonitorStatusId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationMonitor_MonitorStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
