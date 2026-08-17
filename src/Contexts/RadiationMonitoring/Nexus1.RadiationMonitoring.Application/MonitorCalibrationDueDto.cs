namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Atlas C.13.5.2 query 2, verbatim: monitors whose calibration is due.</summary>
public sealed record MonitorCalibrationDueDto(string Code, string Name, string MonitorType, DateTime CalibrationDueAtUtc);
