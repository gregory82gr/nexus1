using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Atlas C.13.5.2 query 2, verbatim: monitors where CalibrationDueAtUtc IS NOT NULL AND CalibrationDueAtUtc &lt;= now, joined to MonitorType.</summary>
public sealed record GetMonitorsWithCalibrationDueQuery : IQuery<IReadOnlyList<MonitorCalibrationDueDto>>;
