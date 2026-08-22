namespace Nexus1.ReinforcementLearning.Application;

public interface IPolicyEntryCountFinder
{
    Task<IReadOnlyList<PolicyEntryCountDto>> GetPolicyEntryCountsAsync(CancellationToken cancellationToken);
}
