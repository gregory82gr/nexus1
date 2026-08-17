using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>
/// No audit columns mapped — deliberate restraint (ADR-024). PersonId and
/// AssignedByUserId stay passport-only — Organization.Person/
/// Security.ApplicationUser, different physical databases. DosimeterId is a
/// real internal FK and NOT NULL.
/// </summary>
public sealed class PersonDosimeterAssignmentConfiguration : IEntityTypeConfiguration<PersonDosimeterAssignment>
{
    public void Configure(EntityTypeBuilder<PersonDosimeterAssignment> builder)
    {
        builder.ToTable("PersonDosimeterAssignment", "RadiationMonitoring", t =>
        {
            t.HasCheckConstraint(
                "CK_RadiationMonitoring_PersonDosimeterAssignment_ReturnedAfterAssigned",
                "[ReturnedAtUtc] IS NULL OR [ReturnedAtUtc] > [AssignedAtUtc]");
        });
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_PersonDosimeterAssignment");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PersonDosimeterAssignmentId(value))
            .HasColumnName("PersonDosimeterAssignmentId")
            .ValueGeneratedNever();

        builder.Property(x => x.PersonId).HasColumnName("PersonId").IsRequired();

        builder.Property(x => x.DosimeterId)
            .HasConversion(id => id.Value, value => new DosimeterId(value))
            .HasColumnName("DosimeterId")
            .IsRequired();

        builder.Property(x => x.AssignedByUserId).HasColumnName("AssignedByUserId");
        builder.Property(x => x.AssignedAtUtc).IsRequired();
        builder.Property(x => x.ReturnedAtUtc);
        builder.Property(x => x.AssignmentPurpose).HasMaxLength(300);

        builder.HasOne<Dosimeter>()
            .WithMany()
            .HasForeignKey(x => x.DosimeterId)
            .HasConstraintName("FK_RadiationMonitoring_PersonDosimeterAssignment_Dosimeter")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
