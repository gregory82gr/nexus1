using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>ModifiedAtUtc/RowVersion are EF-only shadow properties, not Domain-modeled (ADR-026).</summary>
public sealed class ActionSpaceTypeConfiguration : IEntityTypeConfiguration<ActionSpaceType>
{
    public void Configure(EntityTypeBuilder<ActionSpaceType> builder)
    {
        builder.ToTable("ActionSpaceType", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_ActionSpaceType");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ActionSpaceTypeId(value))
            .HasColumnName("ActionSpaceTypeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_ActionSpaceType_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
