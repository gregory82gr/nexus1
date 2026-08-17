using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>
/// Full audit shape mapped as EF shadow properties only, same treatment as
/// RobotModelConfiguration (ADR-023). HomeUnitId carries a real FK to
/// ReactorFleet.Unit via ReactorFleetUnitReference. HomeDockingStationId is
/// deliberately not mapped — its target table does not exist in this pass
/// (ADR-023).
/// </summary>
public sealed class RobotConfiguration : IEntityTypeConfiguration<Robot>
{
    public void Configure(EntityTypeBuilder<Robot> builder)
    {
        builder.ToTable("Robot", "Robotics");
        builder.HasKey(x => x.Id).HasName("PK_Robotics_Robot");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RobotId(value))
            .HasColumnName("RobotId")
            .ValueGeneratedNever();

        builder.Property(x => x.RobotModelId)
            .HasConversion(id => id.Value, value => new RobotModelId(value))
            .HasColumnName("RobotModelId")
            .IsRequired();

        builder.Property(x => x.RobotStatusId)
            .HasConversion(id => id.Value, value => new RobotStatusId(value))
            .HasColumnName("RobotStatusId")
            .IsRequired();

        builder.Property(x => x.HomeUnitId).HasColumnName("HomeUnitId");
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SerialNumber).HasMaxLength(120);
        builder.Property(x => x.CommissionedAtUtc);
        builder.Property(x => x.RetiredAtUtc);
        builder.Property(x => x.LastSeenAtUtc);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Robotics_Robot_Code");

        builder.HasOne<RobotModel>()
            .WithMany()
            .HasForeignKey(x => x.RobotModelId)
            .HasConstraintName("FK_Robotics_Robot_RobotModel")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RobotStatus>()
            .WithMany()
            .HasForeignKey(x => x.RobotStatusId)
            .HasConstraintName("FK_Robotics_Robot_RobotStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.HomeUnitId)
            .HasConstraintName("FK_Robotics_Robot_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
