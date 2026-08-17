using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.UnitTests;

public class EventAlarmLinkTests
{
    [Fact]
    public void Create_with_valid_fields_defaults_link_role_to_supporting()
    {
        var link = EventAlarmLink.Create(new EventAlarmLinkId(1), operationalEventId: 100L, alarmEventId: 200L);

        Assert.Equal(new OperationalEventId(100L), link.OperationalEventId);
        Assert.Equal(200L, link.AlarmEventId);
        Assert.Equal("SUPPORTING", link.LinkRole);
        Assert.Null(link.Note);
    }

    [Fact]
    public void Create_with_explicit_link_role_and_note_sets_them()
    {
        var link = EventAlarmLink.Create(new EventAlarmLinkId(1), 100L, 200L, linkRole: "CAUSAL", note: "Primary trigger");

        Assert.Equal("CAUSAL", link.LinkRole);
        Assert.Equal("Primary trigger", link.Note);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_link_role_throws(string linkRole)
    {
        Assert.Throws<ArgumentException>(() => EventAlarmLink.Create(new EventAlarmLinkId(1), 100L, 200L, linkRole));
    }
}
