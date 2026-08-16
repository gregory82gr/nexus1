using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permission", "Security");
        builder.HasKey(x => x.Id).HasName("PK_Security_Permission");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PermissionId(value))
            .HasColumnName("PermissionId")
            .ValueGeneratedNever();

        builder.Property(x => x.PermissionCategoryId)
            .HasConversion(id => id.Value, value => new PermissionCategoryId(value))
            .HasColumnName("PermissionCategoryId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(700);
        builder.Property(x => x.ResourceType).HasMaxLength(80);
        builder.Property(x => x.ActionName).HasMaxLength(80).IsRequired();
        builder.Property(x => x.IsSafetyRelevant).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<PermissionCategory>().WithMany().HasForeignKey(x => x.PermissionCategoryId)
            .HasConstraintName("FK_Security_Permission_PermissionCategory")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Security_Permission_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
