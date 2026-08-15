using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Compliance.Domain;

namespace Nexus1.Compliance.Infrastructure.Persistence.Configurations.Compliance;

public sealed class ComplianceReviewConfiguration : IEntityTypeConfiguration<ComplianceReview>
{
    public void Configure(EntityTypeBuilder<ComplianceReview> builder)
    {
        builder.ToTable("ComplianceReview", "Compliance");
        builder.HasKey(x => x.Id).HasName("PK_Compliance_ComplianceReview");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ComplianceReviewId(value))
            .HasColumnName("ComplianceReviewId")
            .ValueGeneratedNever();

        builder.Property(x => x.SourceMessageId).IsRequired();
        builder.Property(x => x.SourceAnalysisId).IsRequired();
        builder.Property(x => x.Verdict).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.State).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.OpenedAtUtc).IsRequired();

        // The semantic half of the two-key dedup oracle (ch.34 34-AO,
        // ADR-011) — one business review per verdict.
        builder.HasIndex(x => x.SourceAnalysisId)
            .IsUnique()
            .HasDatabaseName("UX_Compliance_ComplianceReview_SourceAnalysisId");

        builder.Ignore(x => x.DomainEvents);
    }
}
