using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.ToTable("Plant", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_Plant");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PlantId(value))
            .HasColumnName("PlantId")
            .ValueGeneratedNever();

        builder.Property(x => x.SiteId)
            .HasConversion(id => id.Value, value => new SiteId(value))
            .HasColumnName("SiteId")
            .IsRequired();

        builder.Property(x => x.PlantTypeId)
            .HasConversion(id => id.Value, value => new PlantTypeId(value))
            .HasColumnName("PlantTypeId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.OperationalStartDate).HasColumnType("date");
        builder.Property(x => x.IsOperational).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Organization_Plant_Code");
        builder.HasIndex(x => x.SiteId).HasDatabaseName("IX_Organization_Plant_SiteId");

        builder.HasOne<Site>().WithMany().HasForeignKey(x => x.SiteId)
            .HasConstraintName("FK_Organization_Plant_Site")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PlantType>().WithMany().HasForeignKey(x => x.PlantTypeId)
            .HasConstraintName("FK_Organization_Plant_PlantType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
