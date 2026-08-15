using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Book-given for AlarmRead (ch.29 Policy Catalogue 29-H, p.649):
/// PolicyId "rc-alarm-read-v1", 5 retry attempts, 15-minute elapsed budget —
/// used verbatim, already recorded in ADR-008. InitialDelay/MaxDelay/
/// EqualJitterPercent are NOT book-given (the catalogue only states
/// attempt/elapsed budgets); chosen here for demonstrator practicality —
/// fast enough that a real retry-to-poison run completes in well under a
/// minute of wall-clock time rather than tuned for a real dependency's
/// recovery characteristics (ADR-009). Worst-case cumulative wait across all
/// 5 retries is ~44s, comfortably inside the 15-minute book-given budget.
/// </summary>
public static class RetryPolicies
{
    public static readonly RetryPolicy AlarmRead = new(
        PolicyId: "rc-alarm-read-v1",
        MaxRetryAttempts: 5,
        MaxElapsed: TimeSpan.FromMinutes(15),
        InitialDelay: TimeSpan.FromSeconds(2),
        MaxDelay: TimeSpan.FromSeconds(15),
        EqualJitterPercent: 30);
}
