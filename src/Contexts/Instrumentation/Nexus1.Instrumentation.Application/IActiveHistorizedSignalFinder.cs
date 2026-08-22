namespace Nexus1.Instrumentation.Application;

public interface IActiveHistorizedSignalFinder
{
    /// <summary>Atlas C.5.8 query 1: WHERE ru.Code = @unitCode AND s.IsHistorized = 1 AND s.IsDeleted = 0, ORDER BY s.Tag.</summary>
    Task<IReadOnlyList<ActiveHistorizedSignalDto>> GetByUnitCodeAsync(string unitCode, CancellationToken cancellationToken);
}
