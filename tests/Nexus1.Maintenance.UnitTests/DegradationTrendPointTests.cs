using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

public class DegradationTrendPointTests
{
    private static readonly DateTime MeasuredAtUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var trendPoint = DegradationTrendPoint.Create(
            new DegradationTrendPointId(1), new DegradationRecordId(1), engineeringUnitId: 1, measuredAtUtc: MeasuredAtUtc, value: 0.35);

        Assert.Equal(0.35, trendPoint.Value);
        Assert.Null(trendPoint.SourceSignalId);
    }

    [Fact]
    public void Create_with_source_signal_id_sets_it()
    {
        var trendPoint = DegradationTrendPoint.Create(
            new DegradationTrendPointId(1), new DegradationRecordId(1), engineeringUnitId: 1, measuredAtUtc: MeasuredAtUtc,
            value: 0.35, sourceSignalId: 77);

        Assert.Equal(77, trendPoint.SourceSignalId);
    }
}
