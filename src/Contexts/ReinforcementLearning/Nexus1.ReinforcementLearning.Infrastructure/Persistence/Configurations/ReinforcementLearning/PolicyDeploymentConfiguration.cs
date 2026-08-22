using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>
/// Only RowVersion for audit — no Created/Modified columns at all,
/// verified against the real DDL (narrower than every other substantive
/// table in this sector). UnitId is a real FK to ReactorFleet.Unit via the
/// shadow-entity technique. DeployedByUserId is passport-only (ADR-026).
/// </summary>
public sealed class PolicyDeploymentConfiguration : IEntityTypeConfiguration<PolicyDeployment>
{
    public void Configure(EntityTypeBuilder<PolicyDeployment> builder)
    {
        builder.ToTable("PolicyDeployment", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_PolicyDeployment");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PolicyDeploymentId(value))
            .HasColumnName("PolicyDeploymentId")
            .ValueGeneratedNever();

        builder.Property(x => x.PolicyId)
            .HasConversion(id => id.Value, value => new PolicyId(value))
            .HasColumnName("PolicyId")
            .IsRequired();

        builder.Property(x => x.AdvisoryModeId)
            .HasConversion(id => id.Value, value => new AdvisoryModeId(value))
            .HasColumnName("AdvisoryModeId")
            .IsRequired();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();
        builder.Property(x => x.DeployedByUserId).HasColumnName("DeployedByUserId");
        builder.Property(x => x.DeployedAtUtc).IsRequired();
        builder.Property(x => x.RetiredAtUtc);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.DeploymentNote).HasMaxLength(1000);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne<Policy>()
            .WithMany()
            .HasForeignKey(x => x.PolicyId)
            .HasConstraintName("FK_ReinforcementLearning_PolicyDeployment_Policy")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AdvisoryMode>()
            .WithMany()
            .HasForeignKey(x => x.AdvisoryModeId)
            .HasConstraintName("FK_ReinforcementLearning_PolicyDeployment_AdvisoryMode")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_RL_PolicyDeployment_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
