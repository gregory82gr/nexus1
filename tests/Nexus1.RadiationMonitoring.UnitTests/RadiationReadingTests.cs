using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class RadiationReadingTests
{
    private static readonly DateTime TimestampUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var reading = RadiationReading.Create(
            new RadiationReadingId(1), new RadiationMonitorId(1), new MeasurementTypeId(1), 1,
            new MeasurementQualityId(1), TimestampUtc, 0.15m);

        Assert.Equal(0.15m, reading.Value);
        Assert.Equal(TimestampUtc, reading.TimestampUtc);
        Assert.False(reading.IsAlarmRelevant);
        Assert.Null(reading.SourceTimestampUtc);
    }

    [Fact]
    public void Create_with_engineering_unit_id_sets_it_with_no_enforced_fk_at_the_domain_layer()
    {
        var reading = RadiationReading.Create(
            new RadiationReadingId(1), new RadiationMonitorId(1), new MeasurementTypeId(1), 3,
            new MeasurementQualityId(1), TimestampUtc, 0.42m, isAlarmRelevant: true,
            sourceTimestampUtc: TimestampUtc.AddSeconds(-5));

        Assert.Equal(3, reading.EngineeringUnitId);
        Assert.True(reading.IsAlarmRelevant);
        Assert.Equal(TimestampUtc.AddSeconds(-5), reading.SourceTimestampUtc);
    }
}
