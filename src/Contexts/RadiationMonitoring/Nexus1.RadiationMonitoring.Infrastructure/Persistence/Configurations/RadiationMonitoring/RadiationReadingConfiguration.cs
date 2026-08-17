using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>
/// No audit columns — append-only fact table (ADR-024). EngineeringUnitId
/// carries a real FK to CorePlatform.EngineeringUnit via
/// CorePlatformEngineeringUnitReference, named FK_RadiationReading_EngineeringUnit
/// verbatim per ADR-024's own evidence-required section.
/// </summary>
public sealed class RadiationReadingConfiguration : IEntityTypeConfiguration<RadiationReading>
{
    public void Configure(EntityTypeBuilder<RadiationReading> builder)
    {
        builder.ToTable("RadiationReading", "RadiationMonitoring");
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_RadiationReading");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RadiationReadingId(value))
            .HasColumnName("RadiationReadingId")
            .ValueGeneratedNever();

        builder.Property(x => x.RadiationMonitorId)
            .HasConversion(id => id.Value, value => new RadiationMonitorId(value))
            .HasColumnName("RadiationMonitorId")
            .IsRequired();

        builder.Property(x => x.MeasurementTypeId)
            .HasConversion(id => id.Value, value => new MeasurementTypeId(value))
            .HasColumnName("MeasurementTypeId")
            .IsRequired();

        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId").IsRequired();

        builder.Property(x => x.MeasurementQualityId)
            .HasConversion(id => id.Value, value => new MeasurementQualityId(value))
            .HasColumnName("MeasurementQualityId")
            .IsRequired();

        builder.Property(x => x.TimestampUtc).IsRequired();
        builder.Property(x => x.Value).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.IsAlarmRelevant).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.SourceTimestampUtc);

        builder.HasOne<RadiationMonitor>()
            .WithMany()
            .HasForeignKey(x => x.RadiationMonitorId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationReading_RadiationMonitor")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MeasurementType>()
            .WithMany()
            .HasForeignKey(x => x.MeasurementTypeId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationReading_MeasurementType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_RadiationReading_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MeasurementQuality>()
            .WithMany()
            .HasForeignKey(x => x.MeasurementQualityId)
            .HasConstraintName("FK_RadiationMonitoring_RadiationReading_MeasurementQuality")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
