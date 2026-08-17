namespace Nexus1.EmergencyPreparedness.Application;

public interface ISiteActivePlansFinder
{
    Task<IReadOnlyList<ActiveEmergencyPlanDto>> GetActivePlansAsync(int siteId, CancellationToken cancellationToken);
}
