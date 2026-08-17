using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>
/// No audit columns mapped beyond row-insertion bookkeeping (ADR-024).
/// EngineeringUnitId carries a real FK to CorePlatform.EngineeringUnit via
/// CorePlatformEngineeringUnitReference, named
/// FK_PersonDoseReading_EngineeringUnit verbatim per ADR-024's own
/// evidence-required section.
/// </summary>
public sealed class PersonDoseReadingConfiguration : IEntityTypeConfiguration<PersonDoseReading>
{
    public void Configure(EntityTypeBuilder<PersonDoseReading> builder)
    {
        builder.ToTable("PersonDoseReading", "RadiationMonitoring", t =>
        {
            t.HasCheckConstraint("CK_RadiationMonitoring_PersonDoseReading_DoseValue", "[DoseValue] >= 0");
        });
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_PersonDoseReading");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PersonDoseReadingId(value))
            .HasColumnName("PersonDoseReadingId")
            .ValueGeneratedNever();

        builder.Property(x => x.PersonDosimeterAssignmentId)
            .HasConversion(id => id.Value, value => new PersonDosimeterAssignmentId(value))
            .HasColumnName("PersonDosimeterAssignmentId")
            .IsRequired();

        builder.Property(x => x.DoseTypeId)
            .HasConversion(id => id.Value, value => new DoseTypeId(value))
            .HasColumnName("DoseTypeId")
            .IsRequired();

        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId").IsRequired();

        builder.Property(x => x.MeasurementQualityId)
            .HasConversion(id => id.Value, value => new MeasurementQualityId(value))
            .HasColumnName("MeasurementQualityId")
            .IsRequired();

        builder.Property(x => x.ReadingAtUtc).IsRequired();
        builder.Property(x => x.DoseValue).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.IsFinal).IsRequired().HasDefaultValue(false);

        builder.HasOne<PersonDosimeterAssignment>()
            .WithMany()
            .HasForeignKey(x => x.PersonDosimeterAssignmentId)
            .HasConstraintName("FK_RadiationMonitoring_PersonDoseReading_PersonDosimeterAssignment")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DoseType>()
            .WithMany()
            .HasForeignKey(x => x.DoseTypeId)
            .HasConstraintName("FK_RadiationMonitoring_PersonDoseReading_DoseType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_PersonDoseReading_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MeasurementQuality>()
            .WithMany()
            .HasForeignKey(x => x.MeasurementQualityId)
            .HasConstraintName("FK_RadiationMonitoring_PersonDoseReading_MeasurementQuality")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
