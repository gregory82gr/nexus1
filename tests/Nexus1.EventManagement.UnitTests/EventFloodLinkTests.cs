using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.UnitTests;

public class EventFloodLinkTests
{
    [Fact]
    public void Create_with_valid_fields_defaults_link_role_to_trigger()
    {
        var link = EventFloodLink.Create(new EventFloodLinkId(1), operationalEventId: 100L, alarmFloodId: 300L);

        Assert.Equal(new OperationalEventId(100L), link.OperationalEventId);
        Assert.Equal(300L, link.AlarmFloodId);
        Assert.Equal("TRIGGER", link.LinkRole);
        Assert.Null(link.Note);
    }

    [Fact]
    public void Create_with_explicit_link_role_and_note_sets_them()
    {
        var link = EventFloodLink.Create(new EventFloodLinkId(1), 100L, 300L, linkRole: "CONTEXT", note: "Concurrent flood");

        Assert.Equal("CONTEXT", link.LinkRole);
        Assert.Equal("Concurrent flood", link.Note);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_link_role_throws(string linkRole)
    {
        Assert.Throws<ArgumentException>(() => EventFloodLink.Create(new EventFloodLinkId(1), 100L, 300L, linkRole));
    }
}
