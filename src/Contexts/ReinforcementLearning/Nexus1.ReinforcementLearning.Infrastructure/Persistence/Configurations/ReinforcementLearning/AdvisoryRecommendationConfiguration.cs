using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>
/// No audit columns beyond what's listed, verified against the real DDL.
/// Bigint identity key. RequestedAtUtc IS domain-modeled (a required
/// constructor param) despite also carrying a SQL DEFAULT, matching
/// QTable.SnapshotAtUtc/AdvisorySession.StartedAtUtc's own pattern.
///
/// Two distinct FKs to the SAME ActionDefinition table:
/// RecommendedActionDefinitionId (the raw policy pick, NOT NULL) and
/// ClampedActionDefinitionId (the action actually offered after the safety
/// clamp, nullable). EF requires two separate HasOne/WithMany
/// configurations with distinct constraint names since both point at the
/// same principal type — named FK_RL_AdvisoryRecommendation_Action and
/// FK_RL_AdvisoryRecommendation_ClampedAction, matching the atlas's own
/// naming (ADR-026).
/// </summary>
public sealed class AdvisoryRecommendationConfiguration : IEntityTypeConfiguration<AdvisoryRecommendation>
{
    public void Configure(EntityTypeBuilder<AdvisoryRecommendation> builder)
    {
        builder.ToTable("AdvisoryRecommendation", "ReinforcementLearning", t => t.HasCheckConstraint(
            "CK_ReinforcementLearning_AdvisoryRecommendation_ConfidenceScore",
            "[ConfidenceScore] IS NULL OR ([ConfidenceScore] >= 0 AND [ConfidenceScore] <= 1)"));
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_AdvisoryRecommendation");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AdvisoryRecommendationId(value))
            .HasColumnName("AdvisoryRecommendationId")
            .ValueGeneratedNever();

        builder.Property(x => x.AdvisorySessionId)
            .HasConversion(id => id.Value, value => new AdvisorySessionId(value))
            .HasColumnName("AdvisorySessionId")
            .IsRequired();

        builder.Property(x => x.RecommendationStatusId)
            .HasConversion(id => id.Value, value => new RecommendationStatusId(value))
            .HasColumnName("RecommendationStatusId")
            .IsRequired();

        builder.Property(x => x.StateDefinitionId)
            .HasConversion(id => id.Value, value => new StateDefinitionId(value))
            .HasColumnName("StateDefinitionId")
            .IsRequired();

        builder.Property(x => x.RecommendedActionDefinitionId)
            .HasConversion(id => id.Value, value => new ActionDefinitionId(value))
            .HasColumnName("RecommendedActionDefinitionId")
            .IsRequired();

        builder.Property(x => x.ClampedActionDefinitionId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new ActionDefinitionId(value.Value) : (ActionDefinitionId?)null)
            .HasColumnName("ClampedActionDefinitionId");

        builder.Property(x => x.ObservedPowerPercent).HasColumnType("decimal(12,6)");
        builder.Property(x => x.TargetPowerPercent).HasColumnType("decimal(12,6)");
        builder.Property(x => x.ConfidenceScore).HasColumnType("decimal(10,6)");
        builder.Property(x => x.WasClamped).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ClampReason).HasMaxLength(500);
        builder.Property(x => x.RequestedAtUtc).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.ExpiresAtUtc);

        builder.HasOne<AdvisorySession>()
            .WithMany()
            .HasForeignKey(x => x.AdvisorySessionId)
            .HasConstraintName("FK_ReinforcementLearning_AdvisoryRecommendation_AdvisorySession")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RecommendationStatus>()
            .WithMany()
            .HasForeignKey(x => x.RecommendationStatusId)
            .HasConstraintName("FK_ReinforcementLearning_AdvisoryRecommendation_RecommendationStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StateDefinition>()
            .WithMany()
            .HasForeignKey(x => x.StateDefinitionId)
            .HasConstraintName("FK_ReinforcementLearning_AdvisoryRecommendation_StateDefinition")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionDefinition>()
            .WithMany()
            .HasForeignKey(x => x.RecommendedActionDefinitionId)
            .HasConstraintName("FK_RL_AdvisoryRecommendation_Action")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ClampedActionDefinitionId)
            .HasConstraintName("FK_RL_AdvisoryRecommendation_ClampedAction")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
