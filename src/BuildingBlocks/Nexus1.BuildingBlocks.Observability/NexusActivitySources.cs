using System.Diagnostics;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// One static ActivitySource per context plus a shared Messaging source for
/// the cross-cutting publish/consume spans that live in
/// Nexus1.BuildingBlocks.Messaging itself (ch.51 Executable Asset 51-A).
/// StartActivity may return null when no listener is recording — callers
/// must remain correct in that state (ch.51 ".NET BEHAVIOR").
/// </summary>
public static class NexusActivitySources
{
    public const string ReactorFleet = "Nexus1.ReactorFleet";
    public const string AlarmManagement = "Nexus1.AlarmManagement";
    public const string RootCause = "Nexus1.RootCauseAnalysis";
    public const string Audit = "Nexus1.Audit";
    public const string Compliance = "Nexus1.Compliance";
    public const string Reporting = "Nexus1.Reporting";
    public const string Messaging = "Nexus1.Messaging";

    public static readonly ActivitySource ReactorFleetSource = new(ReactorFleet, "1.0.0");
    public static readonly ActivitySource AlarmManagementSource = new(AlarmManagement, "1.0.0");
    public static readonly ActivitySource RootCauseSource = new(RootCause, "1.0.0");
    public static readonly ActivitySource AuditSource = new(Audit, "1.0.0");
    public static readonly ActivitySource ComplianceSource = new(Compliance, "1.0.0");
    public static readonly ActivitySource ReportingSource = new(Reporting, "1.0.0");
    public static readonly ActivitySource MessagingSource = new(Messaging, "1.0.0");

    /// <summary>Every source name the OTel SDK registration (ServiceDefaults) must AddSource — a zero-registration gap is a test failure, not a warning (ch.51 51-AD).</summary>
    public static readonly IReadOnlyList<string> All =
    [
        ReactorFleet, AlarmManagement, RootCause, Audit, Compliance, Reporting, Messaging
    ];
}
