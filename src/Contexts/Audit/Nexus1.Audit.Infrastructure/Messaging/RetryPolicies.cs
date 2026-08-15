using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.Audit.Infrastructure.Messaging;

/// <summary>
/// Book-given for Audit (ch.29 Policy Catalogue 29-H, p.649): 8 retry
/// attempts, 60-minute elapsed budget — used verbatim. The book's catalogue
/// keys this by the queue name "audit.public-facts.v1" (ch.25's broader,
/// superseded topology); this project's actual queue is
/// "audit.root-cause-verdicts.v1" (ch.34's frozen shape, ADR-010), so the
/// PolicyId is renamed to match while keeping the book's numbers. InitialDelay/
/// MaxDelay/EqualJitterPercent are not book-given, same reasoning as
/// RootCause's rc-alarm-read-v1 (ADR-009): fast enough for a real end-to-end
/// run, not tuned for production dependency recovery.
/// </summary>
public static class RetryPolicies
{
    public static readonly RetryPolicy AuditRootCauseVerdicts = new(
        PolicyId: "audit-root-cause-verdicts-v1",
        MaxRetryAttempts: 8,
        MaxElapsed: TimeSpan.FromMinutes(60),
        InitialDelay: TimeSpan.FromSeconds(2),
        MaxDelay: TimeSpan.FromSeconds(20),
        EqualJitterPercent: 30);
}
