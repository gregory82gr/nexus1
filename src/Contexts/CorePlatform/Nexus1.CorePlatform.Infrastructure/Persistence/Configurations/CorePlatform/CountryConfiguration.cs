using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Country", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_Country");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new CountryId(value))
            .HasColumnName("CountryId")
            .ValueGeneratedNever();

        builder.Property(x => x.Iso2Code).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(x => x.Iso3Code).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.NumericCode).HasMaxLength(3).IsFixedLength();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.OfficialName).HasMaxLength(250);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Iso2Code).IsUnique().HasDatabaseName("UQ_CorePlatform_Country_Iso2Code");
        builder.HasIndex(x => x.Iso3Code).IsUnique().HasDatabaseName("UQ_CorePlatform_Country_Iso3Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
