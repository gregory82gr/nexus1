using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Atlas C.13.5.2 query 4, verbatim: DoseAlert rows where AlertStatus.Code IN ('OPEN','ACKNOWLEDGED'), joined through to the person and dose value that produced them.</summary>
public sealed record GetOpenDoseAlertsQuery : IQuery<IReadOnlyList<OpenDoseAlertDto>>;
