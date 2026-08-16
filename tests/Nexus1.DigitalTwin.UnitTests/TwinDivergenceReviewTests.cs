using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class TwinDivergenceReviewTests
{
    private static readonly DateTime ReviewedAtUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_minimal_fields_succeeds()
    {
        var review = TwinDivergenceReview.Create(
            new TwinDivergenceReviewId(1), new TwinDivergenceId(1), new DivergenceStatusId(1), ReviewedAtUtc);

        Assert.Equal(ReviewedAtUtc, review.ReviewedAtUtc);
        Assert.Null(review.ReviewedByUserId);
    }

    [Fact]
    public void Create_with_full_disposition_records_it()
    {
        var review = TwinDivergenceReview.Create(
            new TwinDivergenceReviewId(1), new TwinDivergenceId(1), new DivergenceStatusId(1), ReviewedAtUtc,
            reviewedByUserId: 7, reviewNote: "Sensor drift confirmed.", correctiveAction: "Recalibrate SB-104.");

        Assert.Equal(7, review.ReviewedByUserId);
        Assert.Equal("Sensor drift confirmed.", review.ReviewNote);
        Assert.Equal("Recalibrate SB-104.", review.CorrectiveAction);
    }
}
