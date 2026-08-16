using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>
/// CountryId/RegionId/TimeZoneId are plain passport columns with no FK
/// constraint — CorePlatform lives in AlarmManagementDb while Organization
/// gets its own OrganizationDb, so real cross-database FOREIGN KEYs are not
/// possible (ADR-017).
/// </summary>
public sealed class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Site", "Organization", t =>
        {
            t.HasCheckConstraint("CK_Organization_Site_Latitude", "[Latitude] IS NULL OR [Latitude] BETWEEN -90 AND 90");
            t.HasCheckConstraint("CK_Organization_Site_Longitude", "[Longitude] IS NULL OR [Longitude] BETWEEN -180 AND 180");
        });
        builder.HasKey(x => x.Id).HasName("PK_Organization_Site");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SiteId(value))
            .HasColumnName("SiteId")
            .ValueGeneratedNever();

        builder.Property(x => x.LegalEntityId)
            .HasConversion(id => id.Value, value => new LegalEntityId(value))
            .HasColumnName("LegalEntityId")
            .IsRequired();

        builder.Property(x => x.SiteTypeId)
            .HasConversion(id => id.Value, value => new SiteTypeId(value))
            .HasColumnName("SiteTypeId")
            .IsRequired();

        builder.Property(x => x.CountryId).HasColumnName("CountryId").IsRequired();
        builder.Property(x => x.RegionId).HasColumnName("RegionId");
        builder.Property(x => x.TimeZoneId).HasColumnName("TimeZoneId").IsRequired();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.AddressLine1).HasMaxLength(250);
        builder.Property(x => x.AddressLine2).HasMaxLength(250);
        builder.Property(x => x.City).HasMaxLength(150);
        builder.Property(x => x.PostalCode).HasMaxLength(50);
        builder.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
        builder.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
        builder.Property(x => x.IsOperational).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Organization_Site_Code");
        builder.HasIndex(x => x.LegalEntityId).HasDatabaseName("IX_Organization_Site_LegalEntityId");
        builder.HasIndex(x => new { x.CountryId, x.RegionId }).HasDatabaseName("IX_Organization_Site_CountryId_RegionId");

        builder.HasOne<LegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId)
            .HasConstraintName("FK_Organization_Site_LegalEntity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SiteType>().WithMany().HasForeignKey(x => x.SiteTypeId)
            .HasConstraintName("FK_Organization_Site_SiteType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
