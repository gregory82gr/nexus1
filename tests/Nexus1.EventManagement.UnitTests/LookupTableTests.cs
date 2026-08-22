using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void EventType_Create_with_valid_fields_succeeds()
    {
        var type = EventType.Create(new EventTypeId(1), "INCIDENT", "Incident", NowUtc);
        Assert.Equal("INCIDENT", type.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EventType_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => EventType.Create(new EventTypeId(1), code, "Incident", NowUtc));
    }

    [Fact]
    public void EventStatus_Create_with_valid_fields_succeeds()
    {
        var status = EventStatus.Create(new EventStatusId(1), "NEW", "New", NowUtc);
        Assert.Equal("NEW", status.Code);
    }

    [Fact]
    public void EventSeverity_Create_with_valid_fields_succeeds()
    {
        var severity = EventSeverity.Create(new EventSeverityId(1), "MAJOR", "Major", NowUtc);
        Assert.Equal("MAJOR", severity.Code);
    }

    [Fact]
    public void EventSourceType_Create_with_valid_fields_succeeds()
    {
        var sourceType = EventSourceType.Create(new EventSourceTypeId(1), "ALARM_FLOOD", "Alarm Flood", NowUtc);
        Assert.Equal("ALARM_FLOOD", sourceType.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EventSourceType_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => EventSourceType.Create(new EventSourceTypeId(1), "ALARM_FLOOD", name, NowUtc));
    }

    [Fact]
    public void EventTimelineEntryType_Create_with_valid_fields_succeeds()
    {
        var type = EventTimelineEntryType.Create(new EventTimelineEntryTypeId(1), "DETECTED", "Detected", NowUtc);
        Assert.Equal("DETECTED", type.Code);
    }

    [Fact]
    public void IncidentType_Create_with_valid_fields_succeeds()
    {
        var type = IncidentType.Create(new IncidentTypeId(1), "OPERATIONAL", "Operational", NowUtc);
        Assert.Equal("OPERATIONAL", type.Code);
    }

    [Fact]
    public void IncidentStatus_Create_with_valid_fields_succeeds()
    {
        var status = IncidentStatus.Create(new IncidentStatusId(1), "OPENED", "Opened", NowUtc);
        Assert.Equal("OPENED", status.Code);
    }

    [Fact]
    public void IncidentActionType_Create_with_valid_fields_succeeds()
    {
        var type = IncidentActionType.Create(new IncidentActionTypeId(1), "CORRECTIVE", "Corrective", NowUtc);
        Assert.Equal("CORRECTIVE", type.Code);
    }

    [Fact]
    public void IncidentActionStatus_Create_with_valid_fields_succeeds()
    {
        var status = IncidentActionStatus.Create(new IncidentActionStatusId(1), "PROPOSED", "Proposed", NowUtc);
        Assert.Equal("PROPOSED", status.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IncidentActionStatus_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => IncidentActionStatus.Create(new IncidentActionStatusId(1), code, "Proposed", NowUtc));
    }
}
