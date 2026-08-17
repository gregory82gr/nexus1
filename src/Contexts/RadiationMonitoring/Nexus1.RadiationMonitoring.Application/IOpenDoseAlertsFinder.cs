namespace Nexus1.RadiationMonitoring.Application;

public interface IOpenDoseAlertsFinder
{
    Task<IReadOnlyList<OpenDoseAlertDto>> GetOpenDoseAlertsAsync(CancellationToken cancellationToken);
}
