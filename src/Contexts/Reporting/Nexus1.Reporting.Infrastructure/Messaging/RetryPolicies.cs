using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.Reporting.Infrastructure.Messaging;

/// <summary>
/// Book-given for Reporting (ch.29 Policy Catalogue 29-H, p.649): 6 retry
/// attempts, 45-minute elapsed budget — used verbatim. Unlike Audit's and
/// Compliance's policies, no renaming was needed here: the catalogue's key
/// "reporting.integration-events.v1" already matches ch.35's actual frozen
/// queue name (ADR-012) — ch.25's illustrative topology and ch.34/35's
/// frozen shape happen to agree for this one queue. InitialDelay/MaxDelay/
/// EqualJitterPercent are not book-given, same reasoning as every other
/// policy in this project (ADR-009).
/// </summary>
public static class RetryPolicies
{
    public static readonly RetryPolicy ReportProject = new(
        PolicyId: "report-project-v1",
        MaxRetryAttempts: 6,
        MaxElapsed: TimeSpan.FromMinutes(45),
        InitialDelay: TimeSpan.FromSeconds(2),
        MaxDelay: TimeSpan.FromSeconds(20),
        EqualJitterPercent: 30);
}
