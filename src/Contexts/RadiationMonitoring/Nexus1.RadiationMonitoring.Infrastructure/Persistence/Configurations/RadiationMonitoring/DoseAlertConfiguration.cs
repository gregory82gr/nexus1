using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>
/// No audit columns mapped beyond row-insertion bookkeeping (ADR-024).
/// AcknowledgedByUserId stays passport-only — Security.ApplicationUser, a
/// different physical database. PersonDoseReadingId is a real internal FK,
/// nullable.
/// </summary>
public sealed class DoseAlertConfiguration : IEntityTypeConfiguration<DoseAlert>
{
    public void Configure(EntityTypeBuilder<DoseAlert> builder)
    {
        builder.ToTable("DoseAlert", "RadiationMonitoring");
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_DoseAlert");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DoseAlertId(value))
            .HasColumnName("DoseAlertId")
            .ValueGeneratedNever();

        builder.Property(x => x.DoseLimitId)
            .HasConversion(id => id.Value, value => new DoseLimitId(value))
            .HasColumnName("DoseLimitId")
            .IsRequired();

        builder.Property(x => x.PersonDoseReadingId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (long?)null,
                value => value.HasValue ? new PersonDoseReadingId(value.Value) : (PersonDoseReadingId?)null)
            .HasColumnName("PersonDoseReadingId");

        builder.Property(x => x.AlertStatusId)
            .HasConversion(id => id.Value, value => new AlertStatusId(value))
            .HasColumnName("AlertStatusId")
            .IsRequired();

        builder.Property(x => x.AcknowledgedByUserId).HasColumnName("AcknowledgedByUserId");
        builder.Property(x => x.AlertAtUtc).IsRequired();
        builder.Property(x => x.AcknowledgedAtUtc);
        builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();

        builder.HasOne<DoseLimit>()
            .WithMany()
            .HasForeignKey(x => x.DoseLimitId)
            .HasConstraintName("FK_RadiationMonitoring_DoseAlert_DoseLimit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PersonDoseReading>()
            .WithMany()
            .HasForeignKey(x => x.PersonDoseReadingId)
            .HasConstraintName("FK_RadiationMonitoring_DoseAlert_PersonDoseReading")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AlertStatus>()
            .WithMany()
            .HasForeignKey(x => x.AlertStatusId)
            .HasConstraintName("FK_RadiationMonitoring_DoseAlert_AlertStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
