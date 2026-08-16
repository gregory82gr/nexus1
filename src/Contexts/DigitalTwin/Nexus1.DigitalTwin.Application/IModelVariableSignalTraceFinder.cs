namespace Nexus1.DigitalTwin.Application;

public interface IModelVariableSignalTraceFinder
{
    Task<IReadOnlyList<ModelVariableSignalTraceDto>> GetByTwinCodeAsync(string twinCode, CancellationToken cancellationToken);
}
