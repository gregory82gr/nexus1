namespace Nexus1.Organization.Application;

/// <summary>
/// Shaped for the BFF's Personnel screen. Scoped to a Department, not a
/// ReactorFleet unit — see IDepartmentRosterFinder's doc comment for why a
/// per-unit roster is not something this domain model can honestly support.
/// ApplicationUserId is exposed as-is (the raw passport int, ADR-017) —
/// Organization only knows whether a person has a linked login, never any
/// detail about that login itself; resolving it further is Security's job.
/// </summary>
public sealed record DepartmentRosterEntryDto(
    int PersonId,
    string DisplayName,
    string? PersonnelNumber,
    string? PositionTitle,
    bool? IsSafetyCriticalPosition,
    int? ApplicationUserId,
    DateOnly StartDate,
    bool IsPrimary);
