namespace Nexus1.ReinforcementLearning.Application;

public interface IFinalQTableEntryCountFinder
{
    Task<IReadOnlyList<FinalQTableEntryCountDto>> GetFinalQTableEntryCountsAsync(CancellationToken cancellationToken);
}
