using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>The atlas DDL (C.6.4.5) gives this table only CreatedAtUtc — see TwinSnapshotConfiguration's XML doc for the same pattern's rationale.</summary>
public sealed class TwinSnapshotValueConfiguration : IEntityTypeConfiguration<TwinSnapshotValue>
{
    public void Configure(EntityTypeBuilder<TwinSnapshotValue> builder)
    {
        builder.ToTable("TwinSnapshotValue", "DigitalTwin", tb => tb.HasCheckConstraint(
            "CK_DigitalTwin_TwinSnapshotValue_OneValue",
            "[NumericValue] IS NOT NULL OR [TextValue] IS NOT NULL OR [JsonValue] IS NOT NULL"));
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinSnapshotValue");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinSnapshotValueId(value))
            .HasColumnName("TwinSnapshotValueId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinSnapshotId)
            .HasConversion(id => id.Value, value => new TwinSnapshotId(value))
            .HasColumnName("TwinSnapshotId")
            .IsRequired();

        builder.Property(x => x.TwinVariableId)
            .HasConversion(id => id.Value, value => new TwinVariableId(value))
            .HasColumnName("TwinVariableId")
            .IsRequired();

        builder.Property(x => x.NumericValue).HasColumnName("NumericValue");
        builder.Property(x => x.TextValue).HasColumnName("TextValue").HasMaxLength(1000);
        builder.Property(x => x.JsonValue).HasColumnName("JsonValue");

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => new { x.TwinSnapshotId, x.TwinVariableId }).IsUnique()
            .HasDatabaseName("UQ_DigitalTwin_TwinSnapshotValue_Snapshot_Variable");

        builder.HasOne<TwinSnapshot>()
            .WithMany()
            .HasForeignKey(x => x.TwinSnapshotId)
            .HasConstraintName("FK_DigitalTwin_TwinSnapshotValue_TwinSnapshot")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TwinVariable>()
            .WithMany()
            .HasForeignKey(x => x.TwinVariableId)
            .HasConstraintName("FK_DigitalTwin_TwinSnapshotValue_TwinVariable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
