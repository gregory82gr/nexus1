using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>Full audit shape mapped as EF shadow properties only (ADR-026). No internal FKs.</summary>
public sealed class HyperparameterSetConfiguration : IEntityTypeConfiguration<HyperparameterSet>
{
    public void Configure(EntityTypeBuilder<HyperparameterSet> builder)
    {
        builder.ToTable("HyperparameterSet", "ReinforcementLearning", t =>
        {
            t.HasCheckConstraint("CK_ReinforcementLearning_HyperparameterSet_LearningRateAlpha", "[LearningRateAlpha] > 0 AND [LearningRateAlpha] <= 1");
            t.HasCheckConstraint("CK_ReinforcementLearning_HyperparameterSet_DiscountGamma", "[DiscountGamma] >= 0 AND [DiscountGamma] <= 1");
            t.HasCheckConstraint("CK_ReinforcementLearning_HyperparameterSet_EpsilonStart", "[EpsilonStart] >= 0 AND [EpsilonStart] <= 1");
            t.HasCheckConstraint("CK_ReinforcementLearning_HyperparameterSet_EpsilonEnd", "[EpsilonEnd] >= 0 AND [EpsilonEnd] <= 1");
            t.HasCheckConstraint("CK_ReinforcementLearning_HyperparameterSet_EpisodeCount", "[EpisodeCount] > 0");
            t.HasCheckConstraint("CK_ReinforcementLearning_HyperparameterSet_StepsPerEpisode", "[StepsPerEpisode] > 0");
        });
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_HyperparameterSet");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new HyperparameterSetId(value))
            .HasColumnName("HyperparameterSetId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LearningRateAlpha).HasColumnType("decimal(10,6)").IsRequired();
        builder.Property(x => x.DiscountGamma).HasColumnType("decimal(10,6)").IsRequired();
        builder.Property(x => x.EpsilonStart).HasColumnType("decimal(10,6)").IsRequired();
        builder.Property(x => x.EpsilonEnd).HasColumnType("decimal(10,6)").IsRequired();
        builder.Property(x => x.EpsilonDecay).HasColumnType("decimal(10,6)").IsRequired();
        builder.Property(x => x.EpisodeCount).IsRequired();
        builder.Property(x => x.StepsPerEpisode).IsRequired();
        builder.Property(x => x.RandomSeed);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_HyperparameterSet_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
