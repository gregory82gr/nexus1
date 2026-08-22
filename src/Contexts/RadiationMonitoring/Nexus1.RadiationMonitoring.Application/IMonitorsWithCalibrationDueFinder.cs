namespace Nexus1.RadiationMonitoring.Application;

public interface IMonitorsWithCalibrationDueFinder
{
    Task<IReadOnlyList<MonitorCalibrationDueDto>> GetMonitorsWithCalibrationDueAsync(CancellationToken cancellationToken);
}
