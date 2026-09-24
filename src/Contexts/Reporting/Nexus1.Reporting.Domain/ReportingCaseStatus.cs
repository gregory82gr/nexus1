namespace Nexus1.Reporting.Domain;

public enum ReportingCaseStatus
{
    Open,
    VerdictIssued,

    /// <summary>The source case was terminally marked Inconclusive (ADR-039) — projected from RootCauseCaseInconclusiveV1. A terminal read-model status like VerdictIssued, but carrying a reason, never a verdict.</summary>
    Inconclusive,
}
