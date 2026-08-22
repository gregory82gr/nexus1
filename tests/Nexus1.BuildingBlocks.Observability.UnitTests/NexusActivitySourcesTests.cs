using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

/// <summary>
/// Zero-discovery gate (ch.51 51-AD): a missing package, source-name typo
/// or unregistered source cannot produce a successful evidence verdict
/// merely because the application returned 202 — the source inventory
/// itself is asserted here so registration drift fails a unit test, not a
/// silent gap in ServiceDefaults' AddSource call.
/// </summary>
public sealed class NexusActivitySourcesTests
{
    [Fact]
    public void All_lists_every_static_source_name_exactly_once()
    {
        var staticSourceNames = new[]
        {
            NexusActivitySources.ReactorFleetSource.Name,
            NexusActivitySources.AlarmManagementSource.Name,
            NexusActivitySources.RootCauseSource.Name,
            NexusActivitySources.AuditSource.Name,
            NexusActivitySources.ComplianceSource.Name,
            NexusActivitySources.ReportingSource.Name,
            NexusActivitySources.MessagingSource.Name,
        };

        Assert.Equal(staticSourceNames.OrderBy(x => x), NexusActivitySources.All.OrderBy(x => x));
        Assert.Equal(staticSourceNames.Distinct().Count(), NexusActivitySources.All.Count);
    }

    [Fact]
    public void Every_source_has_an_explicit_version()
    {
        var sources = new[]
        {
            NexusActivitySources.ReactorFleetSource, NexusActivitySources.AlarmManagementSource,
            NexusActivitySources.RootCauseSource, NexusActivitySources.AuditSource,
            NexusActivitySources.ComplianceSource, NexusActivitySources.ReportingSource,
            NexusActivitySources.MessagingSource,
        };

        Assert.All(sources, s => Assert.False(string.IsNullOrWhiteSpace(s.Version)));
    }
}
