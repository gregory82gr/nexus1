using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// The single source of truth for the EVT-2026-0420 QA-escape case (ADR-040, ADR-041) —
/// the one fixed incident the engine deliberately does NOT serve (it is not in
/// <see cref="IncidentRegistry"/>, so both engine routes 404). Instead it exists as a
/// provenance-originated human RootCauseAnalysis recorded Inconclusive.
///
/// Lives here, next to IncidentRegistry, so dev provisioning and the H9 evaluation harness
/// build the case through the SAME code and constants — the harness then asserts the actual
/// provisioned content rather than a re-typed copy (ADR-041).
/// </summary>
public static class Evt20260420QaEscape
{
    public const string IncidentId = "EVT-2026-0420";

    /// <summary>Fixed AnalysisId, so provisioning is idempotent.</summary>
    public const long AnalysisId = 20260420L;

    /// <summary>Filed under unit 1 (the console's default view) so it is visible in the investigation history.</summary>
    public const int UnitId = 1;

    public const string OpenedBy = "provenance.audit";

    /// <summary>The non-conforming control rod (From Flood to Cause, Ch.10).</summary>
    public const string ControlRodTag = "CR-7";

    /// <summary>The mis-dispositioned quality waiver (From Flood to Cause, Ch.10).</summary>
    public const string WaiverTag = "WV-318";

    public static readonly DateTime OpenedAtUtc = new(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc);

    public const string Reason =
        "EVT-2026-0420 (QA escape): a non-conforming control rod, " + ControlRodTag + ", entered service through a "
        + "mis-dispositioned quality waiver, " + WaiverTag + ". It raised no alarm and left no telemetry signature for "
        + "months, so the flood/graph/witness engine has no reach here. It was exposed only by an out-of-band "
        + "provenance audit that traced the waiver to a part that should never have been installed -- not by this "
        + "system's automated diagnosis.";

    /// <summary>
    /// Builds the 0420 case: a provenance-originated RootCauseAnalysis (no flood) terminally
    /// marked Inconclusive with the narrative reason — never a synthesized verdict (ADR-040).
    /// </summary>
    public static RootCauseAnalysis BuildCase()
    {
        var analysis = RootCauseAnalysis.OpenForProvenance(
            new RootCauseAnalysisId(AnalysisId), new UnitId(UnitId), OpenedBy, OpenedAtUtc);
        analysis.MarkInconclusive(Reason, OpenedBy, OpenedAtUtc);
        return analysis;
    }
}
