using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Building", "Organization", t => t.HasCheckConstraint(
            "CK_Organization_Building_FloorCount", "[FloorCount] IS NULL OR [FloorCount] >= 0"));
        builder.HasKey(x => x.Id).HasName("PK_Organization_Building");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new BuildingId(value))
            .HasColumnName("BuildingId")
            .ValueGeneratedNever();

        builder.Property(x => x.SiteId)
            .HasConversion(id => id.Value, value => new SiteId(value))
            .HasColumnName("SiteId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BuildingUsage).HasMaxLength(150);
        builder.Property(x => x.FloorCount);
        builder.Property(x => x.IsControlledArea).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.SiteId, x.Code }).IsUnique().HasDatabaseName("UQ_Organization_Building_Site_Code");

        builder.HasOne<Site>().WithMany().HasForeignKey(x => x.SiteId)
            .HasConstraintName("FK_Organization_Building_Site")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
