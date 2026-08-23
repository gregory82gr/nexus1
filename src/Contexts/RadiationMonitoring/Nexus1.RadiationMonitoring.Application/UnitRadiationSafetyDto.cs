namespace Nexus1.RadiationMonitoring.Application;

/// <summary>
/// Shaped for the BFF's Radiation &amp; Safety screen (per unit). Does NOT
/// include personnel dose data (DoseAlert/PersonDoseReading/Dosimeter) — a
/// named gap, not an oversight: dose in this domain model is tracked per
/// PERSON (via PersonDosimeterAssignment), never per unit. There is no
/// "unit dose" concept to project here; only ambient monitor readings and
/// zone classification are genuinely unit-scoped in this domain model. See
/// ILatestReadingPerMonitorFinder.GetLatestReadingsForUnitAsync's doc
/// comment for the full explanation.
/// </summary>
public sealed record UnitRadiationSafetyDto(
    int UnitId,
    IReadOnlyList<UnitRadiationMonitorReadingDto> Monitors,
    IReadOnlyList<UnitRadiationZoneDto> Zones);
