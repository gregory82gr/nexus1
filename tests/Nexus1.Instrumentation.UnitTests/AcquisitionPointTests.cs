using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class AcquisitionPointTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var point = AcquisitionPoint.Create(
            new AcquisitionPointId(1), new AcquisitionConnectionId(1), "PT-1", "ns=2;s=Reactor.Power", NowUtc,
            scaleFactor: 1.0m, offsetValue: 0.0m);

        Assert.Equal("PT-1", point.Code);
        Assert.Equal(1.0m, point.ScaleFactor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_raw_address_throws(string rawAddress)
    {
        Assert.Throws<ArgumentException>(() => AcquisitionPoint.Create(
            new AcquisitionPointId(1), new AcquisitionConnectionId(1), "PT-1", rawAddress, NowUtc));
    }
}
