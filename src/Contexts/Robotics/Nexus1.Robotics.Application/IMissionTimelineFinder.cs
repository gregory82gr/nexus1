namespace Nexus1.Robotics.Application;

public interface IMissionTimelineFinder
{
    Task<IReadOnlyList<MissionTimelineEntryDto>> GetTimelineAsync(long missionId, CancellationToken cancellationToken);
}
