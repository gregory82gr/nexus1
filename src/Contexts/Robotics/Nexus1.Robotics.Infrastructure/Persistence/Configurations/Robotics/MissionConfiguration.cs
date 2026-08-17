using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>
/// Full audit shape mapped as EF shadow properties only, same treatment as
/// RobotModelConfiguration (ADR-023). UnitId carries a real FK to
/// ReactorFleet.Unit via ReactorFleetUnitReference. RequestedByUserId/
/// ApprovedByUserId stay passport-only — Security.ApplicationUser lives in
/// SecurityDb, a different physical database.
/// </summary>
public sealed class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.ToTable("Mission", "Robotics", t =>
        {
            t.HasCheckConstraint("CK_Robotics_Mission_PlannedDateRange", "[PlannedEndUtc] IS NULL OR [PlannedStartUtc] IS NULL OR [PlannedEndUtc] >= [PlannedStartUtc]");
            t.HasCheckConstraint("CK_Robotics_Mission_ActualDateRange", "[ActualEndUtc] IS NULL OR [ActualStartUtc] IS NULL OR [ActualEndUtc] >= [ActualStartUtc]");
        });
        builder.HasKey(x => x.Id).HasName("PK_Robotics_Mission");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MissionId(value))
            .HasColumnName("MissionId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();

        builder.Property(x => x.MissionTypeId)
            .HasConversion(id => id.Value, value => new MissionTypeId(value))
            .HasColumnName("MissionTypeId")
            .IsRequired();

        builder.Property(x => x.MissionStatusId)
            .HasConversion(id => id.Value, value => new MissionStatusId(value))
            .HasColumnName("MissionStatusId")
            .IsRequired();

        builder.Property(x => x.MissionPriorityId)
            .HasConversion(id => id.Value, value => new MissionPriorityId(value))
            .HasColumnName("MissionPriorityId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Objective).HasMaxLength(2000);
        builder.Property(x => x.RequestedAtUtc).IsRequired();
        builder.Property(x => x.PlannedStartUtc);
        builder.Property(x => x.PlannedEndUtc);
        builder.Property(x => x.ActualStartUtc);
        builder.Property(x => x.ActualEndUtc);
        builder.Property(x => x.RequestedByUserId).HasColumnName("RequestedByUserId");
        builder.Property(x => x.ApprovedByUserId).HasColumnName("ApprovedByUserId");

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Robotics_Mission_Code");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_Robotics_Mission_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MissionType>()
            .WithMany()
            .HasForeignKey(x => x.MissionTypeId)
            .HasConstraintName("FK_Robotics_Mission_MissionType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MissionStatus>()
            .WithMany()
            .HasForeignKey(x => x.MissionStatusId)
            .HasConstraintName("FK_Robotics_Mission_MissionStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MissionPriority>()
            .WithMany()
            .HasForeignKey(x => x.MissionPriorityId)
            .HasConstraintName("FK_Robotics_Mission_MissionPriority")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
