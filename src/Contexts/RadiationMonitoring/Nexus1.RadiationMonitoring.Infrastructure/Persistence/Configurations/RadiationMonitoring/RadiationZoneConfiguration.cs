using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>
/// Full audit shape mapped as EF shadow properties only, same treatment as
/// Robotics.RobotConfiguration (ADR-024). UnitId carries a real FK to
/// ReactorFleet.Unit via ReactorFleetUnitReference, named FK_RadiationZone_Unit
/// verbatim per ADR-024's own evidence-required section. EquipmentLocationId
/// is deliberately not mapped to any FK — its target table does not exist in
/// this pass (ADR-024).
/// </summary>
public sealed class RadiationZoneConfiguration : IEntityTypeConfiguration<RadiationZone>
{
    public void Configure(EntityTypeBuilder<RadiationZone> builder)
    {
        builder.ToTable("RadiationZone", "RadiationMonitoring");
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_RadiationZone");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RadiationZoneId(value))
            .HasColumnName("RadiationZoneId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId");
        builder.Property(x => x.EquipmentLocationId).HasColumnName("EquipmentLocationId");

        builder.Property(x => x.RadiationZoneTypeId)
            .HasConversion(id => id.Value, value => new RadiationZoneTypeId(value))
            .HasColumnName("RadiationZoneTypeId")
            .IsRequired();

        builder.Property(x => x.RadiationZoneStatusId)
            .HasConversion(id => id.Value, value => new RadiationZoneStatusId(value))
            .HasColumnName("RadiationZoneStatusId")
            .IsRequired();

        builder.Property(x => x.RadiationAreaClassificationId)
            .HasConversion(id => id.Value, value => new RadiationAreaClassificationId(value))
            .HasColumnName("RadiationAreaClassificationId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsEntryControlled).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.RequiresDosimeter).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.PostedAtUtc);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_RadiationMonitoring_RadiationZone_Code");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_RadiationZone_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RadiationZoneType>()
            .WithMany()
            .HasForeignKey(x => x.RadiationZoneTypeId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationZone_RadiationZoneType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RadiationZoneStatus>()
            .WithMany()
            .HasForeignKey(x => x.RadiationZoneStatusId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationZone_RadiationZoneStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RadiationAreaClassification>()
            .WithMany()
            .HasForeignKey(x => x.RadiationAreaClassificationId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationZone_RadiationAreaClassification")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
