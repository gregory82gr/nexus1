using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>No audit columns at all — the real DDL gives this table none (ADR-026). Bigint identity key.</summary>
public sealed class PolicyEntryConfiguration : IEntityTypeConfiguration<PolicyEntry>
{
    public void Configure(EntityTypeBuilder<PolicyEntry> builder)
    {
        builder.ToTable("PolicyEntry", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_PolicyEntry");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PolicyEntryId(value))
            .HasColumnName("PolicyEntryId")
            .ValueGeneratedNever();

        builder.Property(x => x.PolicyId)
            .HasConversion(id => id.Value, value => new PolicyId(value))
            .HasColumnName("PolicyId")
            .IsRequired();

        builder.Property(x => x.StateDefinitionId)
            .HasConversion(id => id.Value, value => new StateDefinitionId(value))
            .HasColumnName("StateDefinitionId")
            .IsRequired();

        builder.Property(x => x.BestActionDefinitionId)
            .HasConversion(id => id.Value, value => new ActionDefinitionId(value))
            .HasColumnName("BestActionDefinitionId")
            .IsRequired();

        builder.Property(x => x.BestQValue).HasColumnType("decimal(20,10)").IsRequired();
        builder.Property(x => x.SecondBestQValue).HasColumnType("decimal(20,10)");
        builder.Property(x => x.ActionMargin).HasColumnType("decimal(20,10)");
        builder.Property(x => x.IsTie).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => new { x.PolicyId, x.StateDefinitionId })
            .IsUnique()
            .HasDatabaseName("UQ_ReinforcementLearning_PolicyEntry_Policy_StateDefinition");

        builder.HasOne<Policy>()
            .WithMany()
            .HasForeignKey(x => x.PolicyId)
            .HasConstraintName("FK_ReinforcementLearning_PolicyEntry_Policy")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StateDefinition>()
            .WithMany()
            .HasForeignKey(x => x.StateDefinitionId)
            .HasConstraintName("FK_ReinforcementLearning_PolicyEntry_StateDefinition")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionDefinition>()
            .WithMany()
            .HasForeignKey(x => x.BestActionDefinitionId)
            .HasConstraintName("FK_ReinforcementLearning_PolicyEntry_ActionDefinition")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
