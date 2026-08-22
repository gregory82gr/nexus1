using Nexus1.Reporting.Domain;

namespace Nexus1.Reporting.UnitTests;

public class RootCauseCaseSummaryTests
{
    private static readonly DateTime OpenedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private static RootCauseCaseSummary ApplyOpenedSample() => RootCauseCaseSummary.ApplyOpened(
        new RootCauseCaseSummaryId(700), unitId: 1, alarmFloodId: 500, OpenedAtUtc, appliedAtUtc: OpenedAtUtc, messageId: Guid.NewGuid());

    [Fact]
    public void ApplyOpened_starts_in_open_status_with_no_verdict()
    {
        var summary = ApplyOpenedSample();

        Assert.Equal(new RootCauseCaseSummaryId(700), summary.Id);
        Assert.Equal(1, summary.UnitId);
        Assert.Equal(500, summary.AlarmFloodId);
        Assert.Equal(ReportingCaseStatus.Open, summary.Status);
        Assert.Null(summary.Verdict);
        Assert.Null(summary.VerdictIssuedAtUtc);
    }

    [Fact]
    public void ApplyVerdictIssued_advances_to_verdict_issued_and_copies_every_field()
    {
        var summary = ApplyOpenedSample();
        var verdictIssuedAtUtc = OpenedAtUtc.AddHours(2);
        var appliedAtUtc = verdictIssuedAtUtc.AddSeconds(1);
        var messageId = Guid.NewGuid();

        summary.ApplyVerdictIssued("Loose fitting confirmed as cause.", verdictIssuedAtUtc, appliedAtUtc, messageId);

        Assert.Equal(ReportingCaseStatus.VerdictIssued, summary.Status);
        Assert.Equal("Loose fitting confirmed as cause.", summary.Verdict);
        Assert.Equal(verdictIssuedAtUtc, summary.VerdictIssuedAtUtc);
        Assert.Equal(appliedAtUtc, summary.LastAppliedAtUtc);
        Assert.Equal(messageId, summary.LastAppliedMessageId);
    }

    [Fact]
    public void ApplyVerdictIssued_twice_throws()
    {
        var summary = ApplyOpenedSample();
        summary.ApplyVerdictIssued("Confirmed.", OpenedAtUtc, OpenedAtUtc, Guid.NewGuid());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            summary.ApplyVerdictIssued("Different verdict.", OpenedAtUtc, OpenedAtUtc, Guid.NewGuid()));
        Assert.Contains("already has an issued verdict", ex.Message);
    }

    [Fact]
    public void ApplyVerdictIssued_rejects_an_empty_verdict()
    {
        var summary = ApplyOpenedSample();

        var ex = Assert.Throws<ArgumentException>(() =>
            summary.ApplyVerdictIssued("  ", OpenedAtUtc, OpenedAtUtc, Guid.NewGuid()));
        Assert.Equal("verdict", ex.ParamName);
    }
}
