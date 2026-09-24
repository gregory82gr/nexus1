using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Reporting.Infrastructure.Projection;

public sealed class PendingInconclusiveConfiguration : IEntityTypeConfiguration<PendingInconclusive>
{
    public void Configure(EntityTypeBuilder<PendingInconclusive> builder)
    {
        builder.ToTable("PendingInconclusive", "Reporting");
        builder.HasKey(x => x.AnalysisId).HasName("PK_Reporting_PendingInconclusive");

        builder.Property(x => x.AnalysisId).ValueGeneratedNever();
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.DecidedAtUtc).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).IsRequired();
    }
}
