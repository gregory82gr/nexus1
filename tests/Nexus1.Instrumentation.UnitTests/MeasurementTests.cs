using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class MeasurementTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_numeric_value_only_succeeds()
    {
        var measurement = Measurement.Create(
            new MeasurementId(1), new SignalId(1), new SignalQualityId(1), new MeasurementSourceId(1),
            NowUtc, NowUtc, numericValue: 42.5);

        Assert.Equal(42.5, measurement.NumericValue);
        Assert.Null(measurement.TextValue);
    }

    [Fact]
    public void Create_with_text_value_only_succeeds()
    {
        var measurement = Measurement.Create(
            new MeasurementId(1), new SignalId(1), new SignalQualityId(1), new MeasurementSourceId(1),
            NowUtc, NowUtc, textValue: "TRIPPED");

        Assert.Null(measurement.NumericValue);
        Assert.Equal("TRIPPED", measurement.TextValue);
    }

    [Fact]
    public void Create_with_both_numeric_and_text_value_succeeds()
    {
        var measurement = Measurement.Create(
            new MeasurementId(1), new SignalId(1), new SignalQualityId(1), new MeasurementSourceId(1),
            NowUtc, NowUtc, numericValue: 42.5, textValue: "OK");

        Assert.Equal(42.5, measurement.NumericValue);
        Assert.Equal("OK", measurement.TextValue);
    }

    [Fact]
    public void Create_with_neither_numeric_nor_text_value_throws()
    {
        Assert.Throws<ArgumentException>(() => Measurement.Create(
            new MeasurementId(1), new SignalId(1), new SignalQualityId(1), new MeasurementSourceId(1), NowUtc, NowUtc));
    }

    [Fact]
    public void Create_defaults_IsEstimated_to_false()
    {
        var measurement = Measurement.Create(
            new MeasurementId(1), new SignalId(1), new SignalQualityId(1), new MeasurementSourceId(1),
            NowUtc, NowUtc, numericValue: 1.0);

        Assert.False(measurement.IsEstimated);
    }
}
