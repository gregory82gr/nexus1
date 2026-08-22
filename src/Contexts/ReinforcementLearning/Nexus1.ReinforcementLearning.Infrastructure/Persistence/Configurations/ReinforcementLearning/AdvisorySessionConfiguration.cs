using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>
/// No audit columns beyond what's listed — no RowVersion for this table,
/// verified against the real DDL. Bigint identity key. UnitId is a real FK
/// to ReactorFleet.Unit via the shadow-entity technique. StartedByUserId is
/// passport-only (ADR-026). StartedAtUtc IS domain-modeled (a required
/// constructor param) despite also carrying a SQL DEFAULT, matching
/// QTable.SnapshotAtUtc's own pattern.
/// </summary>
public sealed class AdvisorySessionConfiguration : IEntityTypeConfiguration<AdvisorySession>
{
    public void Configure(EntityTypeBuilder<AdvisorySession> builder)
    {
        builder.ToTable("AdvisorySession", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_AdvisorySession");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AdvisorySessionId(value))
            .HasColumnName("AdvisorySessionId")
            .ValueGeneratedNever();

        builder.Property(x => x.PolicyDeploymentId)
            .HasConversion(id => id.Value, value => new PolicyDeploymentId(value))
            .HasColumnName("PolicyDeploymentId")
            .IsRequired();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();
        builder.Property(x => x.StartedByUserId).HasColumnName("StartedByUserId");
        builder.Property(x => x.StartedAtUtc).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.EndedAtUtc);
        builder.Property(x => x.SessionNote).HasMaxLength(1000);

        builder.HasOne<PolicyDeployment>()
            .WithMany()
            .HasForeignKey(x => x.PolicyDeploymentId)
            .HasConstraintName("FK_ReinforcementLearning_AdvisorySession_PolicyDeployment")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_RL_AdvisorySession_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
