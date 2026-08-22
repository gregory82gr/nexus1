using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>
/// No audit columns — the atlas DDL genuinely gives this table none
/// (ADR-017). EvaluatedByUserId is a plain passport column with no FK
/// constraint for the same cross-database reason as StaffingScenario's
/// CreatedByUserId.
/// </summary>
public sealed class StaffingScenarioResultConfiguration : IEntityTypeConfiguration<StaffingScenarioResult>
{
    public void Configure(EntityTypeBuilder<StaffingScenarioResult> builder)
    {
        builder.ToTable("StaffingScenarioResult", "Organization", t => t.HasCheckConstraint(
            "CK_Organization_StaffingScenarioResult_OverallStatus",
            "[OverallStatus] IN ('Pass','Warning','Fail','NotEvaluated')"));
        builder.HasKey(x => x.Id).HasName("PK_Organization_StaffingScenarioResult");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new StaffingScenarioResultId(value))
            .HasColumnName("StaffingScenarioResultId")
            .ValueGeneratedNever();

        builder.Property(x => x.StaffingScenarioId)
            .HasConversion(id => id.Value, value => new StaffingScenarioId(value))
            .HasColumnName("StaffingScenarioId")
            .IsRequired();

        builder.Property(x => x.EvaluatedAtUtc).IsRequired();
        builder.Property(x => x.EvaluatedByUserId).HasColumnName("EvaluatedByUserId");
        builder.Property(x => x.OverallStatus).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000);

        builder.HasOne<StaffingScenario>().WithMany().HasForeignKey(x => x.StaffingScenarioId)
            .HasConstraintName("FK_Organization_StaffingScenarioResult_StaffingScenario")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
