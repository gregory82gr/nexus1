using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>
/// VerifiedByUserId is a plain passport column with no FK constraint —
/// Security lives in its own SecurityDb while Organization gets its own
/// OrganizationDb, so a real cross-database FOREIGN KEY is not possible
/// (ADR-017).
/// </summary>
public sealed class PersonQualificationConfiguration : IEntityTypeConfiguration<PersonQualification>
{
    public void Configure(EntityTypeBuilder<PersonQualification> builder)
    {
        builder.ToTable("PersonQualification", "Organization", t => t.HasCheckConstraint(
            "CK_Organization_PersonQualification_Expiry",
            "[ExpiresAtUtc] IS NULL OR [IssuedAtUtc] IS NULL OR [ExpiresAtUtc] > [IssuedAtUtc]"));
        builder.HasKey(x => x.Id).HasName("PK_Organization_PersonQualification");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PersonQualificationId(value))
            .HasColumnName("PersonQualificationId")
            .ValueGeneratedNever();

        builder.Property(x => x.PersonId)
            .HasConversion(id => id.Value, value => new PersonId(value))
            .HasColumnName("PersonId")
            .IsRequired();

        builder.Property(x => x.QualificationId)
            .HasConversion(id => id.Value, value => new QualificationId(value))
            .HasColumnName("QualificationId")
            .IsRequired();

        builder.Property(x => x.QualificationStatusId)
            .HasConversion(id => id.Value, value => new QualificationStatusId(value))
            .HasColumnName("QualificationStatusId")
            .IsRequired();

        builder.Property(x => x.IssuedAtUtc);
        builder.Property(x => x.ExpiresAtUtc);
        builder.Property(x => x.VerifiedAtUtc);
        builder.Property(x => x.VerifiedByUserId).HasColumnName("VerifiedByUserId");
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<Person>().WithMany().HasForeignKey(x => x.PersonId)
            .HasConstraintName("FK_Organization_PersonQualification_Person")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Qualification>().WithMany().HasForeignKey(x => x.QualificationId)
            .HasConstraintName("FK_Organization_PersonQualification_Qualification")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<QualificationStatus>().WithMany().HasForeignKey(x => x.QualificationStatusId)
            .HasConstraintName("FK_Organization_PersonQualification_QualificationStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
