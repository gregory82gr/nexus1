using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

/// <summary>Minimal IMeterFactory for unit tests — no DI container, no OpenTelemetry SDK, just the one Meter these types need (ch.52 52-AB's "instrument unit test" pattern, in process).</summary>
internal sealed class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _meters = [];

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options);
        _meters.Add(meter);
        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _meters)
        {
            meter.Dispose();
        }
    }
}
