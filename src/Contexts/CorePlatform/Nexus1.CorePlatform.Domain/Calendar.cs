using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>Named schedule rules (atlas C.1.4.7): 24x7 operations, weekday maintenance, regulatory reporting calendars. Depends on TimeZone.</summary>
public sealed class Calendar : Entity<CalendarId>, IAggregateRoot
{
    private Calendar(
        CalendarId id, string code, string name, TimeZoneReferenceId timeZoneId, CalendarType calendarType,
        byte workingDaysMask, TimeOnly? workingDayStart, TimeOnly? workingDayEnd, bool is24x7,
        string? description, bool isActive, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        TimeZoneId = timeZoneId;
        CalendarType = calendarType;
        WorkingDaysMask = workingDaysMask;
        WorkingDayStart = workingDayStart;
        WorkingDayEnd = workingDayEnd;
        Is24x7 = is24x7;
        Description = description;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public TimeZoneReferenceId TimeZoneId { get; }

    public CalendarType CalendarType { get; }

    public byte WorkingDaysMask { get; }

    public TimeOnly? WorkingDayStart { get; }

    public TimeOnly? WorkingDayEnd { get; }

    public bool Is24x7 { get; }

    public string? Description { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public static Calendar Create(
        CalendarId id, string code, string name, TimeZoneReferenceId timeZoneId, CalendarType calendarType,
        DateTime createdAtUtc, byte workingDaysMask = 127, TimeOnly? workingDayStart = null,
        TimeOnly? workingDayEnd = null, bool is24x7 = false, string? description = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Calendar code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Calendar name must not be empty.", nameof(name));
        }

        if (workingDaysMask > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(workingDaysMask), workingDaysMask, "WorkingDaysMask must be between 0 and 127.");
        }

        if (!is24x7 && (workingDayStart is null || workingDayEnd is null || workingDayEnd <= workingDayStart))
        {
            throw new ArgumentException(
                "A calendar that is not 24x7 must have WorkingDayStart and WorkingDayEnd, with end after start.");
        }

        return new Calendar(
            id, code, name, timeZoneId, calendarType, workingDaysMask, workingDayStart, workingDayEnd,
            is24x7, description, isActive, createdAtUtc);
    }
}
