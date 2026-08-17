using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

public sealed class AssetCriticalityConfiguration : IEntityTypeConfiguration<AssetCriticality>
{
    public void Configure(EntityTypeBuilder<AssetCriticality> builder)
    {
        builder.ToTable("AssetCriticality", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_AssetCriticality");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AssetCriticalityId(value))
            .HasColumnName("AssetCriticalityId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Maintenance_AssetCriticality_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
