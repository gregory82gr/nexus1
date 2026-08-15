using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.Infrastructure.Persistence.Configurations.ReactorFleet;

/// <summary>
/// Matches the atlas's append-only UnitPowerSnapshot table (no update columns,
/// ADR-003) — no FK to Unit is enforced at the database level beyond the
/// passport UnitId column, matching the atlas's own design.
/// </summary>
public sealed class UnitPowerSnapshotConfiguration : IEntityTypeConfiguration<UnitPowerSnapshot>
{
    public void Configure(EntityTypeBuilder<UnitPowerSnapshot> builder)
    {
        builder.ToTable("UnitPowerSnapshot", "ReactorFleet");
        builder.HasKey(x => x.Id).HasName("PK_ReactorFleet_UnitPowerSnapshot");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new UnitPowerSnapshotId(value))
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId)
            .HasConversion(id => id.Value, value => new UnitId(value))
            .HasColumnName("UnitId")
            .IsRequired();

        builder.Property(x => x.PowerPercent)
            .HasConversion(p => p.Value, value => new PowerPercent(value))
            .HasColumnName("PowerPercent")
            .HasColumnType("decimal(9,6)")
            .IsRequired();

        builder.Property(x => x.RecordedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.UnitId, x.RecordedAtUtc })
            .HasDatabaseName("IX_ReactorFleet_UnitPowerSnapshot_UnitId_RecordedAtUtc");

        builder.Ignore(x => x.DomainEvents);
    }
}
