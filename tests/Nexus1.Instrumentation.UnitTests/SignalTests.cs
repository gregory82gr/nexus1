using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class SignalTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static Signal CreateSignal(
        decimal? normalMin = null, decimal? normalMax = null, decimal? scanRateHz = null) => Signal.Create(
        new SignalId(1), unitId: 1, new SignalTypeId(1), new SignalCategoryId(1), new SignalRoleId(1),
        engineeringUnitId: 1, new SamplingModeId(1), new HistorianRetentionClassId(1), "NX1-U1.RX.POWER",
        "Reactor Power", NowUtc, normalMin: normalMin, normalMax: normalMax, scanRateHz: scanRateHz);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var signal = CreateSignal();

        Assert.Equal("NX1-U1.RX.POWER", signal.Tag);
        Assert.False(signal.IsSafetyRelated);
        Assert.True(signal.IsHistorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_tag_throws(string tag)
    {
        Assert.Throws<ArgumentException>(() => Signal.Create(
            new SignalId(1), unitId: 1, new SignalTypeId(1), new SignalCategoryId(1), new SignalRoleId(1),
            engineeringUnitId: 1, new SamplingModeId(1), new HistorianRetentionClassId(1), tag, "Reactor Power", NowUtc));
    }

    [Fact]
    public void Create_with_normal_max_greater_than_normal_min_succeeds()
    {
        var signal = CreateSignal(normalMin: 0m, normalMax: 100m);

        Assert.Equal(0m, signal.NormalMin);
        Assert.Equal(100m, signal.NormalMax);
    }

    [Fact]
    public void Create_with_normal_max_equal_to_normal_min_throws()
    {
        Assert.Throws<ArgumentException>(() => CreateSignal(normalMin: 50m, normalMax: 50m));
    }

    [Fact]
    public void Create_with_normal_max_less_than_normal_min_throws()
    {
        Assert.Throws<ArgumentException>(() => CreateSignal(normalMin: 100m, normalMax: 0m));
    }

    [Fact]
    public void Create_with_positive_scan_rate_succeeds()
    {
        var signal = CreateSignal(scanRateHz: 10m);

        Assert.Equal(10m, signal.ScanRateHz);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_with_non_positive_scan_rate_throws(int scanRateHz)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateSignal(scanRateHz: scanRateHz));
    }
}
