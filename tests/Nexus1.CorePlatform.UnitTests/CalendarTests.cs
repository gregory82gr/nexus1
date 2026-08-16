using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class CalendarTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_24x7_without_working_hours_succeeds()
    {
        var calendar = Calendar.Create(
            new CalendarId(1), "OPS-24X7", "Operations 24x7", new TimeZoneReferenceId(1), CalendarType.Shift, NowUtc,
            is24x7: true);

        Assert.True(calendar.Is24x7);
        Assert.Null(calendar.WorkingDayStart);
    }

    [Fact]
    public void Create_non_24x7_without_working_hours_throws()
    {
        Assert.Throws<ArgumentException>(() => Calendar.Create(
            new CalendarId(1), "MAINT-WEEKDAY", "Maintenance Weekday", new TimeZoneReferenceId(1),
            CalendarType.Maintenance, NowUtc));
    }

    [Fact]
    public void Create_with_end_before_start_throws()
    {
        Assert.Throws<ArgumentException>(() => Calendar.Create(
            new CalendarId(1), "MAINT-WEEKDAY", "Maintenance Weekday", new TimeZoneReferenceId(1),
            CalendarType.Maintenance, NowUtc,
            workingDayStart: new TimeOnly(17, 0), workingDayEnd: new TimeOnly(8, 0)));
    }

    [Fact]
    public void Create_non_24x7_with_valid_working_hours_succeeds()
    {
        var calendar = Calendar.Create(
            new CalendarId(1), "MAINT-WEEKDAY", "Maintenance Weekday", new TimeZoneReferenceId(1),
            CalendarType.Maintenance, NowUtc,
            workingDayStart: new TimeOnly(8, 0), workingDayEnd: new TimeOnly(17, 0));

        Assert.Equal(new TimeOnly(8, 0), calendar.WorkingDayStart);
        Assert.Equal(new TimeOnly(17, 0), calendar.WorkingDayEnd);
    }

    [Fact]
    public void Create_with_working_days_mask_over_127_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Calendar.Create(
            new CalendarId(1), "OPS-24X7", "Operations 24x7", new TimeZoneReferenceId(1), CalendarType.Shift, NowUtc,
            workingDaysMask: 200, is24x7: true));
    }
}
