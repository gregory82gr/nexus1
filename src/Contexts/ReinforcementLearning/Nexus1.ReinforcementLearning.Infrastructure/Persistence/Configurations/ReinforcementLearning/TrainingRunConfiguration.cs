using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>
/// Audit shape: CreatedAtUtc/CreatedBy/ModifiedAtUtc/ModifiedBy/RowVersion
/// only — NO IsDeleted (verified against the real DDL, narrower than
/// EnvironmentModel's full six-column shape). Not modeled in Domain — EF
/// shadow properties only (ADR-026). Seven internal FKs, all NOT NULL.
/// </summary>
public sealed class TrainingRunConfiguration : IEntityTypeConfiguration<TrainingRun>
{
    public void Configure(EntityTypeBuilder<TrainingRun> builder)
    {
        builder.ToTable("TrainingRun", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_TrainingRun");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TrainingRunId(value))
            .HasColumnName("TrainingRunId")
            .ValueGeneratedNever();

        builder.Property(x => x.ExperimentId)
            .HasConversion(id => id.Value, value => new ExperimentId(value))
            .HasColumnName("ExperimentId")
            .IsRequired();

        builder.Property(x => x.EnvironmentModelId)
            .HasConversion(id => id.Value, value => new EnvironmentModelId(value))
            .HasColumnName("EnvironmentModelId")
            .IsRequired();

        builder.Property(x => x.StateSpaceId)
            .HasConversion(id => id.Value, value => new StateSpaceId(value))
            .HasColumnName("StateSpaceId")
            .IsRequired();

        builder.Property(x => x.ActionSpaceId)
            .HasConversion(id => id.Value, value => new ActionSpaceId(value))
            .HasColumnName("ActionSpaceId")
            .IsRequired();

        builder.Property(x => x.RewardFunctionId)
            .HasConversion(id => id.Value, value => new RewardFunctionId(value))
            .HasColumnName("RewardFunctionId")
            .IsRequired();

        builder.Property(x => x.HyperparameterSetId)
            .HasConversion(id => id.Value, value => new HyperparameterSetId(value))
            .HasColumnName("HyperparameterSetId")
            .IsRequired();

        builder.Property(x => x.LearningAlgorithmId)
            .HasConversion(id => id.Value, value => new LearningAlgorithmId(value))
            .HasColumnName("LearningAlgorithmId")
            .IsRequired();

        builder.Property(x => x.TrainingRunStatusId)
            .HasConversion(id => id.Value, value => new TrainingRunStatusId(value))
            .HasColumnName("TrainingRunStatusId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.StartedAtUtc);
        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.EpisodeCountCompleted).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TotalReward).HasColumnType("decimal(20,6)");
        builder.Property(x => x.AverageReward).HasColumnType("decimal(20,6)");
        builder.Property(x => x.RunSeed);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_TrainingRun_Code");

        builder.HasOne<Experiment>()
            .WithMany()
            .HasForeignKey(x => x.ExperimentId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_Experiment")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EnvironmentModel>()
            .WithMany()
            .HasForeignKey(x => x.EnvironmentModelId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_EnvironmentModel")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StateSpace>()
            .WithMany()
            .HasForeignKey(x => x.StateSpaceId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_StateSpace")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionSpace>()
            .WithMany()
            .HasForeignKey(x => x.ActionSpaceId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_ActionSpace")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RewardFunction>()
            .WithMany()
            .HasForeignKey(x => x.RewardFunctionId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_RewardFunction")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HyperparameterSet>()
            .WithMany()
            .HasForeignKey(x => x.HyperparameterSetId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_HyperparameterSet")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LearningAlgorithm>()
            .WithMany()
            .HasForeignKey(x => x.LearningAlgorithmId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_LearningAlgorithm")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TrainingRunStatus>()
            .WithMany()
            .HasForeignKey(x => x.TrainingRunStatusId)
            .HasConstraintName("FK_ReinforcementLearning_TrainingRun_TrainingRunStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
