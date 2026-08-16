using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Region", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_Region");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RegionId(value))
            .HasColumnName("RegionId")
            .ValueGeneratedNever();

        builder.Property(x => x.CountryId)
            .HasConversion(id => id.Value, value => new CountryId(value))
            .HasColumnName("CountryId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.RegionType).HasMaxLength(50);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        // Atlas declares a plain FK with no ON DELETE clause (SQL Server default:
        // NO ACTION) — EF Core's own default for a required FK is CASCADE.
        // Restrict matches the atlas's actual behavior (see LocalizationConfiguration).
        builder.HasOne<Country>().WithMany().HasForeignKey(x => x.CountryId)
            .HasConstraintName("FK_CorePlatform_Region_Country")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CountryId, x.Code }).IsUnique().HasDatabaseName("UQ_CorePlatform_Region_Country_Code");
        builder.HasIndex(x => x.CountryId).HasDatabaseName("IX_CorePlatform_Region_CountryId");

        builder.Ignore(x => x.DomainEvents);
    }
}
