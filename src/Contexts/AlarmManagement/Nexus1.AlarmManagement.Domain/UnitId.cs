namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// AlarmManagement's own passport reference to ReactorFleet.Unit — deliberately
/// not shared with Nexus1.ReactorFleet.Domain.UnitId (ADR-004: Domain layers
/// never reference another context's Domain project).
/// </summary>
public readonly record struct UnitId(int Value);
