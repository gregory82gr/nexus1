namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// Matches the Schema Atlas's seeded AlarmState lookup codes (ADR-004). The
/// book's own example calls the initial state "Raised" — the atlas is
/// authoritative for anything persisted (CLAUDE.md §1), so this uses "Active".
/// </summary>
public enum AlarmState
{
    Active,
    Acknowledged,
    Returned,
    Cleared,
    Shelved,
    Suppressed,
}
