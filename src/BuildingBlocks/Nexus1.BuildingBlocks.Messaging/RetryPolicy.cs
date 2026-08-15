namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// From_Services_To_Runtime Executable Asset 29-G, adopted exactly (ADR-009).
/// PolicyId identity travels with every ticket/poison record so a later
/// configuration change never silently rewrites an already-scheduled
/// decision (book's own stated rule, ch.29 p.649).
/// </summary>
public sealed record RetryPolicy(
    string PolicyId,
    int MaxRetryAttempts,
    TimeSpan MaxElapsed,
    TimeSpan InitialDelay,
    TimeSpan MaxDelay,
    int EqualJitterPercent)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(PolicyId))
        {
            throw new InvalidRetryPolicyException("policy-id");
        }

        if (MaxRetryAttempts is < 0 or > 20)
        {
            throw new InvalidRetryPolicyException("attempt-budget");
        }

        if (MaxElapsed <= TimeSpan.Zero)
        {
            throw new InvalidRetryPolicyException("elapsed-budget");
        }

        if (InitialDelay <= TimeSpan.Zero || InitialDelay > MaxDelay)
        {
            throw new InvalidRetryPolicyException("delay-range");
        }

        if (MaxDelay > MaxElapsed)
        {
            throw new InvalidRetryPolicyException("delay-exceeds-horizon");
        }

        if (EqualJitterPercent is < 0 or > 50)
        {
            throw new InvalidRetryPolicyException("jitter-range");
        }
    }
}

public sealed class InvalidRetryPolicyException(string code) : Exception($"Invalid retry policy: {code}");
