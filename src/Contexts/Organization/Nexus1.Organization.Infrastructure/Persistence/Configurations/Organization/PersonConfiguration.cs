using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>
/// ApplicationUserId is a plain passport column with no FK constraint —
/// Security lives in its own SecurityDb while Organization gets its own
/// OrganizationDb, so a real cross-database FOREIGN KEY is not possible
/// (ADR-017). The atlas's own real UNIQUE constraint on ApplicationUserId
/// is local to this database and is preserved.
/// </summary>
public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Person", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_Person");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PersonId(value))
            .HasColumnName("PersonId")
            .ValueGeneratedNever();

        builder.Property(x => x.PersonTypeId)
            .HasConversion(id => id.Value, value => new PersonTypeId(value))
            .HasColumnName("PersonTypeId")
            .IsRequired();

        builder.Property(x => x.ApplicationUserId).HasColumnName("ApplicationUserId");

        builder.Property(x => x.LegalEntityId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new LegalEntityId(value.Value) : (LegalEntityId?)null)
            .HasColumnName("LegalEntityId");

        builder.Property(x => x.PersonnelNumber).HasMaxLength(80);
        builder.Property(x => x.GivenName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FamilyName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.WorkEmail).HasMaxLength(256);
        builder.Property(x => x.WorkPhone).HasMaxLength(80);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.PersonnelNumber).IsUnique().HasDatabaseName("UQ_Organization_Person_PersonnelNumber");
        builder.HasIndex(x => x.ApplicationUserId).IsUnique().HasDatabaseName("UQ_Organization_Person_ApplicationUserId");

        builder.HasOne<PersonType>().WithMany().HasForeignKey(x => x.PersonTypeId)
            .HasConstraintName("FK_Organization_Person_PersonType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId)
            .HasConstraintName("FK_Organization_Person_LegalEntity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
