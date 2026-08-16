using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

public sealed class RoleTypeConfiguration : IEntityTypeConfiguration<RoleType>
{
    public void Configure(EntityTypeBuilder<RoleType> builder)
    {
        builder.ToTable("RoleType", "Security");
        builder.HasKey(x => x.Id).HasName("PK_Security_RoleType");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RoleTypeId(value))
            .HasColumnName("RoleTypeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Security_RoleType_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
