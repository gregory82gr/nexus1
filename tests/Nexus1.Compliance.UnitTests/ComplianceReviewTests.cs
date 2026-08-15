using Nexus1.Compliance.Domain;

namespace Nexus1.Compliance.UnitTests;

public class ComplianceReviewTests
{
    private static readonly DateTime OpenedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Open_starts_in_pending_state_with_every_field_copied_verbatim()
    {
        var id = new ComplianceReviewId(Guid.NewGuid());
        var sourceMessageId = Guid.NewGuid();

        var review = ComplianceReview.Open(id, sourceMessageId, sourceAnalysisId: 700, "Loose fitting confirmed as cause.", OpenedAtUtc);

        Assert.Equal(id, review.Id);
        Assert.Equal(sourceMessageId, review.SourceMessageId);
        Assert.Equal(700, review.SourceAnalysisId);
        Assert.Equal("Loose fitting confirmed as cause.", review.Verdict);
        Assert.Equal(ComplianceReviewState.Pending, review.State);
        Assert.Equal(OpenedAtUtc, review.OpenedAtUtc);
    }

    [Fact]
    public void Open_rejects_an_empty_verdict()
    {
        var ex = Assert.Throws<ArgumentException>(() => ComplianceReview.Open(
            new ComplianceReviewId(Guid.NewGuid()), Guid.NewGuid(), sourceAnalysisId: 700, "  ", OpenedAtUtc));

        Assert.Equal("verdict", ex.ParamName);
    }
}
