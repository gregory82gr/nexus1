using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.Grounding;

public sealed class ComponentConfiguration : IEntityTypeConfiguration<Component>
{
    public void Configure(EntityTypeBuilder<Component> builder)
    {
        builder.ToTable("Component", "RootCause");
        builder.HasKey(x => x.ComponentId).HasName("PK_RootCause_Component");

        // Seeded with explicit ids (the incident's real component ids), not generated.
        builder.Property(x => x.ComponentId).ValueGeneratedNever();
        builder.Property(x => x.UnitId).IsRequired();
        builder.Property(x => x.Tag).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Kind).HasMaxLength(32).IsRequired();
        builder.Property(x => x.HealthScore);
        builder.Property(x => x.Status).HasMaxLength(16).IsRequired();
        builder.Property(x => x.AlarmCount).IsRequired();
        builder.Property(x => x.IllustrativeWeight);
        builder.Property(x => x.IllustrativeRole).HasMaxLength(12);

        builder.HasIndex(x => x.Tag).IsUnique().HasDatabaseName("UQ_RootCause_Component_Tag");
    }
}
