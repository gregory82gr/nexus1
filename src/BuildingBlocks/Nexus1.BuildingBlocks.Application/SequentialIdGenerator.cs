namespace Nexus1.BuildingBlocks.Application;

/// <summary>
/// Phase-1 placeholder (single-process ModularRuntime host, no distributed
/// id concerns yet): a per-process monotonic counter seeded from the clock.
/// Not safe across multiple process instances — revisit with a real
/// distributed-safe strategy (DB sequence, Snowflake id) if/when this
/// project runs more than one instance of a host.
/// </summary>
public sealed class SequentialIdGenerator : IIdGenerator
{
    private long _lastLong = DateTime.UtcNow.Ticks;
    private int _lastInt = (int)(DateTime.UtcNow.Ticks % int.MaxValue);

    public long NextLong() => Interlocked.Increment(ref _lastLong);

    public int NextInt() => Interlocked.Increment(ref _lastInt);
}
