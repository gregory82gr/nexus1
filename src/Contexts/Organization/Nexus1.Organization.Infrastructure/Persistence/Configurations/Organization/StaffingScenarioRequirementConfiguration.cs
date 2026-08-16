using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>No audit columns — the atlas DDL genuinely gives this table none (ADR-017).</summary>
public sealed class StaffingScenarioRequirementConfiguration : IEntityTypeConfiguration<StaffingScenarioRequirement>
{
    public void Configure(EntityTypeBuilder<StaffingScenarioRequirement> builder)
    {
        builder.ToTable("StaffingScenarioRequirement", "Organization", t => t.HasCheckConstraint(
            "CK_Organization_StaffingScenarioRequirement_RequiredCount", "[RequiredCount] >= 0"));
        builder.HasKey(x => x.Id).HasName("PK_Organization_StaffingScenarioRequirement");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new StaffingScenarioRequirementId(value))
            .HasColumnName("StaffingScenarioRequirementId")
            .ValueGeneratedNever();

        builder.Property(x => x.StaffingScenarioId)
            .HasConversion(id => id.Value, value => new StaffingScenarioId(value))
            .HasColumnName("StaffingScenarioId")
            .IsRequired();

        builder.Property(x => x.PositionId)
            .HasConversion(id => id.Value, value => new PositionId(value))
            .HasColumnName("PositionId")
            .IsRequired();

        builder.Property(x => x.RequiredQualificationId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new QualificationId(value.Value) : (QualificationId?)null)
            .HasColumnName("RequiredQualificationId");

        builder.Property(x => x.RequiredCount).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne<StaffingScenario>().WithMany().HasForeignKey(x => x.StaffingScenarioId)
            .HasConstraintName("FK_Organization_StaffingScenarioRequirement_StaffingScenario")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>().WithMany().HasForeignKey(x => x.PositionId)
            .HasConstraintName("FK_Organization_StaffingScenarioRequirement_Position")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Qualification>().WithMany().HasForeignKey(x => x.RequiredQualificationId)
            .HasConstraintName("FK_Organization_StaffingScenarioRequirement_RequiredQualification")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
