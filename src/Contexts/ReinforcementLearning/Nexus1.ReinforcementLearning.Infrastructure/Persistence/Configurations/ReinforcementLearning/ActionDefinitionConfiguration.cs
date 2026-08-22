using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>No audit columns at all — the real DDL gives this table none (ADR-026).</summary>
public sealed class ActionDefinitionConfiguration : IEntityTypeConfiguration<ActionDefinition>
{
    public void Configure(EntityTypeBuilder<ActionDefinition> builder)
    {
        builder.ToTable("ActionDefinition", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_ActionDefinition");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ActionDefinitionId(value))
            .HasColumnName("ActionDefinitionId")
            .ValueGeneratedNever();

        builder.Property(x => x.ActionSpaceId)
            .HasConversion(id => id.Value, value => new ActionSpaceId(value))
            .HasColumnName("ActionSpaceId")
            .IsRequired();

        builder.Property(x => x.ActionIndex).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ActionValue).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.IsNoOp).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => new { x.ActionSpaceId, x.ActionIndex }).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_ActionDefinition_ActionSpace_ActionIndex");
        builder.HasIndex(x => new { x.ActionSpaceId, x.Code }).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_ActionDefinition_ActionSpace_Code");

        builder.HasOne<ActionSpace>()
            .WithMany()
            .HasForeignKey(x => x.ActionSpaceId)
            .HasConstraintName("FK_ReinforcementLearning_ActionDefinition_ActionSpace")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
