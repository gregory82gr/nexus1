namespace Nexus1.EventManagement.Application;

public interface IOpenIncidentActionsFinder
{
    Task<IReadOnlyList<OpenIncidentActionDto>> GetOpenIncidentActionsAsync(CancellationToken cancellationToken);
}
