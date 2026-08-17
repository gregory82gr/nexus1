namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Atlas C.13.5.2 query 3, verbatim: latest radiation reading for each monitor.</summary>
public sealed record LatestRadiationReadingDto(string MonitorCode, DateTime TimestampUtc, decimal Value, string EngineeringUnitSymbol, string Quality);
