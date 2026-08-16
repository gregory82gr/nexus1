using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>
/// GapCount is mapped as a real SQL Server PERSISTED computed column,
/// matching the atlas DDL exactly (<c>CASE WHEN RequiredCount &gt;
/// AvailableCount THEN RequiredCount - AvailableCount ELSE 0 END</c>) — the
/// domain factory (StaffingScenarioGap.Create) computes the identical value
/// so the database and the domain agree by construction (ADR-017). No
/// audit columns — the atlas DDL genuinely gives this table none.
/// </summary>
public sealed class StaffingScenarioGapConfiguration : IEntityTypeConfiguration<StaffingScenarioGap>
{
    public void Configure(EntityTypeBuilder<StaffingScenarioGap> builder)
    {
        builder.ToTable("StaffingScenarioGap", "Organization", t => t.HasCheckConstraint(
            "CK_Organization_StaffingScenarioGap_Counts", "[RequiredCount] >= 0 AND [AvailableCount] >= 0"));
        builder.HasKey(x => x.Id).HasName("PK_Organization_StaffingScenarioGap");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new StaffingScenarioGapId(value))
            .HasColumnName("StaffingScenarioGapId")
            .ValueGeneratedNever();

        builder.Property(x => x.StaffingScenarioResultId)
            .HasConversion(id => id.Value, value => new StaffingScenarioResultId(value))
            .HasColumnName("StaffingScenarioResultId")
            .IsRequired();

        builder.Property(x => x.PositionId)
            .HasConversion(id => id.Value, value => new PositionId(value))
            .HasColumnName("PositionId")
            .IsRequired();

        builder.Property(x => x.RequiredCount).IsRequired();
        builder.Property(x => x.AvailableCount).IsRequired();

        builder.Property(x => x.GapCount)
            .HasComputedColumnSql(
                "(CASE WHEN [RequiredCount] > [AvailableCount] THEN [RequiredCount] - [AvailableCount] ELSE 0 END)",
                stored: true)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne<StaffingScenarioResult>().WithMany().HasForeignKey(x => x.StaffingScenarioResultId)
            .HasConstraintName("FK_Organization_StaffingScenarioGap_StaffingScenarioResult")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>().WithMany().HasForeignKey(x => x.PositionId)
            .HasConstraintName("FK_Organization_StaffingScenarioGap_Position")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
