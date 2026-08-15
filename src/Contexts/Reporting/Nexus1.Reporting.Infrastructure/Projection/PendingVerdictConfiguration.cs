using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Reporting.Infrastructure.Projection;

public sealed class PendingVerdictConfiguration : IEntityTypeConfiguration<PendingVerdict>
{
    public void Configure(EntityTypeBuilder<PendingVerdict> builder)
    {
        builder.ToTable("PendingVerdict", "Reporting");
        builder.HasKey(x => x.AnalysisId).HasName("PK_Reporting_PendingVerdict");

        builder.Property(x => x.AnalysisId).ValueGeneratedNever();
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.Verdict).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.VerdictIssuedAtUtc).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).IsRequired();
    }
}
