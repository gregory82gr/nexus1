namespace Nexus1.ReactorFleet.Domain;

/// <summary>
/// The seam AlarmManagement's in-process flood detector consumes (ADR-001-amend, ADR-003).
/// </summary>
public sealed record UnitPowerRecorded(UnitId UnitId, PowerPercent PowerPercent, DateTime RecordedAtUtc);
