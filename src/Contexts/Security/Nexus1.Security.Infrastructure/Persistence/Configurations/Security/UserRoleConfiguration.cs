using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole", "Security");

        builder.Property(x => x.ApplicationUserId)
            .HasConversion(id => id.Value, value => new ApplicationUserId(value))
            .HasColumnName("ApplicationUserId")
            .IsRequired();

        builder.Property(x => x.ApplicationRoleId)
            .HasConversion(id => id.Value, value => new ApplicationRoleId(value))
            .HasColumnName("ApplicationRoleId")
            .IsRequired();

        builder.HasKey(x => new { x.ApplicationUserId, x.ApplicationRoleId }).HasName("PK_Security_UserRole");

        builder.Property(x => x.AssignedAtUtc).IsRequired();

        builder.Property(x => x.AssignedByUserId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new ApplicationUserId(value.Value) : (ApplicationUserId?)null)
            .HasColumnName("AssignedByUserId");

        builder.Property(x => x.ExpiresAtUtc);
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ApplicationUserId)
            .HasConstraintName("FK_Security_UserRole_ApplicationUser")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationRole>().WithMany().HasForeignKey(x => x.ApplicationRoleId)
            .HasConstraintName("FK_Security_UserRole_ApplicationRole")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AssignedByUserId)
            .HasConstraintName("FK_Security_UserRole_AssignedByUser")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
