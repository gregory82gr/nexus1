namespace Nexus1.BuildingBlocks.Application;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
