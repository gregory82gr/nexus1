using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

/// <summary>Design-time series-budget worksheet (ch.52 52-C/52-G) — computed and asserted before any traffic exists, not discovered after a cardinality incident.</summary>
public sealed class MetricCardinalityTests
{
    [Fact]
    public void Product_multiplies_domain_sizes()
    {
        Assert.Equal(20, MetricCardinality.Product(MetricVocabulary.Operations, ["a", "b", "c", "d", "e"]));
    }

    [Fact]
    public void Message_attempts_and_duration_series_stay_within_budget()
    {
        var product = MetricCardinality.Product(MetricVocabulary.Operations, MetricVocabulary.Outcomes, MetricVocabulary.Components);

        Assert.True(product <= MetricCardinality.MessageMetricsBudget,
            $"Operations({MetricVocabulary.Operations.Count}) x Outcomes({MetricVocabulary.Outcomes.Count}) x Components({MetricVocabulary.Components.Count}) = {product}, exceeds budget {MetricCardinality.MessageMetricsBudget}.");
    }

    [Fact]
    public void Reviewed_label_domains_never_admit_a_forbidden_locator_key()
    {
        string[] forbiddenKeys = ["nexus1.message.id", "trace.id", "span.id", "nexus1.case.id"];

        Assert.All(forbiddenKeys, key => Assert.DoesNotContain(key, MetricVocabulary.Operations));
        Assert.All(forbiddenKeys, key => Assert.DoesNotContain(key, MetricVocabulary.Outcomes));
        Assert.All(forbiddenKeys, key => Assert.DoesNotContain(key, MetricVocabulary.Components));
    }

    [Fact]
    public void Product_throws_rather_than_silently_wrapping_on_overflow()
    {
        var hugeDomain = Enumerable.Range(0, 100_000).Select(i => i.ToString()).ToArray();

        Assert.Throws<OverflowException>(() => MetricCardinality.Product(hugeDomain, hugeDomain, hugeDomain, hugeDomain, hugeDomain));
    }
}
