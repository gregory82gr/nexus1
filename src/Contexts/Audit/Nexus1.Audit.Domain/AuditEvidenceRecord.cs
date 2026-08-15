using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Audit.Domain;

/// <summary>
/// Append-only evidence record per From_Services_To_Runtime Executable Asset
/// 34-AE, reduced to this project's actual envelope fields (ADR-010): a
/// single SourceAnalysisId stands in for the book's separate VerdictId/
/// RootCauseCaseId, since ADR-005 already collapsed "verdict" and "case"
/// into one RootCauseAnalysis identity — there's nothing here for a second
/// field to carry. No public mutators exist at all — the domain shape alone
/// enforces "Audit never mutates history" (ch.34, 34-AF); the EF interceptor
/// (ADR-010) is the second, infrastructure-level line of defense against the
/// same rule.
/// </summary>
public sealed class AuditEvidenceRecord : Entity<AuditEvidenceId>, IAggregateRoot
{
    private AuditEvidenceRecord(
        AuditEvidenceId id, Guid sourceMessageId, long sourceAnalysisId, string eventType, int schemaVersion,
        byte[] envelopeBytes, byte[] envelopeSha256, Guid correlationId, Guid? causationId,
        DateTime occurredAtUtc, DateTime recordedAtUtc)
        : base(id)
    {
        SourceMessageId = sourceMessageId;
        SourceAnalysisId = sourceAnalysisId;
        EventType = eventType;
        SchemaVersion = schemaVersion;
        EnvelopeBytes = envelopeBytes;
        EnvelopeSha256 = envelopeSha256;
        CorrelationId = correlationId;
        CausationId = causationId;
        OccurredAtUtc = occurredAtUtc;
        RecordedAtUtc = recordedAtUtc;
    }

    public Guid SourceMessageId { get; }

    public long SourceAnalysisId { get; }

    public string EventType { get; }

    public int SchemaVersion { get; }

    public byte[] EnvelopeBytes { get; }

    public byte[] EnvelopeSha256 { get; }

    public Guid CorrelationId { get; }

    public Guid? CausationId { get; }

    public DateTime OccurredAtUtc { get; }

    public DateTime RecordedAtUtc { get; }

    public static AuditEvidenceRecord Append(
        AuditEvidenceId id, Guid sourceMessageId, long sourceAnalysisId, string eventType, int schemaVersion,
        byte[] envelopeBytes, byte[] envelopeSha256, Guid correlationId, Guid? causationId,
        DateTime occurredAtUtc, DateTime recordedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("EventType must not be empty.", nameof(eventType));
        }

        return new AuditEvidenceRecord(
            id, sourceMessageId, sourceAnalysisId, eventType, schemaVersion,
            envelopeBytes, envelopeSha256, correlationId, causationId, occurredAtUtc, recordedAtUtc);
    }
}
