using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>No audit columns — append-only fact table (ADR-023).</summary>
public sealed class RobotHealthSnapshotConfiguration : IEntityTypeConfiguration<RobotHealthSnapshot>
{
    public void Configure(EntityTypeBuilder<RobotHealthSnapshot> builder)
    {
        builder.ToTable("RobotHealthSnapshot", "Robotics", t =>
        {
            t.HasCheckConstraint("CK_Robotics_RobotHealthSnapshot_BatteryPercent", "[BatteryPercent] IS NULL OR ([BatteryPercent] BETWEEN 0 AND 100)");
            t.HasCheckConstraint("CK_Robotics_RobotHealthSnapshot_CpuLoadPercent", "[CpuLoadPercent] IS NULL OR ([CpuLoadPercent] BETWEEN 0 AND 100)");
        });
        builder.HasKey(x => x.Id).HasName("PK_Robotics_RobotHealthSnapshot");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RobotHealthSnapshotId(value))
            .HasColumnName("RobotHealthSnapshotId")
            .ValueGeneratedNever();

        builder.Property(x => x.RobotId)
            .HasConversion(id => id.Value, value => new RobotId(value))
            .HasColumnName("RobotId")
            .IsRequired();

        builder.Property(x => x.BatteryStatusId)
            .HasConversion(id => id.Value, value => new BatteryStatusId(value))
            .HasColumnName("BatteryStatusId")
            .IsRequired();

        builder.Property(x => x.CommunicationStatusId)
            .HasConversion(id => id.Value, value => new CommunicationStatusId(value))
            .HasColumnName("CommunicationStatusId")
            .IsRequired();

        builder.Property(x => x.SnapshotAtUtc).IsRequired();
        builder.Property(x => x.BatteryPercent).HasColumnType("decimal(5,2)");
        builder.Property(x => x.EstimatedRuntimeMin);
        builder.Property(x => x.CpuLoadPercent).HasColumnType("decimal(5,2)");
        builder.Property(x => x.FaultCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.Summary).HasMaxLength(1000);

        builder.HasOne<Robot>()
            .WithMany()
            .HasForeignKey(x => x.RobotId)
            .HasConstraintName("FK_Robotics_RobotHealthSnapshot_Robot")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BatteryStatus>()
            .WithMany()
            .HasForeignKey(x => x.BatteryStatusId)
            .HasConstraintName("FK_Robotics_RobotHealthSnapshot_BatteryStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CommunicationStatus>()
            .WithMany()
            .HasForeignKey(x => x.CommunicationStatusId)
            .HasConstraintName("FK_Robotics_RobotHealthSnapshot_CommunicationStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
