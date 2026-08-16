using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class LegalEntityTypeConfiguration : IEntityTypeConfiguration<LegalEntityType>
{
    public void Configure(EntityTypeBuilder<LegalEntityType> builder)
    {
        builder.ToTable("LegalEntityType", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_LegalEntityType");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new LegalEntityTypeId(value))
            .HasColumnName("LegalEntityTypeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Organization_LegalEntityType_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
