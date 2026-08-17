namespace Nexus1.Robotics.Application;

public interface IBlockingReadinessFailuresFinder
{
    Task<IReadOnlyList<ReadinessFailureDto>> GetBlockingFailuresAsync(long missionId, CancellationToken cancellationToken);
}
