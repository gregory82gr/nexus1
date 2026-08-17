using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

public class AssetConditionMeasurementTests
{
    private static readonly DateTime MeasuredAtUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var measurement = AssetConditionMeasurement.Create(
            new AssetConditionMeasurementId(1), new AssetConditionId(1), engineeringUnitId: 1, measuredValue: 12.5,
            measuredAtUtc: MeasuredAtUtc);

        Assert.Equal(12.5, measurement.MeasuredValue);
        Assert.Null(measurement.SignalId);
    }

    [Fact]
    public void Create_with_signal_id_sets_it()
    {
        var measurement = AssetConditionMeasurement.Create(
            new AssetConditionMeasurementId(1), new AssetConditionId(1), engineeringUnitId: 1, measuredValue: 12.5,
            measuredAtUtc: MeasuredAtUtc, signalId: 99);

        Assert.Equal(99, measurement.SignalId);
    }
}
