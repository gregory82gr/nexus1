using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The seeded grounding fixtures -- the single, auditable place the "resolved
/// figure" numbers live. Two incidents (ADR-032, ADR-037):
///
/// - EVT-2026-0418 (unit 1): the feedwater cascade. Every value traceable to
///   <c>From Flood to Cause</c> Figure 2.1 (Ch.2) / Figure 1.1 (Ch.1). Coverage is
///   NOT stored -- it is computed by the graph walk from these alarm counts, so
///   "FV-104 explains 11 of 14" emerges from the fixture rather than being asserted.
/// - EVT-2026-0419 (unit 2, "SMR Module A"): the EMI measurement artefact. Values
///   from Ch.5 ("Telemetry as the Witness") / Ch.10 ("Three Case Studies"): the
///   simultaneous coupled deflections, the flat independent witnesses, and the
///   0.70/0.16/0.10 ranking. The origin is a measurement artefact confirmed by an
///   ABSENCE (the witnesses stay flat), the opposite direction from 0418.
///
/// Nothing here is invented plant data: each corpus body is the book's own worked
/// example, labelled honestly as such. Incident identity (unit, alarmed components,
/// flood onset) is owned by <see cref="IncidentRegistry"/>; the seeded rows are
/// built to agree with it, so the runnable context and the data cannot drift apart.
///
/// The two incidents are isolated by <c>UnitId</c> (each is a different unit), which
/// is what the walker, graph reader, corroborator and retriever scope on -- the
/// "one seeded incident per unit" invariant recorded in ADR-037.
/// </summary>
public static class GroundingSeed
{
    // ----- EVT-2026-0418 (unit 1) -----
    public static string IncidentId => IncidentRegistry.Evt20260418.IncidentId;

    public static int UnitId => IncidentRegistry.Evt20260418.UnitId;

    public static DateTime FloodStartUtc => IncidentRegistry.Evt20260418.FloodStartUtc;

    public static IReadOnlyList<int> AlarmedComponentIds => IncidentRegistry.Evt20260418.AlarmedComponentIds;

    // Stable component ids for the 0418 fixture -- also the historian ChannelId per node.
    public const int Fv104 = 1;        // origin -- feedwater control valve, actuator latency
    public const int Sg1Level = 2;     // steam-generator level low / low-low
    public const int FwPump2A = 3;     // feedwater pump overspeed +12%
    public const int Pump2ABearing = 4;// pump bearing temp + vibration (the loud, proximate symptom)
    public const int LoopFlow = 5;     // primary-loop 0.8 Hz oscillation
    public const int CoreT = 6;        // core temperature-difference excursion > 50C
    public const int ReactorTrip = 7;  // terminus
    public const int Rcp1B = 8;        // RCP-1B bearing wear -- parallel strand
    public const int Bus2A = 9;        // 4kV bus voltage transient -- learned contributor
    public const int Ft7 = 10;         // flow sensor FT-7 -- correlation-flagged, ruled out

    // ----- EVT-2026-0419 (unit 2) -----
    public static string IncidentId0419 => IncidentRegistry.Evt20260419.IncidentId;

    public static int UnitId0419 => IncidentRegistry.Evt20260419.UnitId;

    public static DateTime FloodStartUtc0419 => IncidentRegistry.Evt20260419.FloodStartUtc;

    // Stable component ids for the 0419 fixture (11..17), disjoint from 0418's.
    public const int Bkr2A = 11;   // EMI source -- switchgear breaker close (computed origin, weight 0.70)
    public const int Ft9 = 12;     // primary-flow transmitter -- +18% step (proximate, weight 0.16)
    public const int Lt3 = 13;     // pressuriser-level transmitter -- spike (coupled)
    public const int Pt7 = 14;     // RCS-pressure transmitter -- spike (coupled)
    public const int Rtd1 = 15;    // primary RTD T-avg -- independent witness, stays flat
    public const int Nfx1 = 16;    // neutron-flux -- independent witness, stays flat
    public const int Lof1 = 17;    // genuine loss-of-flow excursion hypothesis -- ruled out (weight 0.10)

    // ================= EVT-2026-0418 rows (unit 1) =================

