using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.RootCause;

public sealed class AnalysisHypothesisConfiguration : IEntityTypeConfiguration<AnalysisHypothesis>
{
    public void Configure(EntityTypeBuilder<AnalysisHypothesis> builder)
    {
        builder.ToTable("AnalysisHypothesis", "RootCause");
        builder.HasKey(x => x.Id).HasName("PK_RootCause_AnalysisHypothesis");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AnalysisHypothesisId(value))
            .HasColumnName("AnalysisHypothesisId")
            .ValueGeneratedNever();

        builder.Property(x => x.HypothesisStatement).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.Property(x => x.RejectedAtUtc);

        builder.HasMany(x => x.Evidence)
            .WithOne()
            .HasForeignKey("AnalysisHypothesisId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RootCause_HypothesisEvidence_AnalysisHypothesisId");
        builder.Navigation(x => x.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
