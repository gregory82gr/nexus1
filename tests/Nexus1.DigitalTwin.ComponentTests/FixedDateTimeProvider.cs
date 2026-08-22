using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.ComponentTests;

internal sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}
