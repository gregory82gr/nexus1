namespace Nexus1.CorePlatform.Domain;

/// <summary>Matches the atlas's CK_CorePlatform_Calendar_Type check constraint exactly (C.1.4.7).</summary>
public enum CalendarType
{
    General,
    Shift,
    Maintenance,
    Reporting,
    Regulatory,
}
