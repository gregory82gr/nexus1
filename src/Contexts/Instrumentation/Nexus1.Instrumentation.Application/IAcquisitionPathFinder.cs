namespace Nexus1.Instrumentation.Application;

public interface IAcquisitionPathFinder
{
    Task<IReadOnlyList<AcquisitionPathDto>> GetByTagAsync(string tag, CancellationToken cancellationToken);
}
