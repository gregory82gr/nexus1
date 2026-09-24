using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.UnitTests;

/// <summary>
/// The engine's reach, made a test (ADR-037/ADR-040): the diagnosis and graph routes
/// resolve the incident id against IncidentRegistry, so an id that is not registered is a
/// 404. EVT-2026-0420 is DELIBERATELY not registered -- the engine cannot solve the
/// QA-escape, and its absence here is exactly what makes both routes 404 (ADR-040).
/// </summary>
public class IncidentRegistryTests
{
    [Theory]
    [InlineData("EVT-2026-0418")]
    [InlineData("EVT-2026-0419")]
    public void Registered_incidents_resolve(string incidentId)
    {
        Assert.True(IncidentRegistry.TryGet(incidentId, out var ctx));
        Assert.Equal(incidentId, ctx.IncidentId);
    }

    [Theory]
    [InlineData("EVT-2026-0420")] // the QA-escape: engine cannot solve it -> not registered -> 404
    [InlineData("EVT-9999-9999")]
    public void Unregistered_incidents_do_not_resolve(string incidentId)
    {
        Assert.False(IncidentRegistry.TryGet(incidentId, out _));
    }
}
