namespace Nexus1.Instrumentation.Application;

/// <summary>
/// Shaped for the BFF's Reactor sub-screens (Core, Control Rods, Kinetics,
/// Neutronics, Coolant/TH, Steam Generators — six of the book's seven
/// Reactor screens; the seventh, Model Analysis, is a different real
/// grouping, see GetOpenSignalQualityEventsForUnitQuery). Instrumentation's
/// domain model has no separate entity for "core," "control rods,"
/// "kinetics," "neutronics," "coolant," or "steam generators" — those are
/// all just Signal rows distinguished by Tag/Name/CategoryCode, measured
/// identically through the same Measurement table. There is exactly one
/// real per-unit grouping here (signals + their latest reading), not six —
/// the six screen names are a UI/book concern, not a domain-model one.
/// CategoryCode is included so a client can group/filter by it if the
/// category data happens to distinguish subsystems, but that's data content,
/// not a domain concept this codebase models directly.
/// </summary>
public sealed record UnitSignalReadingDto(
    string Tag,
    string Name,
    string CategoryCode,
    double? LatestValue,
    string? LatestQualityCode,
    DateTime? LatestTimestampUtc);
