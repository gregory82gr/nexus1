using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.Grounding;

public sealed class HistorianSampleConfiguration : IEntityTypeConfiguration<HistorianSample>
{
    public void Configure(EntityTypeBuilder<HistorianSample> builder)
    {
        builder.ToTable("Historian", "RootCause");

        // Composite key (ChannelId, TimestampUtc): append-only per-channel
        // samples. No columnstore index at skeleton scale (deferred, ADR-032).
        builder.HasKey(x => new { x.ChannelId, x.TimestampUtc }).HasName("PK_RootCause_Historian");

        builder.Property(x => x.ChannelId).IsRequired();
        builder.Property(x => x.TimestampUtc).IsRequired();
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.Quality).IsRequired();
    }
}
