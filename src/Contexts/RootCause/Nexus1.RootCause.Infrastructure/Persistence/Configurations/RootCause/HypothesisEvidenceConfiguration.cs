using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.RootCause;

public sealed class HypothesisEvidenceConfiguration : IEntityTypeConfiguration<HypothesisEvidence>
{
    public void Configure(EntityTypeBuilder<HypothesisEvidence> builder)
    {
        builder.ToTable("HypothesisEvidence", "RootCause");
        builder.HasKey(x => x.Id).HasName("PK_RootCause_HypothesisEvidence");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new HypothesisEvidenceId(value))
            .HasColumnName("HypothesisEvidenceId")
            .ValueGeneratedNever();

        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.RecordedAtUtc).IsRequired();

        builder.Ignore(x => x.DomainEvents);
    }
}
