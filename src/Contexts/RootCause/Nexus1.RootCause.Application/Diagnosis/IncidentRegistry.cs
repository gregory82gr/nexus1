namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// The small, closed set of seeded incidents the diagnosis engine serves, keyed by
/// incident id (ADR-037). Replaces the single hard-coded <c>FixedIncident</c>
/// identity: both route handlers look an id up here (unknown id -> 404), and the
/// seed builds its rows to agree with these contexts. Each entry is the single
/// source of truth for one incident's identity -- unit, alarmed components, flood
/// onset, retrieval query text, and corpus version -- so the runnable context and
/// the seeded rows can never drift apart.
///
/// The set is deliberately closed and tiny: EVT-2026-0420 (the QA-escape) is NOT
/// here, because it is unbuilt (it needs the deferred abstention-capable
/// AnalysisStatus third state), so it correctly 404s -- absence here is the refusal.
/// </summary>
public static class IncidentRegistry
{
    /// <summary>EVT-2026-0418 -- the feedwater cascade (ADR-032). Unit 1.</summary>
    public static readonly IncidentContext Evt20260418 = new(
        IncidentId: "EVT-2026-0418",
        UnitId: 1,
        AlarmedComponentIds: [1, 2, 3, 4, 5, 6, 7, 8, 9], // FV-104 .. 4kV bus; FT-7 (10) raised none
        FloodStartUtc: new DateTime(2026, 4, 18, 17, 9, 52, DateTimeKind.Utc),
        QueryText: "feedwater control valve actuator latency cascade root cause",
        CorpusVersion: "evt-2026-0418-book-worked-example-v1");

    /// <summary>EVT-2026-0419 -- the EMI measurement artefact on SMR Module A (ADR-037). Unit 2.</summary>
    public static readonly IncidentContext Evt20260419 = new(
        IncidentId: "EVT-2026-0419",
        UnitId: 2,
        AlarmedComponentIds: [12, 13, 14], // FT-9, LT-3, PT-7 -- the three coupled channels (ten alarms)
        FloodStartUtc: new DateTime(2026, 4, 19, 17, 42, 10, DateTimeKind.Utc), // BKR-2A close
        QueryText: "electromagnetic interference switchgear breaker measurement artefact flat independent witnesses",
        CorpusVersion: "evt-2026-0419-book-worked-example-v1");

    private static readonly IReadOnlyDictionary<string, IncidentContext> ById =
        new[] { Evt20260418, Evt20260419 }.ToDictionary(c => c.IncidentId, StringComparer.OrdinalIgnoreCase);

    /// <summary>Every seeded incident (used by the seed to build all incidents' rows).</summary>
    public static IReadOnlyCollection<IncidentContext> All => (IReadOnlyCollection<IncidentContext>)ById.Values;

    /// <summary>Resolve an incident id to its context. False (not an exception) for an unknown id -> the handler maps that to 404.</summary>
    public static bool TryGet(string incidentId, out IncidentContext context) =>
        ById.TryGetValue(incidentId, out context!);
}
