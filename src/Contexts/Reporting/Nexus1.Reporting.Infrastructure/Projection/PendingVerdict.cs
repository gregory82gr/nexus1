namespace Nexus1.Reporting.Infrastructure.Projection;

/// <summary>
/// Reduced stand-in for the book's stream-position gap buffer (ADR-012):
/// keyed by AnalysisId rather than (StreamId, Position). Holds a
/// RootCauseVerdictIssuedV1 that arrived before its case's
/// RootCauseCaseSummary row existed — applied and removed once the matching
/// RootCauseCaseOpenedV1 creates that row. Infrastructure plumbing, not a
/// domain concept, same reasoning as InboxReceipt/OutboxMessage.
/// </summary>
public sealed class PendingVerdict
{
    private PendingVerdict()
    {
        Verdict = null!;
    }

    public PendingVerdict(long analysisId, Guid messageId, string verdict, DateTime verdictIssuedAtUtc, DateTime receivedAtUtc)
    {
        AnalysisId = analysisId;
        MessageId = messageId;
        Verdict = verdict;
        VerdictIssuedAtUtc = verdictIssuedAtUtc;
        ReceivedAtUtc = receivedAtUtc;
    }

    public long AnalysisId { get; private set; }

    public Guid MessageId { get; private set; }

    public string Verdict { get; private set; }

    public DateTime VerdictIssuedAtUtc { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }
}
