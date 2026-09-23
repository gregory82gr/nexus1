using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.Grounding;

public sealed class DiagnosisRunConfiguration : IEntityTypeConfiguration<DiagnosisRun>
{
    public void Configure(EntityTypeBuilder<DiagnosisRun> builder)
    {
        builder.ToTable("DiagnosisRun", "RootCause");
        builder.HasKey(x => x.DiagnosisRunId).HasName("PK_RootCause_DiagnosisRun");

        builder.Property(x => x.DiagnosisRunId).ValueGeneratedOnAdd();
        builder.Property(x => x.IncidentId).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.Verdict).HasMaxLength(64);
        builder.Property(x => x.AbstainReason).HasMaxLength(256);
        builder.Property(x => x.CorpusVersion).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.IncidentId).HasDatabaseName("IX_RootCause_DiagnosisRun_IncidentId");
    }
}
