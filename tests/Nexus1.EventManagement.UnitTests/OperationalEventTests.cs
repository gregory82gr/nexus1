using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.UnitTests;

public class OperationalEventTests
{
    private static readonly DateTime DetectedAtUtc = new(2026, 8, 17, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ReportedAtUtc = new(2026, 8, 17, 8, 5, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_defaults_is_drill_false()
    {
        var operationalEvent = OperationalEvent.Create(
            new OperationalEventId(1), unitId: 1, new EventTypeId(1), new EventStatusId(1), new EventSeverityId(1),
            new EventSourceTypeId(1), "EVT-2026-0418", "Feedwater flow deviation", DetectedAtUtc, ReportedAtUtc);

        Assert.Equal("EVT-2026-0418", operationalEvent.EventCode);
        Assert.False(operationalEvent.IsDrill);
        Assert.Null(operationalEvent.PlantId);
        Assert.Null(operationalEvent.ClosedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_event_code_throws(string eventCode)
    {
        Assert.Throws<ArgumentException>(() => OperationalEvent.Create(
            new OperationalEventId(1), 1, new EventTypeId(1), new EventStatusId(1), new EventSeverityId(1),
            new EventSourceTypeId(1), eventCode, "Feedwater flow deviation", DetectedAtUtc, ReportedAtUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<ArgumentException>(() => OperationalEvent.Create(
            new OperationalEventId(1), 1, new EventTypeId(1), new EventStatusId(1), new EventSeverityId(1),
            new EventSourceTypeId(1), "EVT-2026-0418", title, DetectedAtUtc, ReportedAtUtc));
    }

    [Fact]
    public void Create_with_is_drill_true_sets_it()
    {
        var operationalEvent = OperationalEvent.Create(
            new OperationalEventId(1), 1, new EventTypeId(1), new EventStatusId(1), new EventSeverityId(1),
            new EventSourceTypeId(1), "EVT-2026-0419", "Quarterly evacuation drill", DetectedAtUtc, ReportedAtUtc,
            isDrill: true);

        Assert.True(operationalEvent.IsDrill);
    }

    [Fact]
    public void Create_with_passport_only_ids_sets_them_with_no_enforced_fk()
    {
        var operationalEvent = OperationalEvent.Create(
            new OperationalEventId(1), 1, new EventTypeId(1), new EventStatusId(1), new EventSeverityId(1),
            new EventSourceTypeId(1), "EVT-2026-0420", "Feedwater flow deviation", DetectedAtUtc, ReportedAtUtc,
            plantId: 3, reportedByUserId: 10, ownerUserId: 11, owningPersonId: 12);

        Assert.Equal(3, operationalEvent.PlantId);
        Assert.Equal(10, operationalEvent.ReportedByUserId);
        Assert.Equal(11, operationalEvent.OwnerUserId);
        Assert.Equal(12, operationalEvent.OwningPersonId);
    }
}
