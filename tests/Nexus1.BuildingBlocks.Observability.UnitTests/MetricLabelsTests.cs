using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

public sealed class MetricLabelsTests
{
    [Fact]
    public void ToTagList_projects_exactly_the_three_bounded_keys_when_no_error_type()
    {
        var labels = new MetricLabels("process", "COMMITTED", NexusActivitySources.RootCause);

        var tags = labels.ToTagList();

        Assert.Equal(3, tags.Count);
        Assert.Contains(tags, t => t.Key == "nexus1.operation" && (string?)t.Value == "process");
        Assert.Contains(tags, t => t.Key == "nexus1.outcome" && (string?)t.Value == "COMMITTED");
        Assert.Contains(tags, t => t.Key == "nexus1.component" && (string?)t.Value == NexusActivitySources.RootCause);
    }

    [Fact]
    public void ToTagList_adds_error_type_only_when_present()
    {
        var labels = new MetricLabels("process", "FAILED", NexusActivitySources.RootCause, "contract_invalid");

        var tags = labels.ToTagList();

        Assert.Equal(4, tags.Count);
        Assert.Contains(tags, t => t.Key == "error.type" && (string?)t.Value == "contract_invalid");
    }

    [Theory]
    [InlineData("publish", "COMMITTED", "Nexus1.RootCauseAnalysis")]
    [InlineData("process", "FAILED", "Nexus1.Messaging")]
    public void TryFor_admits_reviewed_combinations(string operation, string outcome, string component)
    {
        var admitted = MetricLabelPolicy.TryFor(operation, outcome, component, out var labels);

        Assert.True(admitted);
        Assert.Equal(operation, labels.Operation);
        Assert.Equal(outcome, labels.Outcome);
        Assert.Equal(component, labels.Component);
    }

    [Theory]
    [InlineData("unreviewed-operation", "COMMITTED", "Nexus1.RootCauseAnalysis")]
    [InlineData("publish", "UNREVIEWED_OUTCOME", "Nexus1.RootCauseAnalysis")]
    [InlineData("publish", "COMMITTED", "unreviewed-component")]
    public void TryFor_rejects_any_out_of_vocabulary_value_rather_than_admitting_a_new_series(
        string operation, string outcome, string component)
    {
        var admitted = MetricLabelPolicy.TryFor(operation, outcome, component, out _);

        Assert.False(admitted);
    }

    [Fact]
    public void TryFor_with_errorType_never_needs_to_validate_it_since_it_is_machine_classified()
    {
        var admitted = MetricLabelPolicy.TryFor("process", "FAILED", NexusActivitySources.RootCause, "dependency_unavailable", out var labels);

        Assert.True(admitted);
        Assert.Equal("dependency_unavailable", labels.ErrorType);
    }
}