    private static IReadOnlyList<Component> Components0418() =>
    [
        // Tag, Kind, HealthScore, Status, AlarmCount, IllustrativeWeight, IllustrativeRole.
        // The origin's role is deliberately left null -- the walk computes it.
        new() { ComponentId = Fv104, UnitId = UnitId, Tag = "FV-104", Kind = "valve", HealthScore = 0.41, Status = "degrading", AlarmCount = 1, IllustrativeWeight = 0.66, IllustrativeRole = null },
        new() { ComponentId = Sg1Level, UnitId = UnitId, Tag = "SG-1", Kind = "condition", HealthScore = null, Status = "degrading", AlarmCount = 2, IllustrativeWeight = null, IllustrativeRole = null },
        new() { ComponentId = FwPump2A, UnitId = UnitId, Tag = "FWP-2A", Kind = "pump", HealthScore = 0.58, Status = "degrading", AlarmCount = 1, IllustrativeWeight = null, IllustrativeRole = null },
        new() { ComponentId = Pump2ABearing, UnitId = UnitId, Tag = "PB-2A", Kind = "sensor", HealthScore = 0.33, Status = "failed", AlarmCount = 2, IllustrativeWeight = 0.20, IllustrativeRole = "proximate" },
        new() { ComponentId = LoopFlow, UnitId = UnitId, Tag = "LF-1", Kind = "condition", HealthScore = null, Status = "degrading", AlarmCount = 2, IllustrativeWeight = null, IllustrativeRole = null },
        new() { ComponentId = CoreT, UnitId = UnitId, Tag = "CT-1", Kind = "condition", HealthScore = null, Status = "degrading", AlarmCount = 2, IllustrativeWeight = null, IllustrativeRole = null },
        new() { ComponentId = ReactorTrip, UnitId = UnitId, Tag = "RT-1", Kind = "condition", HealthScore = null, Status = "failed", AlarmCount = 1, IllustrativeWeight = null, IllustrativeRole = null },
        new() { ComponentId = Rcp1B, UnitId = UnitId, Tag = "RCP-1B", Kind = "pump", HealthScore = 0.62, Status = "degrading", AlarmCount = 2, IllustrativeWeight = 0.07, IllustrativeRole = "parallel" },
        new() { ComponentId = Bus2A, UnitId = UnitId, Tag = "BUS-2A", Kind = "bus", HealthScore = null, Status = "degrading", AlarmCount = 1, IllustrativeWeight = 0.05, IllustrativeRole = "contributing" },
        new() { ComponentId = Ft7, UnitId = UnitId, Tag = "FT-7", Kind = "sensor", HealthScore = 0.94, Status = "ok", AlarmCount = 0, IllustrativeWeight = 0.02, IllustrativeRole = "ruled-out" },
    ];

    private static IReadOnlyList<Edge> Edges0418() =>
    [
        // Backbone (solid) -- the engineered fault-tree cascade FV-104 -> ... -> trip.
        // DelayMin == DelayMax: the console reproduces point delays (Figure 2.1).
        Backbone(Fv104, Sg1Level, 36, "FT-FW-01"),
        Backbone(Sg1Level, FwPump2A, 18, "FT-FW-02"),
        Backbone(FwPump2A, Pump2ABearing, 42, "FT-FW-03"),
        Backbone(Pump2ABearing, LoopFlow, 12, "FT-TH-01"),
        Backbone(LoopFlow, CoreT, 30, "FT-TH-02"),
        Backbone(CoreT, ReactorTrip, 4, "FT-TH-03"),
        Backbone(Rcp1B, LoopFlow, 10, "FT-TH-04"),   // parallel strand into the loop oscillation
        // Learned (dashed violet) -- data-informed, subordinate, never walked as backbone.
        new() { FromComponentId = Bus2A, ToComponentId = FwPump2A, Kind = "learned", DelayMinSeconds = 2, DelayMaxSeconds = 2, SourceRef = "learned-2026Q1" },
        // Rejected (struck-grey) -- FT-7 to loop flow, considered and ruled out; kept visible, never walked.
        new() { FromComponentId = Ft7, ToComponentId = LoopFlow, Kind = "rejected", DelayMinSeconds = 0, DelayMaxSeconds = 0, SourceRef = "corr-flagged" },
    ];

    private static IReadOnlyList<HistorianSample> Historian0418() =>
    [
        Onset(Fv104, FloodStartUtc, 0),
        Onset(Sg1Level, FloodStartUtc, 36),
        Onset(FwPump2A, FloodStartUtc, 36 + 18),
        Onset(Pump2ABearing, FloodStartUtc, 36 + 18 + 42),
        Onset(LoopFlow, FloodStartUtc, 36 + 18 + 42 + 12),
        Onset(CoreT, FloodStartUtc, 36 + 18 + 42 + 12 + 30),
        Onset(ReactorTrip, FloodStartUtc, 36 + 18 + 42 + 12 + 30 + 4),
        Onset(Rcp1B, FloodStartUtc, (36 + 18 + 42 + 12) - 10),      // 10s before loop flow -> parallel edge fits
        Onset(Bus2A, FloodStartUtc, (36 + 18) - 2),                  // 2s before the pump -> learned edge fits
    ];

