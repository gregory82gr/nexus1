using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.Grounding;

public sealed class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidate", "RootCause");
        builder.HasKey(x => new { x.DiagnosisRunId, x.ComponentId }).HasName("PK_RootCause_Candidate");

        builder.Property(x => x.Role).HasMaxLength(12).IsRequired();
        builder.Property(x => x.Weight).IsRequired();
        builder.Property(x => x.Coverage);

        builder.HasOne<DiagnosisRun>().WithMany().HasForeignKey(x => x.DiagnosisRunId)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_RootCause_Candidate_DiagnosisRunId");
        builder.HasOne<Component>().WithMany().HasForeignKey(x => x.ComponentId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_RootCause_Candidate_ComponentId");
    }
}
