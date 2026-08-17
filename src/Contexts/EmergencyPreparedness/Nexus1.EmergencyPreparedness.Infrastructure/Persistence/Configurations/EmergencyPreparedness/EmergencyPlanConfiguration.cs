using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// Full audit shape mapped as EF shadow properties only, same treatment as
/// RadiationMonitoring.RadiationZoneConfiguration (ADR-025). PlanStatusId
/// is a real internal FK, NOT NULL. SiteId/PlantId/OwnerUserId are
/// passport-only plain columns with no HasOne/FK declared at all —
/// Organization.Site/Plant live in OrganizationDb, Security.ApplicationUser
/// lives in SecurityDb, both different physical databases (ADR-025).
/// </summary>
public sealed class EmergencyPlanConfiguration : IEntityTypeConfiguration<EmergencyPlan>
{
    public void Configure(EntityTypeBuilder<EmergencyPlan> builder)
    {
        builder.ToTable("EmergencyPlan", "EmergencyPreparedness", t => t.HasCheckConstraint(
            "CK_EmergencyPreparedness_EmergencyPlan_EffectiveDateRange",
            "[EffectiveToUtc] IS NULL OR [EffectiveFromUtc] IS NULL OR [EffectiveToUtc] >= [EffectiveFromUtc]"));
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_EmergencyPlan");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EmergencyPlanId(value))
            .HasColumnName("EmergencyPlanId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.Property(x => x.PlanStatusId)
            .HasConversion(id => id.Value, value => new PlanStatusId(value))
            .HasColumnName("PlanStatusId")
            .IsRequired();

        builder.Property(x => x.SiteId).HasColumnName("SiteId").IsRequired();
        builder.Property(x => x.PlantId).HasColumnName("PlantId");
        builder.Property(x => x.OwnerUserId).HasColumnName("OwnerUserId").IsRequired();
        builder.Property(x => x.CurrentRevisionNumber).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.EffectiveFromUtc);
        builder.Property(x => x.EffectiveToUtc);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EmergencyPreparedness_EmergencyPlan_Code");

        builder.HasOne<PlanStatus>()
            .WithMany()
            .HasForeignKey(x => x.PlanStatusId)
            .HasConstraintName("FK_EmergencyPreparedness_EmergencyPlan_PlanStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
