using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// Full audit shape mapped as EF shadow properties only. ResourceTypeId/
/// ResourceStatusId are real internal FKs, NOT NULL. EngineeringUnitId
/// carries a real FK to CorePlatform.EngineeringUnit via
/// CorePlatformEngineeringUnitReference, named
/// FK_EmergencyResource_EngineeringUnit verbatim per ADR-025's own
/// evidence-required section. SiteId/PlantId/OwnerTeamId are passport-only —
/// Organization.Site/Plant/Team live in OrganizationDb (ADR-025).
/// </summary>
public sealed class EmergencyResourceConfiguration : IEntityTypeConfiguration<EmergencyResource>
{
    public void Configure(EntityTypeBuilder<EmergencyResource> builder)
    {
        builder.ToTable("EmergencyResource", "EmergencyPreparedness", t => t.HasCheckConstraint(
            "CK_EmergencyPreparedness_EmergencyResource_QuantityOnHand", "[QuantityOnHand] IS NULL OR [QuantityOnHand] >= 0"));
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_EmergencyResource");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EmergencyResourceId(value))
            .HasColumnName("EmergencyResourceId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.Property(x => x.ResourceTypeId)
            .HasConversion(id => id.Value, value => new ResourceTypeId(value))
            .HasColumnName("ResourceTypeId")
            .IsRequired();

        builder.Property(x => x.ResourceStatusId)
            .HasConversion(id => id.Value, value => new ResourceStatusId(value))
            .HasColumnName("ResourceStatusId")
            .IsRequired();

        builder.Property(x => x.SiteId).HasColumnName("SiteId").IsRequired();
        builder.Property(x => x.PlantId).HasColumnName("PlantId");
        builder.Property(x => x.OwnerTeamId).HasColumnName("OwnerTeamId");
        builder.Property(x => x.QuantityOnHand).HasColumnType("decimal(18,3)");
        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId");
        builder.Property(x => x.LocationText).HasMaxLength(300);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EmergencyPreparedness_EmergencyResource_Code");

        builder.HasOne<ResourceType>()
            .WithMany()
            .HasForeignKey(x => x.ResourceTypeId)
            .HasConstraintName("FK_EmergencyPreparedness_EmergencyResource_ResourceType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ResourceStatus>()
            .WithMany()
            .HasForeignKey(x => x.ResourceStatusId)
            .HasConstraintName("FK_EmergencyPreparedness_EmergencyResource_ResourceStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_EmergencyResource_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
