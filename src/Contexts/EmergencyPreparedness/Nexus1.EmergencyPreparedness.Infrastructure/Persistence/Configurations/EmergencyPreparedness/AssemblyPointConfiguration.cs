using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// Full audit shape mapped as EF shadow properties only. RadiationZoneId
/// carries a real FK to RadiationMonitoring.RadiationZone via
/// RadiationMonitoringRadiationZoneReference, named
/// FK_AssemblyPoint_RadiationZone verbatim per ADR-025's own
/// evidence-required section. SiteId/PlantId are passport-only —
/// Organization.Site/Plant live in OrganizationDb (ADR-025).
/// </summary>
public sealed class AssemblyPointConfiguration : IEntityTypeConfiguration<AssemblyPoint>
{
    public void Configure(EntityTypeBuilder<AssemblyPoint> builder)
    {
        builder.ToTable("AssemblyPoint", "EmergencyPreparedness", t => t.HasCheckConstraint(
            "CK_EmergencyPreparedness_AssemblyPoint_MaxOccupancy", "[MaxOccupancy] IS NULL OR [MaxOccupancy] >= 0"));
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_AssemblyPoint");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AssemblyPointId(value))
            .HasColumnName("AssemblyPointId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SiteId).HasColumnName("SiteId").IsRequired();
        builder.Property(x => x.PlantId).HasColumnName("PlantId");
        builder.Property(x => x.RadiationZoneId).HasColumnName("RadiationZoneId");
        builder.Property(x => x.MaxOccupancy);
        builder.Property(x => x.IsIndoor).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Latitude).HasColumnType("decimal(9,6)");
        builder.Property(x => x.Longitude).HasColumnType("decimal(9,6)");
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EmergencyPreparedness_AssemblyPoint_Code");

        builder.HasOne<RadiationMonitoringRadiationZoneReference>()
            .WithMany()
            .HasForeignKey(x => x.RadiationZoneId)
            .HasConstraintName("FK_AssemblyPoint_RadiationZone")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
