namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// From_Services_To_Runtime Executable Asset 29-I, adopted exactly except
/// DateTimeOffset -> DateTime (ADR-009): this project's convention
/// everywhere else is plain UTC DateTime via IDateTimeProvider, and a
/// DateTime read back from SQL Server through EF Core loses its Kind
/// (becomes Unspecified) — the implicit DateTime-to-DateTimeOffset
/// conversion then treats it as local time, silently shifting the value by
/// the machine's timezone offset. Matching the surrounding codebase's type
/// avoids that pitfall entirely rather than requiring every call site to
/// remember DateTime.SpecifyKind.
/// </summary>
public sealed record RetryBudgetDecision(bool CanRetry, int NextAttempt, string Reason);

public static class RetryBudget
{
    /// <summary>
    /// MaxRetryAttempts counts retries after the initial delivery — a policy
    /// with five retries allows at most six handler deliveries, and
    /// MaxElapsed can terminate the sequence earlier (ch.29 p.650).
    /// </summary>
    public static RetryBudgetDecision Evaluate(
        RetryPolicy policy, int currentAttempt, DateTime firstFailedAtUtc, DateTime nowUtc)
    {
        policy.Validate();

        if (currentAttempt < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentAttempt), currentAttempt, "Attempt cannot be negative.");
        }

        var next = checked(currentAttempt + 1);
        if (next > policy.MaxRetryAttempts)
        {
            return new RetryBudgetDecision(false, next, "attempt-budget-exhausted");
        }

        if (nowUtc - firstFailedAtUtc >= policy.MaxElapsed)
        {
            return new RetryBudgetDecision(false, next, "elapsed-budget-exhausted");
        }

        return new RetryBudgetDecision(true, next, "retry-permitted");
    }
}
