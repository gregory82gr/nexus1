using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.RootCause.UnitTests;

/// <summary>Mirrors From_Services_To_Runtime's own budget examples (ch.29, Budget Examples 29-J).</summary>
public sealed class RetryBudgetTests
{
    private static readonly RetryPolicy Policy = new(
        "test-policy", MaxRetryAttempts: 3, MaxElapsed: TimeSpan.FromMinutes(10),
        InitialDelay: TimeSpan.FromSeconds(1), MaxDelay: TimeSpan.FromSeconds(30), EqualJitterPercent: 20);

    private static readonly DateTime FirstFailedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    public void Retry_is_permitted_while_the_attempt_budget_remains(int currentAttempt, int expectedNextAttempt)
    {
        var decision = RetryBudget.Evaluate(Policy, currentAttempt, FirstFailedAtUtc, FirstFailedAtUtc);

        Assert.True(decision.CanRetry);
        Assert.Equal(expectedNextAttempt, decision.NextAttempt);
        Assert.Equal("retry-permitted", decision.Reason);
    }

    [Fact]
    public void Retry_is_refused_once_the_attempt_budget_is_exhausted()
    {
        var decision = RetryBudget.Evaluate(Policy, currentAttempt: 3, FirstFailedAtUtc, FirstFailedAtUtc);

        Assert.False(decision.CanRetry);
        Assert.Equal(4, decision.NextAttempt);
        Assert.Equal("attempt-budget-exhausted", decision.Reason);
    }

    [Fact]
    public void Retry_is_refused_once_the_elapsed_budget_is_exhausted_even_with_attempts_remaining()
    {
        var nowUtc = FirstFailedAtUtc.AddMinutes(10);

        var decision = RetryBudget.Evaluate(Policy, currentAttempt: 1, FirstFailedAtUtc, nowUtc);

        Assert.False(decision.CanRetry);
        Assert.Equal("elapsed-budget-exhausted", decision.Reason);
    }
}
