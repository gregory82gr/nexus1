namespace Nexus1.ReinforcementLearning.Application;

public interface IPolicyGridFinder
{
    Task<IReadOnlyList<PolicyGridEntryDto>> GetPolicyGridAsync(int policyId, CancellationToken cancellationToken);
}
