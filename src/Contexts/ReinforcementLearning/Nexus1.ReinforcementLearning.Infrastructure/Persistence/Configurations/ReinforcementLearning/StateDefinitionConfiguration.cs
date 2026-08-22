using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>No audit columns at all — the real DDL gives this table none (ADR-026).</summary>
public sealed class StateDefinitionConfiguration : IEntityTypeConfiguration<StateDefinition>
{
    public void Configure(EntityTypeBuilder<StateDefinition> builder)
    {
        builder.ToTable("StateDefinition", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_StateDefinition");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new StateDefinitionId(value))
            .HasColumnName("StateDefinitionId")
            .ValueGeneratedNever();

        builder.Property(x => x.StateSpaceId)
            .HasConversion(id => id.Value, value => new StateSpaceId(value))
            .HasColumnName("StateSpaceId")
            .IsRequired();

        builder.Property(x => x.StateIndex).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DeviationBin).HasMaxLength(50);
        builder.Property(x => x.TrendBin).HasMaxLength(50);
        builder.Property(x => x.IsTerminal).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => new { x.StateSpaceId, x.StateIndex }).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_StateDefinition_StateSpace_StateIndex");
        builder.HasIndex(x => new { x.StateSpaceId, x.Code }).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_StateDefinition_StateSpace_Code");

        builder.HasOne<StateSpace>()
            .WithMany()
            .HasForeignKey(x => x.StateSpaceId)
            .HasConstraintName("FK_ReinforcementLearning_StateDefinition_StateSpace")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
