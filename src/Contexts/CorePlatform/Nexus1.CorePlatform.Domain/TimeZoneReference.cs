using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Reference table (atlas C.1.4.7, table name CorePlatform.TimeZone):
/// canonical IANA time zones. Named TimeZoneReference here, not TimeZone —
/// <see cref="System.TimeZone"/> already exists in the BCL, and with this
/// project's ImplicitUsings enabled (which brings System into scope), a
/// type literally named TimeZone in this namespace would be a genuine
/// CS0104 ambiguity, not just a style collision. EF Core maps the C# type
/// name independently of the SQL table name, so the table itself is still
/// named CorePlatform.TimeZone.
/// </summary>
public sealed class TimeZoneReference : Entity<TimeZoneReferenceId>, IAggregateRoot
{
    private TimeZoneReference(
        TimeZoneReferenceId id, string ianaName, string displayName, string? windowsName,
        short currentUtcOffsetMinutes, bool observesDst, bool isActive, int displayOrder, DateTime createdAtUtc)
        : base(id)
    {
        IanaName = ianaName;
        DisplayName = displayName;
        WindowsName = windowsName;
        CurrentUtcOffsetMinutes = currentUtcOffsetMinutes;
        ObservesDst = observesDst;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }

    public string IanaName { get; }

    public string DisplayName { get; }

    public string? WindowsName { get; }

    public short CurrentUtcOffsetMinutes { get; }

    public bool ObservesDst { get; }

    public bool IsActive { get; }

    public int DisplayOrder { get; }

    public DateTime CreatedAtUtc { get; }

    public static TimeZoneReference Create(
        TimeZoneReferenceId id, string ianaName, string displayName, short currentUtcOffsetMinutes,
        DateTime createdAtUtc, string? windowsName = null, bool observesDst = false, bool isActive = true,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(ianaName))
        {
            throw new ArgumentException("IanaName must not be empty.", nameof(ianaName));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("DisplayName must not be empty.", nameof(displayName));
        }

        if (currentUtcOffsetMinutes is < (short)-840 or > (short)840)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentUtcOffsetMinutes), currentUtcOffsetMinutes, "CurrentUtcOffsetMinutes must be between -840 and 840.");
        }

        return new TimeZoneReference(
            id, ianaName, displayName, windowsName, currentUtcOffsetMinutes, observesDst, isActive, displayOrder, createdAtUtc);
    }
}
