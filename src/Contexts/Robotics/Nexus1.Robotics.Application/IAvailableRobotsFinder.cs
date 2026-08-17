namespace Nexus1.Robotics.Application;

public interface IAvailableRobotsFinder
{
    Task<IReadOnlyList<AvailableRobotDto>> GetAvailableRobotsAsync(CancellationToken cancellationToken);
}
