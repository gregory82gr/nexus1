namespace Nexus1.Maintenance.Application;

public interface IWorkOrdersWithOriginFinder
{
    Task<IReadOnlyList<WorkOrderWithOriginDto>> GetWorkOrdersWithOriginAsync(CancellationToken cancellationToken);
}
