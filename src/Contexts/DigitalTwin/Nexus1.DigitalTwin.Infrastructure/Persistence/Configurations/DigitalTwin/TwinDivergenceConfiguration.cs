using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>
/// The atlas DDL (C.6.4.5) gives this table only CreatedAtUtc + RowVersion
/// (no CreatedBy/ModifiedAtUtc/ModifiedBy/IsDeleted), verified directly
/// against the DDL. DetectedAtUtc is the real business timestamp (the
/// sector's own "conscience table" moment) and is domain-modeled;
/// CreatedAtUtc is pure row-insertion bookkeeping with a SQL DEFAULT and is
/// mapped as an unmapped-from-Domain shadow column only.
/// </summary>
public sealed class TwinDivergenceConfiguration : IEntityTypeConfiguration<TwinDivergence>
{
    public void Configure(EntityTypeBuilder<TwinDivergence> builder)
    {
        builder.ToTable("TwinDivergence", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinDivergence");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinDivergenceId(value))
            .HasColumnName("TwinDivergenceId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinSnapshotId)
            .HasConversion(id => id.Value, value => new TwinSnapshotId(value))
            .HasColumnName("TwinSnapshotId")
            .IsRequired();

        builder.Property(x => x.SignalId).HasColumnName("SignalId").IsRequired();

        builder.Property(x => x.TwinVariableId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new TwinVariableId(value.Value) : (TwinVariableId?)null)
            .HasColumnName("TwinVariableId");

        builder.Property(x => x.DivergenceSeverityId)
            .HasConversion(id => id.Value, value => new DivergenceSeverityId(value))
            .HasColumnName("DivergenceSeverityId")
            .IsRequired();

        builder.Property(x => x.DivergenceStatusId)
            .HasConversion(id => id.Value, value => new DivergenceStatusId(value))
            .HasColumnName("DivergenceStatusId")
            .IsRequired();

        builder.Property(x => x.DetectedAtUtc).HasColumnName("DetectedAtUtc").IsRequired();
        builder.Property(x => x.ModeledValue).HasColumnName("ModeledValue").IsRequired();
        builder.Property(x => x.MeasuredValue).HasColumnName("MeasuredValue").IsRequired();
        builder.Property(x => x.DeltaValue).HasColumnName("DeltaValue").IsRequired();
        builder.Property(x => x.DeltaPercent).HasColumnName("DeltaPercent").HasColumnType("decimal(10,4)");
        builder.Property(x => x.ThresholdAbs).HasColumnName("ThresholdAbs");
        builder.Property(x => x.ThresholdPercent).HasColumnName("ThresholdPercent").HasColumnType("decimal(10,4)");
        builder.Property(x => x.Explanation).HasColumnName("Explanation").HasMaxLength(1000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne<TwinSnapshot>()
            .WithMany()
            .HasForeignKey(x => x.TwinSnapshotId)
            .HasConstraintName("FK_DigitalTwin_TwinDivergence_TwinSnapshot")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<InstrumentationSignalReference>()
            .WithMany()
            .HasForeignKey(x => x.SignalId)
            .HasConstraintName("FK_DigitalTwin_TwinDivergence_Signal")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TwinVariable>()
            .WithMany()
            .HasForeignKey(x => x.TwinVariableId)
            .HasConstraintName("FK_DigitalTwin_TwinDivergence_TwinVariable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DivergenceSeverity>()
            .WithMany()
            .HasForeignKey(x => x.DivergenceSeverityId)
            .HasConstraintName("FK_DigitalTwin_TwinDivergence_DivergenceSeverity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DivergenceStatus>()
            .WithMany()
            .HasForeignKey(x => x.DivergenceStatusId)
            .HasConstraintName("FK_DigitalTwin_TwinDivergence_DivergenceStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
