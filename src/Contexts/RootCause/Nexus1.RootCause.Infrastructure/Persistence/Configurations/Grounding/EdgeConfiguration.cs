using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.Grounding;

public sealed class EdgeConfiguration : IEntityTypeConfiguration<Edge>
{
    public void Configure(EntityTypeBuilder<Edge> builder)
    {
        builder.ToTable("Edge", "RootCause");
        builder.HasKey(x => x.EdgeId).HasName("PK_RootCause_Edge");

        builder.Property(x => x.EdgeId).ValueGeneratedOnAdd();
        builder.Property(x => x.FromComponentId).IsRequired();
        builder.Property(x => x.ToComponentId).IsRequired();
        builder.Property(x => x.Kind).HasMaxLength(12).IsRequired();
        builder.Property(x => x.DelayMinSeconds).IsRequired();
        builder.Property(x => x.DelayMaxSeconds).IsRequired();
        builder.Property(x => x.SourceRef).HasMaxLength(64);

        builder.HasOne<Component>().WithMany().HasForeignKey(x => x.FromComponentId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_RootCause_Edge_FromComponentId");
        builder.HasOne<Component>().WithMany().HasForeignKey(x => x.ToComponentId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_RootCause_Edge_ToComponentId");

        builder.HasIndex(x => x.FromComponentId).HasDatabaseName("IX_RootCause_Edge_FromComponentId");
    }
}
