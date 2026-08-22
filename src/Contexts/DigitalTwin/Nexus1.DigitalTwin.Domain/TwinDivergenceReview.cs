using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Human or system review of a divergence: explained, corrected, waived or
/// closed (atlas C.6.1/C.6.2). Closes the review loop C.6.1's design choice
/// explicitly asks for — recording disagreement without a review path would
/// only half-honor "a twin that records disagreement can be corrected,
/// audited and used honestly" (ADR-020).
///
/// Leaner audit shape than most tables in this sector — the atlas DDL gives
/// this table only CreatedAtUtc (no RowVersion), verified directly against
/// C.6.4.5. ReviewedAtUtc is the real business timestamp (when the review
/// happened) and is modeled here; CreatedAtUtc is pure row-insertion
/// bookkeeping with a SQL DEFAULT and is not modeled.
///
/// ReviewedByUserId is a Security.ApplicationUser passport int — no
/// enforced FK (ADR-020, SecurityDb is a separate physical database).
/// </summary>
public sealed class TwinDivergenceReview : Entity<TwinDivergenceReviewId>, IAggregateRoot
{
    private TwinDivergenceReview(
        TwinDivergenceReviewId id, TwinDivergenceId twinDivergenceId, DivergenceStatusId divergenceStatusId,
        int? reviewedByUserId, DateTime reviewedAtUtc, string? reviewNote, string? correctiveAction)
        : base(id)
    {
        TwinDivergenceId = twinDivergenceId;
        DivergenceStatusId = divergenceStatusId;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = reviewedAtUtc;
        ReviewNote = reviewNote;
        CorrectiveAction = correctiveAction;
    }

    public TwinDivergenceId TwinDivergenceId { get; }

    public DivergenceStatusId DivergenceStatusId { get; }

    /// <summary>Security.ApplicationUser passport id — no enforced FK (ADR-020).</summary>
    public int? ReviewedByUserId { get; }

    public DateTime ReviewedAtUtc { get; }

    public string? ReviewNote { get; }

    public string? CorrectiveAction { get; }

    public static TwinDivergenceReview Create(
        TwinDivergenceReviewId id, TwinDivergenceId twinDivergenceId, DivergenceStatusId divergenceStatusId,
        DateTime reviewedAtUtc, int? reviewedByUserId = null, string? reviewNote = null, string? correctiveAction = null)
    {
        return new TwinDivergenceReview(
            id, twinDivergenceId, divergenceStatusId, reviewedByUserId, reviewedAtUtc, reviewNote, correctiveAction);
    }
}
