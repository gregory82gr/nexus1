using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void SignalType_Create_with_valid_fields_succeeds()
    {
        var type = SignalType.Create(new SignalTypeId(1), "MEASURED", "Measured", NowUtc);
        Assert.Equal("MEASURED", type.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SignalType_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => SignalType.Create(new SignalTypeId(1), code, "Measured", NowUtc));
    }

    [Fact]
    public void SignalCategory_Create_with_valid_fields_succeeds()
    {
        var category = SignalCategory.Create(new SignalCategoryId(1), "POWER", "Power", NowUtc);
        Assert.Equal("POWER", category.Code);
    }

    [Fact]
    public void SignalRole_Create_with_valid_fields_succeeds()
    {
        var role = SignalRole.Create(new SignalRoleId(1), "TELEMETRY", "Telemetry", NowUtc);
        Assert.Equal("TELEMETRY", role.Code);
    }

    [Fact]
    public void SamplingMode_Create_with_valid_fields_succeeds()
    {
        var mode = SamplingMode.Create(new SamplingModeId(1), "PERIODIC", "Periodic", NowUtc);
        Assert.Equal("PERIODIC", mode.Code);
    }

    [Fact]
    public void HistorianRetentionClass_Create_with_valid_fields_succeeds()
    {
        var retentionClass = HistorianRetentionClass.Create(new HistorianRetentionClassId(1), "STANDARD", "Standard", NowUtc);
        Assert.Equal("STANDARD", retentionClass.Code);
    }

    [Fact]
    public void SignalQuality_Create_with_valid_fields_succeeds()
    {
        var quality = SignalQuality.Create(new SignalQualityId(1), "GOOD", "Good", NowUtc);
        Assert.Equal("GOOD", quality.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SignalQuality_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => SignalQuality.Create(new SignalQualityId(1), "GOOD", name, NowUtc));
    }

    [Fact]
    public void MeasurementSource_Create_with_valid_fields_succeeds()
    {
        var source = MeasurementSource.Create(new MeasurementSourceId(1), "HISTORIAN", "Historian", NowUtc);
        Assert.Equal("HISTORIAN", source.Code);
    }

    [Fact]
    public void ChannelStatus_Create_with_valid_fields_succeeds()
    {
        var status = ChannelStatus.Create(new ChannelStatusId(1), "ONLINE", "Online", NowUtc);
        Assert.Equal("ONLINE", status.Code);
    }
}
