using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.ComponentTests;

internal sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}