    private static IReadOnlyList<CorpusChunk> Corpus0418() =>
    [
        new()
        {
            UnitId = UnitId,
            DocId = 1,
            TrustTier = 0,
            Body = "EVT-2026-0418 worked example: a feedwater control valve, FV-104, with rising actuator latency "
                 + "starves SG-1; the level drop forces FW Pump 2A to overspeed, loading and heating the pump bearing, "
                 + "and the thermal-hydraulic cascade runs on to a loop-flow oscillation, a core temperature excursion, "
                 + "and the reactor trip. Correlation flagged flow sensor FT-7; it cross-checked healthy and was ruled out.",
            SourceLabel = "From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.1-2",
            EmbeddingJson = null,
        },
        new()
        {
            UnitId = UnitId,
            DocId = 1,
            TrustTier = 0,
            Body = "Arrival order is not causal order: the FV-104 latency warning crosses its threshold late, well after "
                 + "the loop-flow oscillation it ultimately caused has already alarmed. The engine reconciles alarm times "
                 + "against the continuous historian, where onset is visible directly and fits the modelled delay windows.",
            SourceLabel = "From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.2 (delay windows)",
            EmbeddingJson = null,
        },
    ];

    // ================= EVT-2026-0419 rows (unit 2) =================

    private static IReadOnlyList<Component> Components0419() =>
    [
        // BKR-2A: the EMI source. Role null -- the walk computes "origin" via its
        // artefact edges (it explains the alarms on the channels it corrupts).
        new() { ComponentId = Bkr2A, UnitId = UnitId0419, Tag = "BKR-2A", Kind = "breaker", HealthScore = null, Status = "closed", AlarmCount = 0, IllustrativeWeight = 0.70, IllustrativeRole = null },
        // The three coupled channels -- the flood (ten alarms across these three).
        new() { ComponentId = Ft9, UnitId = UnitId0419, Tag = "FT-9", Kind = "sensor", HealthScore = null, Status = "disturbed", AlarmCount = 4, IllustrativeWeight = 0.16, IllustrativeRole = "proximate" },
        new() { ComponentId = Lt3, UnitId = UnitId0419, Tag = "LT-3", Kind = "sensor", HealthScore = null, Status = "disturbed", AlarmCount = 3, IllustrativeWeight = null, IllustrativeRole = null },
        new() { ComponentId = Pt7, UnitId = UnitId0419, Tag = "PT-7", Kind = "sensor", HealthScore = null, Status = "disturbed", AlarmCount = 3, IllustrativeWeight = null, IllustrativeRole = null },
        // The independent witnesses -- flat (no alarm, no historian deflection).
        new() { ComponentId = Rtd1, UnitId = UnitId0419, Tag = "RTD-1", Kind = "sensor", HealthScore = null, Status = "steady", AlarmCount = 0, IllustrativeWeight = null, IllustrativeRole = "witness" },
        new() { ComponentId = Nfx1, UnitId = UnitId0419, Tag = "NFX-1", Kind = "sensor", HealthScore = null, Status = "steady", AlarmCount = 0, IllustrativeWeight = null, IllustrativeRole = "witness" },
        // The genuine loss-of-flow excursion hypothesis -- ruled out on the strength
        // of the witnesses' silence; kept visible (its rejected edges are the witness map).
        new() { ComponentId = Lof1, UnitId = UnitId0419, Tag = "LOF-1", Kind = "condition", HealthScore = null, Status = "ruled-out", AlarmCount = 0, IllustrativeWeight = 0.10, IllustrativeRole = "ruled-out" },
    ];

    private static IReadOnlyList<Edge> Edges0419() =>
    [
        // Artefact (dashed orange) -- BKR-2A's transient couples EMI into the three
        // transmitters at the same instant. Window [0,0]: simultaneity is the tell.
        Artefact(Bkr2A, Ft9, "emi-artefact"),
        Artefact(Bkr2A, Lt3, "emi-artefact"),
        Artefact(Bkr2A, Pt7, "emi-artefact"),
        // Rejected (struck-grey) -- the corroboration a genuine excursion (LOF-1) would
        // have produced on the independent witnesses, drawn precisely because it is
        // absent. These name the channels the corroborator checks for silence.
        new() { FromComponentId = Lof1, ToComponentId = Rtd1, Kind = "rejected", DelayMinSeconds = 0, DelayMaxSeconds = 0, SourceRef = "excursion-absent-corroboration" },
        new() { FromComponentId = Lof1, ToComponentId = Nfx1, Kind = "rejected", DelayMinSeconds = 0, DelayMaxSeconds = 0, SourceRef = "excursion-absent-corroboration" },
    ];

