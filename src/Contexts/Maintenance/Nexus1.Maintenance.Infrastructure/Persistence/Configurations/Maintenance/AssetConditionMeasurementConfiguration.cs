using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>No audit columns at all per the atlas DDL (C.9.4.3) — verified directly, the leanest table in the sector.</summary>
public sealed class AssetConditionMeasurementConfiguration : IEntityTypeConfiguration<AssetConditionMeasurement>
{
    public void Configure(EntityTypeBuilder<AssetConditionMeasurement> builder)
    {
        builder.ToTable("AssetConditionMeasurement", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_AssetConditionMeasurement");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AssetConditionMeasurementId(value))
            .HasColumnName("AssetConditionMeasurementId")
            .ValueGeneratedNever();

        builder.Property(x => x.AssetConditionId)
            .HasConversion(id => id.Value, value => new AssetConditionId(value))
            .HasColumnName("AssetConditionId")
            .IsRequired();

        builder.Property(x => x.SignalId).HasColumnName("SignalId");
        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId").IsRequired();
        builder.Property(x => x.MeasuredValue).HasColumnName("MeasuredValue").IsRequired();
        builder.Property(x => x.MeasuredAtUtc).HasColumnName("MeasuredAtUtc").IsRequired();
        builder.Property(x => x.MeasurementNote).HasColumnName("MeasurementNote").HasMaxLength(500);

        builder.HasOne<AssetCondition>()
            .WithMany()
            .HasForeignKey(x => x.AssetConditionId)
            .HasConstraintName("FK_Maintenance_AssetConditionMeasurement_AssetCondition")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<InstrumentationSignalReference>()
            .WithMany()
            .HasForeignKey(x => x.SignalId)
            .HasConstraintName("FK_Maintenance_AssetConditionMeasurement_Signal")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_Maintenance_AssetConditionMeasurement_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
