using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>Same shadow-property audit treatment as AssetConfiguration — see its XML doc for the full rationale. AssetComponent's DDL also carries IsDeleted, but no Application operation needs it, so it is not domain-modeled (unlike Asset/WorkOrder) and is mapped here as a shadow property only.</summary>
public sealed class AssetComponentConfiguration : IEntityTypeConfiguration<AssetComponent>
{
    public void Configure(EntityTypeBuilder<AssetComponent> builder)
    {
        builder.ToTable("AssetComponent", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_AssetComponent");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AssetComponentId(value))
            .HasColumnName("AssetComponentId")
            .ValueGeneratedNever();

        builder.Property(x => x.AssetId)
            .HasConversion(id => id.Value, value => new AssetId(value))
            .HasColumnName("AssetId")
            .IsRequired();

        builder.Property(x => x.ParentAssetComponentId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new AssetComponentId(value.Value) : (AssetComponentId?)null)
            .HasColumnName("ParentAssetComponentId");

        builder.Property(x => x.ComponentCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsReplaceable).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.AssetId, x.ComponentCode }).IsUnique()
            .HasDatabaseName("UQ_Maintenance_AssetComponent_Asset_ComponentCode");

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .HasConstraintName("FK_Maintenance_AssetComponent_Asset")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AssetComponent>()
            .WithMany()
            .HasForeignKey(x => x.ParentAssetComponentId)
            .HasConstraintName("FK_Maintenance_AssetComponent_Parent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
