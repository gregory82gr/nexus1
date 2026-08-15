using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Audit.Domain;

namespace Nexus1.Audit.Infrastructure.Persistence.Configurations.Audit;

public sealed class AuditEvidenceRecordConfiguration : IEntityTypeConfiguration<AuditEvidenceRecord>
{
    public void Configure(EntityTypeBuilder<AuditEvidenceRecord> builder)
    {
        builder.ToTable("AuditEvidenceRecord", "Audit");
        builder.HasKey(x => x.Id).HasName("PK_Audit_AuditEvidenceRecord");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AuditEvidenceId(value))
            .HasColumnName("AuditEvidenceId")
            .ValueGeneratedNever();

        builder.Property(x => x.SourceMessageId).IsRequired();
        builder.Property(x => x.SourceAnalysisId).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.EnvelopeBytes).HasColumnType("varbinary(max)").IsRequired();
        builder.Property(x => x.EnvelopeSha256).HasColumnType("binary(32)").IsRequired();
        builder.Property(x => x.CorrelationId).IsRequired();
        builder.Property(x => x.CausationId);
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.RecordedAtUtc).IsRequired();

        // The semantic half of the two-key dedup oracle (ch.34 34-AI,
        // ADR-010) — transport dedup alone (InboxReceipt) can't catch a
        // replay arriving under a new MessageId for the same verdict.
        builder.HasIndex(x => x.SourceAnalysisId)
            .IsUnique()
            .HasDatabaseName("UX_Audit_AuditEvidenceRecord_SourceAnalysisId");

        builder.Ignore(x => x.DomainEvents);
    }
}
