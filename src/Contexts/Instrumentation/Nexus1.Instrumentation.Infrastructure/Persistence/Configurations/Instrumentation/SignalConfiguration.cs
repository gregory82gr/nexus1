using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

/// <summary>
/// EquipmentId/PlantSystemId are mapped as plain nullable int columns with
/// NO enforced FOREIGN KEY: ReactorFleet.Equipment and ReactorFleet.PlantSystem
/// do not exist in this codebase (ReactorFleet's Phase 1 slice, ADR-003, only
/// ever built ReactorFleet.Unit). This is a discrepancy against ADR-019's
/// text, which assumed those two tables were available to FK against —
/// flagged in the session's final report rather than guessed silently.
/// SensorChannelId is omitted entirely (Instrument/Sensor/SensorChannel are
/// out of scope, ADR-019, and the atlas's own DDL makes the column nullable).
/// </summary>
public sealed class SignalConfiguration : IEntityTypeConfiguration<Signal>
{
    public void Configure(EntityTypeBuilder<Signal> builder)
    {
        builder.ToTable("Signal", "Instrumentation", tb =>
        {
            tb.HasCheckConstraint("CK_Instrumentation_Signal_NormalRange", "[NormalMin] IS NULL OR [NormalMax] IS NULL OR [NormalMax] > [NormalMin]");
            tb.HasCheckConstraint("CK_Instrumentation_Signal_ScanRate", "[ScanRateHz] IS NULL OR [ScanRateHz] > 0");
        });
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_Signal");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SignalId(value))
            .HasColumnName("SignalId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();
        builder.Property(x => x.EquipmentId).HasColumnName("EquipmentId");
        builder.Property(x => x.PlantSystemId).HasColumnName("PlantSystemId");

        builder.Property(x => x.SignalTypeId)
            .HasConversion(id => id.Value, value => new SignalTypeId(value))
            .HasColumnName("SignalTypeId")
            .IsRequired();

        builder.Property(x => x.SignalCategoryId)
            .HasConversion(id => id.Value, value => new SignalCategoryId(value))
            .HasColumnName("SignalCategoryId")
            .IsRequired();

        builder.Property(x => x.SignalRoleId)
            .HasConversion(id => id.Value, value => new SignalRoleId(value))
            .HasColumnName("SignalRoleId")
            .IsRequired();

        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId").IsRequired();

        builder.Property(x => x.SamplingModeId)
            .HasConversion(id => id.Value, value => new SamplingModeId(value))
            .HasColumnName("SamplingModeId")
            .IsRequired();

        builder.Property(x => x.HistorianRetentionClassId)
            .HasConversion(id => id.Value, value => new HistorianRetentionClassId(value))
            .HasColumnName("HistorianRetentionClassId")
            .IsRequired();

        builder.Property(x => x.Tag).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ScanRateHz).HasColumnType("decimal(12,6)");
        builder.Property(x => x.PrecisionDigits);
        builder.Property(x => x.NormalMin).HasColumnType("decimal(18,6)");
        builder.Property(x => x.NormalMax).HasColumnType("decimal(18,6)");
        builder.Property(x => x.IsSafetyRelated).IsRequired();
        builder.Property(x => x.IsHistorized).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Tag).IsUnique().HasDatabaseName("UQ_Instrumentation_Signal_Tag");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_Instrumentation_Signal_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_Instrumentation_Signal_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SignalType>()
            .WithMany()
            .HasForeignKey(x => x.SignalTypeId)
            .HasConstraintName("FK_Instrumentation_Signal_SignalType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SignalCategory>()
            .WithMany()
            .HasForeignKey(x => x.SignalCategoryId)
            .HasConstraintName("FK_Instrumentation_Signal_SignalCategory")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SignalRole>()
            .WithMany()
            .HasForeignKey(x => x.SignalRoleId)
            .HasConstraintName("FK_Instrumentation_Signal_SignalRole")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SamplingMode>()
            .WithMany()
            .HasForeignKey(x => x.SamplingModeId)
            .HasConstraintName("FK_Instrumentation_Signal_SamplingMode")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HistorianRetentionClass>()
            .WithMany()
            .HasForeignKey(x => x.HistorianRetentionClassId)
            .HasConstraintName("FK_Instrumentation_Signal_HistorianRetentionClass")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
