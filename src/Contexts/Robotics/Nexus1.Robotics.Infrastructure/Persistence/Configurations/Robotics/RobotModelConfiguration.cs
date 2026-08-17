using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>
/// Full audit shape (CreatedAtUtc/CreatedBy/ModifiedAtUtc/ModifiedBy/
/// IsDeleted/RowVersion) mapped as EF shadow properties only, same
/// treatment as EventManagement.OperationalEventConfiguration (ADR-023).
/// </summary>
public sealed class RobotModelConfiguration : IEntityTypeConfiguration<RobotModel>
{
    public void Configure(EntityTypeBuilder<RobotModel> builder)
    {
        builder.ToTable("RobotModel", "Robotics", t =>
        {
            t.HasCheckConstraint("CK_Robotics_RobotModel_MaxPayloadKg", "[MaxPayloadKg] IS NULL OR [MaxPayloadKg] >= 0");
            t.HasCheckConstraint("CK_Robotics_RobotModel_MaxSpeedMps", "[MaxSpeedMps] IS NULL OR [MaxSpeedMps] >= 0");
        });
        builder.HasKey(x => x.Id).HasName("PK_Robotics_RobotModel");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RobotModelId(value))
            .HasColumnName("RobotModelId")
            .ValueGeneratedNever();

        builder.Property(x => x.RobotTypeId)
            .HasConversion(id => id.Value, value => new RobotTypeId(value))
            .HasColumnName("RobotTypeId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Manufacturer).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ModelName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.MaxPayloadKg).HasColumnType("decimal(10,3)");
        builder.Property(x => x.MaxSpeedMps).HasColumnType("decimal(10,3)");
        builder.Property(x => x.BatteryCapacityWh).HasColumnType("decimal(12,2)");
        builder.Property(x => x.NominalRuntimeMin);
        builder.Property(x => x.IsAutonomousCapable).IsRequired().HasDefaultValue(false);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Robotics_RobotModel_Code");

        builder.HasOne<RobotType>()
            .WithMany()
            .HasForeignKey(x => x.RobotTypeId)
            .HasConstraintName("FK_Robotics_RobotModel_RobotType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
