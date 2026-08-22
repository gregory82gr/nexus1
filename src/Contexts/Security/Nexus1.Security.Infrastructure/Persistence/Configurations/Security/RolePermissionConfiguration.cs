using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermission", "Security");

        builder.Property(x => x.ApplicationRoleId)
            .HasConversion(id => id.Value, value => new ApplicationRoleId(value))
            .HasColumnName("ApplicationRoleId")
            .IsRequired();

        builder.Property(x => x.PermissionId)
            .HasConversion(id => id.Value, value => new PermissionId(value))
            .HasColumnName("PermissionId")
            .IsRequired();

        builder.HasKey(x => new { x.ApplicationRoleId, x.PermissionId }).HasName("PK_Security_RolePermission");

        builder.Property(x => x.IsGranted).IsRequired();
        builder.Property(x => x.GrantedAtUtc).IsRequired();

        builder.Property(x => x.GrantedByUserId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new ApplicationUserId(value.Value) : (ApplicationUserId?)null)
            .HasColumnName("GrantedByUserId");

        builder.Property(x => x.ExpiresAtUtc);

        builder.HasOne<ApplicationRole>().WithMany().HasForeignKey(x => x.ApplicationRoleId)
            .HasConstraintName("FK_Security_RolePermission_ApplicationRole")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Permission>().WithMany().HasForeignKey(x => x.PermissionId)
            .HasConstraintName("FK_Security_RolePermission_Permission")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.GrantedByUserId)
            .HasConstraintName("FK_Security_RolePermission_GrantedByUser")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
