using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>No audit columns at all per the atlas DDL (C.9.4.3) — verified directly.</summary>
public sealed class DegradationTrendPointConfiguration : IEntityTypeConfiguration<DegradationTrendPoint>
{
    public void Configure(EntityTypeBuilder<DegradationTrendPoint> builder)
    {
        builder.ToTable("DegradationTrendPoint", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_DegradationTrendPoint");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DegradationTrendPointId(value))
            .HasColumnName("DegradationTrendPointId")
            .ValueGeneratedNever();

        builder.Property(x => x.DegradationRecordId)
            .HasConversion(id => id.Value, value => new DegradationRecordId(value))
            .HasColumnName("DegradationRecordId")
            .IsRequired();

        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId").IsRequired();
        builder.Property(x => x.SourceSignalId).HasColumnName("SourceSignalId");
        builder.Property(x => x.MeasuredAtUtc).HasColumnName("MeasuredAtUtc").IsRequired();
        builder.Property(x => x.Value).HasColumnName("Value").IsRequired();
        builder.Property(x => x.Note).HasColumnName("Note").HasMaxLength(500);

        builder.HasOne<DegradationRecord>()
            .WithMany()
            .HasForeignKey(x => x.DegradationRecordId)
            .HasConstraintName("FK_Maintenance_DegradationTrendPoint_DegradationRecord")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_Maintenance_DegradationTrendPoint_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<InstrumentationSignalReference>()
            .WithMany()
            .HasForeignKey(x => x.SourceSignalId)
            .HasConstraintName("FK_Maintenance_DegradationTrendPoint_SourceSignal")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
