using Nexus1.Audit.Domain;

namespace Nexus1.Audit.UnitTests;

public class AuditEvidenceRecordTests
{
    private static readonly DateTime OccurredAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime RecordedAtUtc = OccurredAtUtc.AddSeconds(2);

    [Fact]
    public void Append_copies_every_field_verbatim()
    {
        var id = new AuditEvidenceId(Guid.NewGuid());
        var sourceMessageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var envelopeBytes = new byte[] { 1, 2, 3 };
        var envelopeSha256 = new byte[32];

        var record = AuditEvidenceRecord.Append(
            id, sourceMessageId, sourceAnalysisId: 500, eventType: "nexus1.root-cause.root-cause-verdict-issued.v1",
            schemaVersion: 1, envelopeBytes, envelopeSha256, correlationId, causationId, OccurredAtUtc, RecordedAtUtc);

        Assert.Equal(id, record.Id);
        Assert.Equal(sourceMessageId, record.SourceMessageId);
        Assert.Equal(500, record.SourceAnalysisId);
        Assert.Equal("nexus1.root-cause.root-cause-verdict-issued.v1", record.EventType);
        Assert.Equal(1, record.SchemaVersion);
        Assert.Equal(envelopeBytes, record.EnvelopeBytes);
        Assert.Equal(envelopeSha256, record.EnvelopeSha256);
        Assert.Equal(correlationId, record.CorrelationId);
        Assert.Equal(causationId, record.CausationId);
        Assert.Equal(OccurredAtUtc, record.OccurredAtUtc);
        Assert.Equal(RecordedAtUtc, record.RecordedAtUtc);
    }

    [Fact]
    public void Append_allows_a_null_causation_id()
    {
        var record = AuditEvidenceRecord.Append(
            new AuditEvidenceId(Guid.NewGuid()), Guid.NewGuid(), sourceAnalysisId: 500,
            eventType: "nexus1.root-cause.root-cause-verdict-issued.v1", schemaVersion: 1,
            [], new byte[32], Guid.NewGuid(), causationId: null, OccurredAtUtc, RecordedAtUtc);

        Assert.Null(record.CausationId);
    }

    [Fact]
    public void Append_rejects_an_empty_event_type()
    {
        var ex = Assert.Throws<ArgumentException>(() => AuditEvidenceRecord.Append(
            new AuditEvidenceId(Guid.NewGuid()), Guid.NewGuid(), sourceAnalysisId: 500,
            eventType: "  ", schemaVersion: 1, [], new byte[32], Guid.NewGuid(), null, OccurredAtUtc, RecordedAtUtc));

        Assert.Equal("eventType", ex.ParamName);
    }
}
