using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// No audit columns — append-only fact table (ADR-025), matching
/// RadiationMonitoring.RadiationReadingConfiguration's own treatment.
/// ExerciseId and ObservationSeverityId are real internal FKs, NOT NULL.
/// ObservedByUserId is passport-only — Security.ApplicationUser lives in
/// SecurityDb. ExerciseInjectId is deliberately not mapped at all — out of
/// scope this pass.
/// </summary>
public sealed class ExerciseObservationConfiguration : IEntityTypeConfiguration<ExerciseObservation>
{
    public void Configure(EntityTypeBuilder<ExerciseObservation> builder)
    {
        builder.ToTable("ExerciseObservation", "EmergencyPreparedness");
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_ExerciseObservation");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ExerciseObservationId(value))
            .HasColumnName("ExerciseObservationId")
            .ValueGeneratedNever();

        builder.Property(x => x.ExerciseId)
            .HasConversion(id => id.Value, value => new ExerciseId(value))
            .HasColumnName("ExerciseId")
            .IsRequired();

        builder.Property(x => x.ObservationSeverityId)
            .HasConversion(id => id.Value, value => new ObservationSeverityId(value))
            .HasColumnName("ObservationSeverityId")
            .IsRequired();

        builder.Property(x => x.ObservedByUserId).HasColumnName("ObservedByUserId").IsRequired();
        builder.Property(x => x.ObservedAtUtc).IsRequired();
        builder.Property(x => x.FindingText).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.CorrectiveActionRequired).IsRequired().HasDefaultValue(false);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .HasConstraintName("FK_EmergencyPreparedness_ExerciseObservation_Exercise")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ObservationSeverity>()
            .WithMany()
            .HasForeignKey(x => x.ObservationSeverityId)
            .HasConstraintName("FK_EmergencyPreparedness_ExerciseObservation_ObservationSeverity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
