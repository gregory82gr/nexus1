using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

public sealed class PermissionCategoryConfiguration : IEntityTypeConfiguration<PermissionCategory>
{
    public void Configure(EntityTypeBuilder<PermissionCategory> builder)
    {
        builder.ToTable("PermissionCategory", "Security");
        builder.HasKey(x => x.Id).HasName("PK_Security_PermissionCategory");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PermissionCategoryId(value))
            .HasColumnName("PermissionCategoryId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Security_PermissionCategory_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
