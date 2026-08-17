using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// Full audit shape mapped as EF shadow properties only. EmergencyPlanId
/// and PlanStatusId are real internal FKs, NOT NULL. RevisionNumber is
/// unique together with EmergencyPlanId. PreparedByUserId/ApprovedByUserId
/// are passport-only plain columns — Security.ApplicationUser lives in
/// SecurityDb (ADR-025).
/// </summary>
public sealed class EmergencyPlanRevisionConfiguration : IEntityTypeConfiguration<EmergencyPlanRevision>
{
    public void Configure(EntityTypeBuilder<EmergencyPlanRevision> builder)
    {
        builder.ToTable("EmergencyPlanRevision", "EmergencyPreparedness");
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_EmergencyPlanRevision");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EmergencyPlanRevisionId(value))
            .HasColumnName("EmergencyPlanRevisionId")
            .ValueGeneratedNever();

        builder.Property(x => x.EmergencyPlanId)
            .HasConversion(id => id.Value, value => new EmergencyPlanId(value))
            .HasColumnName("EmergencyPlanId")
            .IsRequired();

        builder.Property(x => x.RevisionNumber).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();

        builder.Property(x => x.PlanStatusId)
            .HasConversion(id => id.Value, value => new PlanStatusId(value))
            .HasColumnName("PlanStatusId")
            .IsRequired();

        builder.Property(x => x.PreparedByUserId).HasColumnName("PreparedByUserId").IsRequired();
        builder.Property(x => x.PreparedAtUtc).IsRequired();
        builder.Property(x => x.ApprovedByUserId).HasColumnName("ApprovedByUserId");
        builder.Property(x => x.ApprovedAtUtc);
        builder.Property(x => x.DocumentUri).HasMaxLength(1000);
        builder.Property(x => x.ChangeSummary).HasMaxLength(2000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.EmergencyPlanId, x.RevisionNumber }).IsUnique()
            .HasDatabaseName("UQ_EmergencyPreparedness_EmergencyPlanRevision_Plan_RevisionNumber");

        builder.HasOne<EmergencyPlan>()
            .WithMany()
            .HasForeignKey(x => x.EmergencyPlanId)
            .HasConstraintName("FK_EmergencyPreparedness_EmergencyPlanRevision_EmergencyPlan")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PlanStatus>()
            .WithMany()
            .HasForeignKey(x => x.PlanStatusId)
            .HasConstraintName("FK_EmergencyPreparedness_EmergencyPlanRevision_PlanStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
