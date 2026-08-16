using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class TimeZoneReferenceTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_offset_succeeds()
    {
        var timeZone = TimeZoneReference.Create(new TimeZoneReferenceId(1), "Europe/Athens", "Athens", 120, NowUtc);

        Assert.Equal("Europe/Athens", timeZone.IanaName);
        Assert.Equal((short)120, timeZone.CurrentUtcOffsetMinutes);
    }

    [Theory]
    [InlineData((short)-841)]
    [InlineData((short)841)]
    public void Create_with_offset_outside_range_throws(short offset)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TimeZoneReference.Create(
            new TimeZoneReferenceId(1), "Europe/Athens", "Athens", offset, NowUtc));
    }
}
