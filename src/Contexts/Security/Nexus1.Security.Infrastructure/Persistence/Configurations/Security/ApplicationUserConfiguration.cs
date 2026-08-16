using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUser", "Security", t => t.HasCheckConstraint(
            "CK_Security_ApplicationUser_AccessFailedCount", "[AccessFailedCount] >= 0"));
        builder.HasKey(x => x.Id).HasName("PK_Security_ApplicationUser");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ApplicationUserId(value))
            .HasColumnName("ApplicationUserId")
            .ValueGeneratedNever();

        builder.Property(x => x.UserStatusId)
            .HasConversion(id => id.Value, value => new UserStatusId(value))
            .HasColumnName("UserStatusId")
            .IsRequired();

        builder.Property(x => x.UserName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedUserName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256);
        builder.Property(x => x.EmailConfirmed).IsRequired();
        builder.Property(x => x.PasswordHash);
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GivenName).HasMaxLength(100);
        builder.Property(x => x.FamilyName).HasMaxLength(100);
        builder.Property(x => x.IsServiceAccount).IsRequired();
        builder.Property(x => x.LockoutEnabled).IsRequired();
        builder.Property(x => x.LockoutEndUtc);
        builder.Property(x => x.AccessFailedCount).IsRequired();
        builder.Property(x => x.LastLoginAtUtc);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<UserStatus>().WithMany().HasForeignKey(x => x.UserStatusId)
            .HasConstraintName("FK_Security_ApplicationUser_UserStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.NormalizedUserName).IsUnique().HasDatabaseName("UQ_Security_ApplicationUser_NormalizedUserName");

        builder.Ignore(x => x.DomainEvents);
    }
}
