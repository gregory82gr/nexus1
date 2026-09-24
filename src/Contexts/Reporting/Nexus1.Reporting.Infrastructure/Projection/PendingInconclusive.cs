namespace Nexus1.Reporting.Infrastructure.Projection;

/// <summary>
/// The Inconclusive counterpart to PendingVerdict (ADR-039): holds a
/// RootCauseCaseInconclusiveV1 that arrived before its case's RootCauseCaseSummary row
/// existed — applied and removed once the matching RootCauseCaseOpenedV1 creates that
/// row. A separate, parallel buffer rather than a generalization of PendingVerdict, so
/// the working verdict path is left untouched (ADR-039). Infrastructure plumbing, not a
/// domain concept.
/// </summary>
public sealed class PendingInconclusive
{
    private PendingInconclusive()
    {
        Reason = null!;
    }

    public PendingInconclusive(long analysisId, Guid messageId, string reason, DateTime decidedAtUtc, DateTime receivedAtUtc)
    {
        AnalysisId = analysisId;
        MessageId = messageId;
        Reason = reason;
        DecidedAtUtc = decidedAtUtc;
        ReceivedAtUtc = receivedAtUtc;
    }

    public long AnalysisId { get; private set; }

    public Guid MessageId { get; private set; }

    public string Reason { get; private set; }

    public DateTime DecidedAtUtc { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }
}
