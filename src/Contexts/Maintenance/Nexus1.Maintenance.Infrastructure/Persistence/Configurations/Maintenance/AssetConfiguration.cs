using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>
/// Asset's atlas DDL (C.9.4.3) carries the full audit-column shape
/// (CreatedBy, ModifiedAtUtc, ModifiedBy, IsDeleted, RowVersion). Same
/// shadow-property treatment as DigitalTwin's TwinModelConfiguration (see
/// its XML doc for the full rationale): CreatedBy/ModifiedBy are
/// NVARCHAR(100) NOT NULL with NO SQL DEFAULT in the atlas, so they are
/// mapped as EF shadow properties with HasDefaultValueSql("N'system'") — a
/// DEFAULT CONSTRAINT the atlas's own text does not specify, added solely so
/// the NOT NULL constraint is satisfiable without inventing an audit-trail
/// feature this phase was never asked to build.
/// </summary>
public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Asset", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_Asset");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AssetId(value))
            .HasColumnName("AssetId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();
        builder.Property(x => x.EquipmentId).HasColumnName("EquipmentId");
        builder.Property(x => x.SystemId).HasColumnName("SystemId");

        builder.Property(x => x.AssetCategoryId)
            .HasConversion(id => id.Value, value => new AssetCategoryId(value))
            .HasColumnName("AssetCategoryId")
            .IsRequired();

        builder.Property(x => x.AssetStatusId)
            .HasConversion(id => id.Value, value => new AssetStatusId(value))
            .HasColumnName("AssetStatusId")
            .IsRequired();

        builder.Property(x => x.AssetCriticalityId)
            .HasConversion(id => id.Value, value => new AssetCriticalityId(value))
            .HasColumnName("AssetCriticalityId")
            .IsRequired();

        builder.Property(x => x.AssetCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.LocationText).HasMaxLength(250);
        builder.Property(x => x.Manufacturer).HasMaxLength(150);
        builder.Property(x => x.ModelNumber).HasMaxLength(120);
        builder.Property(x => x.SerialNumber).HasMaxLength(120);
        builder.Property(x => x.CommissionedAtUtc);
        builder.Property(x => x.RetiredAtUtc);
        builder.Property(x => x.IsSafetyRelated).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.AssetCode).IsUnique().HasDatabaseName("UQ_Maintenance_Asset_AssetCode");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_Maintenance_Asset_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AssetCategory>()
            .WithMany()
            .HasForeignKey(x => x.AssetCategoryId)
            .HasConstraintName("FK_Maintenance_Asset_AssetCategory")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AssetStatus>()
            .WithMany()
            .HasForeignKey(x => x.AssetStatusId)
            .HasConstraintName("FK_Maintenance_Asset_AssetStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AssetCriticality>()
            .WithMany()
            .HasForeignKey(x => x.AssetCriticalityId)
            .HasConstraintName("FK_Maintenance_Asset_AssetCriticality")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
