using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>
/// The atlas DDL (C.6.4.5) gives this table only CreatedAtUtc (no
/// CreatedBy/ModifiedAtUtc/ModifiedBy/IsDeleted/RowVersion) — leaner than
/// TwinModel/TwinModelVersion/TwinVariable/SignalBinding, verified directly
/// against the DDL. CreatedAtUtc itself is left unmapped (pure bookkeeping
/// with a SQL DEFAULT — SnapshotAtUtc is the real business timestamp and is
/// mapped as a shadow column so the physical schema still matches the atlas
/// exactly) via HasDefaultValueSql, no shadow-property NOT-NULL problem
/// exists here since it has a real DEFAULT constraint in the atlas.
/// </summary>
public sealed class TwinSnapshotConfiguration : IEntityTypeConfiguration<TwinSnapshot>
{
    public void Configure(EntityTypeBuilder<TwinSnapshot> builder)
    {
        builder.ToTable("TwinSnapshot", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinSnapshot");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinSnapshotId(value))
            .HasColumnName("TwinSnapshotId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinRuntimeSessionId)
            .HasConversion(id => id.Value, value => new TwinRuntimeSessionId(value))
            .HasColumnName("TwinRuntimeSessionId")
            .IsRequired();

        builder.Property(x => x.SnapshotReasonId)
            .HasConversion(id => id.Value, value => new SnapshotReasonId(value))
            .HasColumnName("SnapshotReasonId")
            .IsRequired();

        builder.Property(x => x.SnapshotAtUtc).HasColumnName("SnapshotAtUtc").IsRequired();
        builder.Property(x => x.TimeStepIndex).HasColumnName("TimeStepIndex");
        builder.Property(x => x.StateVectorHash).HasColumnName("StateVectorHash").HasColumnType("varbinary(32)");
        builder.Property(x => x.SummaryJson).HasColumnName("SummaryJson");

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne<TwinRuntimeSession>()
            .WithMany()
            .HasForeignKey(x => x.TwinRuntimeSessionId)
            .HasConstraintName("FK_DigitalTwin_TwinSnapshot_TwinRuntimeSession")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SnapshotReason>()
            .WithMany()
            .HasForeignKey(x => x.SnapshotReasonId)
            .HasConstraintName("FK_DigitalTwin_TwinSnapshot_SnapshotReason")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
