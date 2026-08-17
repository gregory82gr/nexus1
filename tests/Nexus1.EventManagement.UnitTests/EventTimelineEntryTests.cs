using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.UnitTests;

public class EventTimelineEntryTests
{
    private static readonly DateTime EntryAtUtc = new(2026, 8, 17, 8, 10, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var entry = EventTimelineEntry.Create(
            new EventTimelineEntryId(1), operationalEventId: 100L, new EventTimelineEntryTypeId(1), EntryAtUtc,
            "Alarm acknowledged");

        Assert.Equal(new OperationalEventId(100L), entry.OperationalEventId);
        Assert.Equal("Alarm acknowledged", entry.Title);
        Assert.Null(entry.Body);
        Assert.Null(entry.EnteredByUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<ArgumentException>(() => EventTimelineEntry.Create(
            new EventTimelineEntryId(1), 100L, new EventTimelineEntryTypeId(1), EntryAtUtc, title));
    }

    [Fact]
    public void Create_with_passport_only_entered_by_user_id_sets_it_with_no_enforced_fk()
    {
        var entry = EventTimelineEntry.Create(
            new EventTimelineEntryId(1), 100L, new EventTimelineEntryTypeId(1), EntryAtUtc, "Operator note",
            body: "Confirmed via control room", enteredByUserId: 42, sourceReference: "SCADA-LOG-991");

        Assert.Equal(42, entry.EnteredByUserId);
        Assert.Equal("Confirmed via control room", entry.Body);
        Assert.Equal("SCADA-LOG-991", entry.SourceReference);
    }
}
