namespace Nexus1.AlarmManagement.Domain;

/// <summary>Matches the Schema Atlas's seeded AlarmFloodStatus lookup codes.</summary>
public enum AlarmFloodStatus
{
    Detected,
    Open,
    Analyzed,
    HandedOff,
    Closed,
    Archived,
}
