using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.RootCause.UnitTests;

/// <summary>Mirrors From_Services_To_Runtime's own backoff assets (ch.29, Executable Assets 29-AQ/29-AR).</summary>
public sealed class RetryBackoffTests
{
    private static readonly RetryPolicy Policy = new(
        "test-policy", MaxRetryAttempts: 5, MaxElapsed: TimeSpan.FromMinutes(15),
        InitialDelay: TimeSpan.FromSeconds(2), MaxDelay: TimeSpan.FromSeconds(15), EqualJitterPercent: 30);

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 8)]
    [InlineData(4, 15)]
    [InlineData(5, 15)]
    public void ExponentialCap_doubles_then_saturates_at_MaxDelay(int nextAttempt, int expectedSeconds)
    {
        var cap = RetryBackoff.ExponentialCap(Policy, nextAttempt);

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), cap);
    }

    [Fact]
    public void EqualJitter_is_stable_for_the_same_inputs()
    {
        var messageId = Guid.NewGuid();

        var first = RetryBackoff.EqualJitter(Policy, messageId, nextAttempt: 4);
        var second = RetryBackoff.EqualJitter(Policy, messageId, nextAttempt: 4);

        Assert.Equal(first, second);
    }

    [Fact]
    public void EqualJitter_stays_within_the_floor_and_cap_bounds()
    {
        var messageId = Guid.NewGuid();
        var cap = RetryBackoff.ExponentialCap(Policy, nextAttempt: 4);
        var floor = TimeSpan.FromTicks(cap.Ticks * (100 - Policy.EqualJitterPercent) / 100);

        var delay = RetryBackoff.EqualJitter(Policy, messageId, nextAttempt: 4);

        Assert.InRange(delay, floor, cap);
    }

    [Fact]
    public void EqualJitter_differs_across_different_message_ids()
    {
        var cap = RetryBackoff.ExponentialCap(Policy, nextAttempt: 4);
        var samples = Enumerable.Range(0, 20)
            .Select(_ => RetryBackoff.EqualJitter(Policy, Guid.NewGuid(), nextAttempt: 4))
            .Distinct()
            .ToList();

        // Not every sample needs to be unique, but a spread this wide
        // collapsing to a single value would indicate the hash input isn't
        // actually varying with messageId.
        Assert.True(samples.Count > 1, "Expected jitter to vary across different message ids.");
        Assert.All(samples, delay => Assert.InRange(delay, TimeSpan.Zero, cap));
    }
}
