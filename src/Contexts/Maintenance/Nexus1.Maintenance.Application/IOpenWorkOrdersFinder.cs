namespace Nexus1.Maintenance.Application;

public interface IOpenWorkOrdersFinder
{
    Task<IReadOnlyList<OpenWorkOrderDto>> GetOpenWorkOrdersAsync(CancellationToken cancellationToken);
}
