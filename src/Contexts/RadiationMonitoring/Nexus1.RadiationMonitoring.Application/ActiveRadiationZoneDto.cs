namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Atlas C.13.5.2 query 1, verbatim: active radiation zones with their unit and classification.</summary>
public sealed record ActiveRadiationZoneDto(string Code, string Name, string? UnitCode, string Classification, string Status);
