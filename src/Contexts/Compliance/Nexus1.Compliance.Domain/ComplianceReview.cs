using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Compliance.Domain;

/// <summary>
/// Consumer-owned review per Executable Asset 34-AK, reduced to this
/// project's actual envelope fields (ADR-011): a single SourceAnalysisId
/// stands in for the book's separate SourceVerdictId/SourceCaseId, and
/// Verdict (a plain string) stands in for the book's cryptographic
/// ObservedVerdictIdentity hash — this project computes neither a separate
/// verdict/case split nor a verdict-identity digest (ADR-005). No
/// EnvelopeBytes/EnvelopeSha256: unlike AuditEvidenceRecord, Compliance does
/// not copy the full envelope — contract minimization (ch.34, 34-AL), Audit
/// already owns the evidentiary copy.
///
/// Deliberately mutable, unlike AuditEvidenceRecord: State has a private
/// setter because ch.34's own authority model (34-AL) reserves future
/// review-assignment/findings/decision mutation to Compliance — locking
/// this down the way Audit's evidence is locked down would contradict the
/// book's own design, even though nothing in this step actually calls a
/// mutating method yet.
/// </summary>
public sealed class ComplianceReview : Entity<ComplianceReviewId>, IAggregateRoot
{
    private ComplianceReview(ComplianceReviewId id, Guid sourceMessageId, long sourceAnalysisId, string verdict, DateTime openedAtUtc)
        : base(id)
    {
        SourceMessageId = sourceMessageId;
        SourceAnalysisId = sourceAnalysisId;
        Verdict = verdict;
        State = ComplianceReviewState.Pending;
        OpenedAtUtc = openedAtUtc;
    }

    public Guid SourceMessageId { get; }

    public long SourceAnalysisId { get; }

    public string Verdict { get; }

    public ComplianceReviewState State { get; private set; }

    public DateTime OpenedAtUtc { get; }

    public static ComplianceReview Open(ComplianceReviewId id, Guid sourceMessageId, long sourceAnalysisId, string verdict, DateTime openedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(verdict))
        {
            throw new ArgumentException("Verdict must not be empty.", nameof(verdict));
        }

        return new ComplianceReview(id, sourceMessageId, sourceAnalysisId, verdict, openedAtUtc);
    }
}
