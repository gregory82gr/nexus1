namespace Nexus1.Instrumentation.Application;

public interface IOpenSignalQualityEventFinder
{
    Task<IReadOnlyList<OpenSignalQualityEventDto>> GetOpenByUnitCodeAsync(string unitCode, CancellationToken cancellationToken);
}
