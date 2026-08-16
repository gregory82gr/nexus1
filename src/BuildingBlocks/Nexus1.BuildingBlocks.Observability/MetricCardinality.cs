namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Fail-closed series-budget check (ch.52 52-C/52-G), computed before any
/// traffic arrives — a compile-time-checkable worksheet, not a runtime
/// limiter. An SDK cardinality limit is secondary (52-G's "SDK GUARD IS
/// SECONDARY"): this is the design-time gate that keeps the label domains
/// themselves bounded.
/// </summary>
public static class MetricCardinality
{
    /// <summary>
    /// Upper bound on <see cref="MetricNames.MessageAttempts"/>/
    /// <see cref="MetricNames.MessageDuration"/>'s series count:
    /// 4 operations x 5 outcomes x 7 components = 140. Rounded up to a
    /// round budget rather than pinned to the exact product, so adding one
    /// more reviewed component later does not immediately require a budget
    /// change too.
    /// </summary>
    public const int MessageMetricsBudget = 256;

    /// <summary>
    /// Product of the reviewed label-value domains a metric's tags draw
    /// from. `checked` deliberately — a domain large enough to overflow
    /// `int` is itself proof the vocabulary was never reviewed.
    /// </summary>
    public static int Product(params IReadOnlyCollection<string>[] domains)
    {
        checked
        {
            var total = 1;
            foreach (var domain in domains)
            {
                total *= domain.Count;
            }

            return total;
        }
    }
}