    private static IReadOnlyList<HistorianSample> Historian0419() =>
    [
        // The breaker close and the three coupled channels all deflect at the same
        // instant (t=0) -- the simultaneity a real hydraulic event cannot produce.
        Onset(Bkr2A, FloodStartUtc0419, 0),
        Onset(Ft9, FloodStartUtc0419, 0),
        Onset(Lt3, FloodStartUtc0419, 0),
        Onset(Pt7, FloodStartUtc0419, 0),
        // FT-9 self-clears to nominal 8s later (the flow transmitter confirming the
        // transient passed). Onset is the earliest sample, so this does not change it.
        new() { ChannelId = Ft9, TimestampUtc = FloodStartUtc0419.AddSeconds(8), Value = 0.0, Quality = 0 },
        // RTD-1 and NFX-1: deliberately NO samples -- the flat witnesses. Their silence
        // is the decisive evidence and is read as an absence, not seeded as a value.
    ];

    private static IReadOnlyList<CorpusChunk> Corpus0419() =>
    [
        new()
        {
            UnitId = UnitId0419,
            DocId = 2,
            TrustTier = 0,
            Body = "EVT-2026-0419 worked example: a switchgear breaker, BKR-2A, closes and a 4kV-bus voltage transient "
                 + "couples electromagnetic interference into three unrelated instrument channels at the same instant -- "
                 + "the primary-flow transmitter FT-9 steps up eighteen percent while the pressuriser-level (LT-3) and "
                 + "RCS-pressure (PT-7) transmitters spike. The decisive evidence is an absence: the independent witnesses "
                 + "a real loss-of-flow excursion must disturb -- the primary RTD average temperature and the neutron flux -- "
                 + "stay flat, so the apparent excursion is ruled out and the origin is a measurement artefact (EMI), not a "
                 + "process event. FT-9 self-clears to nominal once the transient passes.",
            SourceLabel = "From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.5, Ch.10 (artefact)",
            EmbeddingJson = null,
        },
        new()
        {
            UnitId = UnitId0419,
            DocId = 2,
            TrustTier = 0,
            Body = "Simultaneity is the tell: three independent channels cannot step at the identical instant for a physical "
                 + "reason, because coolant and heat take time to move, but they can for an electrical one, because the noise "
                 + "reaches them together. The engine confirms the artefact by querying the independent witnesses and finding "
                 + "them silent -- the corroboration a genuine excursion would have produced, absent -- and BKR-2A's flow "
                 + "channel FT-9 confirms it by self-clearing eight seconds later.",
            SourceLabel = "From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.5 (silence as evidence)",
            EmbeddingJson = null,
        },
    ];

    // ----- helpers -----

    private static Edge Backbone(int from, int to, int delaySeconds, string sourceRef) =>
        new() { FromComponentId = from, ToComponentId = to, Kind = "backbone", DelayMinSeconds = delaySeconds, DelayMaxSeconds = delaySeconds, SourceRef = sourceRef };

    private static Edge Artefact(int from, int to, string sourceRef) =>
        new() { FromComponentId = from, ToComponentId = to, Kind = "artefact", DelayMinSeconds = 0, DelayMaxSeconds = 0, SourceRef = sourceRef };

    private static HistorianSample Onset(int channelId, DateTime floodStartUtc, int offsetSeconds) =>
        new() { ChannelId = channelId, TimestampUtc = floodStartUtc.AddSeconds(offsetSeconds), Value = 1.0, Quality = 0 };

    /// <summary>
    /// Idempotently seeds every registered incident's fixture into a RootCause
    /// database. Each incident is guarded independently by its unit (ADR-037), so a
    /// second incident seeds after the first without being skipped, and re-running is
    /// safe. Safe to call once per fresh database (component tests) or at provisioning.
    /// </summary>
    public static async Task SeedAsync(RootCauseDbContext db, CancellationToken cancellationToken = default)
    {
        await SeedIncidentAsync(db, UnitId, Components0418(), Edges0418(), Historian0418(), Corpus0418(), cancellationToken);
        await SeedIncidentAsync(db, UnitId0419, Components0419(), Edges0419(), Historian0419(), Corpus0419(), cancellationToken);
    }

    private static async Task SeedIncidentAsync(
        RootCauseDbContext db,
        int unitId,
        IReadOnlyList<Component> components,
        IReadOnlyList<Edge> edges,
        IReadOnlyList<HistorianSample> historian,
        IReadOnlyList<CorpusChunk> corpus,
        CancellationToken cancellationToken)
    {
        // Per-unit guard: skip only this incident if its components already exist,
        // so seeding EVT-2026-0419 after EVT-2026-0418 is not skipped by the other.
        if (await db.Components.AnyAsync(c => c.UnitId == unitId, cancellationToken))
        {
            return;
        }

        db.Components.AddRange(components);
        db.Edges.AddRange(edges);
        db.HistorianSamples.AddRange(historian);
        db.CorpusChunks.AddRange(corpus);
        await db.SaveChangesAsync(cancellationToken);
    }
}
