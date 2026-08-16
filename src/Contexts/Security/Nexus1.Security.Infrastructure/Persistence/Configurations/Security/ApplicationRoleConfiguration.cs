using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRole", "Security");
        builder.HasKey(x => x.Id).HasName("PK_Security_ApplicationRole");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ApplicationRoleId(value))
            .HasColumnName("ApplicationRoleId")
            .ValueGeneratedNever();

        builder.Property(x => x.RoleTypeId)
            .HasConversion(id => id.Value, value => new RoleTypeId(value))
            .HasColumnName("RoleTypeId")
            .IsRequired();

        builder.Property(x => x.ParentRoleId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new ApplicationRoleId(value.Value) : (ApplicationRoleId?)null)
            .HasColumnName("ParentRoleId");

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsBuiltIn).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<RoleType>().WithMany().HasForeignKey(x => x.RoleTypeId)
            .HasConstraintName("FK_Security_ApplicationRole_RoleType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationRole>().WithMany().HasForeignKey(x => x.ParentRoleId)
            .HasConstraintName("FK_Security_ApplicationRole_ParentRole")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.NormalizedName).IsUnique().HasDatabaseName("UQ_Security_ApplicationRole_NormalizedName");

        builder.Ignore(x => x.DomainEvents);
    }
}
