using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>
/// CountryId is a plain passport column with no FK constraint —
/// CorePlatform lives in AlarmManagementDb while Organization gets its own
/// OrganizationDb, so a real cross-database FOREIGN KEY is not possible
/// (ADR-017).
/// </summary>
public sealed class LegalEntityConfiguration : IEntityTypeConfiguration<LegalEntity>
{
    public void Configure(EntityTypeBuilder<LegalEntity> builder)
    {
        builder.ToTable("LegalEntity", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_LegalEntity");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new LegalEntityId(value))
            .HasColumnName("LegalEntityId")
            .ValueGeneratedNever();

        builder.Property(x => x.LegalEntityTypeId)
            .HasConversion(id => id.Value, value => new LegalEntityTypeId(value))
            .HasColumnName("LegalEntityTypeId")
            .IsRequired();

        builder.Property(x => x.ParentLegalEntityId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new LegalEntityId(value.Value) : (LegalEntityId?)null)
            .HasColumnName("ParentLegalEntityId");

        builder.Property(x => x.CountryId).HasColumnName("CountryId");

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.RegistrationNumber).HasMaxLength(100);
        builder.Property(x => x.TaxIdentifier).HasMaxLength(100);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(300);
        builder.Property(x => x.IsOperator).IsRequired();
        builder.Property(x => x.IsVendor).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Organization_LegalEntity_Code");
        builder.HasIndex(x => x.LegalEntityTypeId).HasDatabaseName("IX_Organization_LegalEntity_LegalEntityTypeId");
        builder.HasIndex(x => x.CountryId).HasDatabaseName("IX_Organization_LegalEntity_CountryId");

        builder.HasOne<LegalEntityType>().WithMany().HasForeignKey(x => x.LegalEntityTypeId)
            .HasConstraintName("FK_Organization_LegalEntity_LegalEntityType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LegalEntity>().WithMany().HasForeignKey(x => x.ParentLegalEntityId)
            .HasConstraintName("FK_Organization_LegalEntity_ParentLegalEntity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
