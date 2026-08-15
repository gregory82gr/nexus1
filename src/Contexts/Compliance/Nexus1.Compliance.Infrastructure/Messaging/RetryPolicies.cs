using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.Compliance.Infrastructure.Messaging;

/// <summary>
/// Book-given for Compliance (ch.29 Policy Catalogue 29-H, p.649): 4 retry
/// attempts, 30-minute elapsed budget — used verbatim. The book's catalogue
/// keys this by the queue name "compliance.verdict-events.v1" (ch.25's
/// broader, superseded topology); this project's actual queue is
/// "compliance.root-cause-verdicts.v1" (ch.34's frozen shape, ADR-011), so
/// the PolicyId is renamed to match while keeping the book's numbers.
/// InitialDelay/MaxDelay/EqualJitterPercent are not book-given, same
/// reasoning as Audit's/RootCause's policies (ADR-009/ADR-010).
/// </summary>
public static class RetryPolicies
{
    public static readonly RetryPolicy ComplianceRootCauseVerdicts = new(
        PolicyId: "compliance-root-cause-verdicts-v1",
        MaxRetryAttempts: 4,
        MaxElapsed: TimeSpan.FromMinutes(30),
        InitialDelay: TimeSpan.FromSeconds(2),
        MaxDelay: TimeSpan.FromSeconds(20),
        EqualJitterPercent: 30);
}
