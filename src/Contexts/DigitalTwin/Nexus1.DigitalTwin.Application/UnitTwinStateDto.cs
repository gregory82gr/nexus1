namespace Nexus1.DigitalTwin.Application;

/// <summary>
/// Shaped for the BFF's Plant 3D View screen (per-unit twin state) — unlike
/// ActiveTwinDto (GetActiveTwinsForFleetQuery, fleet-wide), this is scoped to
/// one unit, so UnitId is included and IsAuthoritative is surfaced (a unit
/// can have more than one active, non-deleted twin model; IsAuthoritative is
/// what the domain itself uses to say which one is the live one).
///
/// Does NOT include divergence/sync-drift data — see IActiveTwinFinder.GetActiveTwinsForUnitAsync's
/// doc comment for why that's a named gap, not an oversight.
/// </summary>
public sealed record UnitTwinStateDto(
    int UnitId,
    string UnitCode,
    string TwinCode,
    string TwinName,
    string ModelType,
    string Status,
    string Fidelity,
    bool IsAuthoritative);
