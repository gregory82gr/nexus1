using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The fixed EVT-2026-0418 grounding fixture (ADR-032) -- the single, auditable
/// place the "resolved Figure 2.1" numbers live. Every value here is traceable
/// to <c>From Flood to Cause</c>: the causal graph and delay windows to
/// Figure 2.1 (Ch.2), the alarm counts to Figure 1.1 (Ch.1), the illustrative
/// weights and roles to the ranked-by-causal-origin panel of Figure 1.1, and
/// the trip wall-clock to the Ch.1 timeline. Recovered directly from the book
/// (pages 6 and 8, <c>pdftotext -table</c>) because the prior architects'
/// resolution was never persisted; the derivation and its one medium-confidence
/// point (the learned 4kV edge's 2s) are recorded in ADR-032.
///
/// Nothing here is invented plant data. The corpus body is the book's own
/// worked-example passage, labelled honestly as such (H-controls, ADR-032); the
/// historian carries physical onsets that fit the delay windows, distinct from
/// alarm timestamps (the book's central point -- arrival order is not causal
/// order), so it exists to prove the corroborator's window-fit logic, not to
/// reproduce the scrambled alarm clock.
///
/// Coverage is NOT stored -- it is computed by the graph walk from these alarm
/// counts, so "FV-104 explains 11 of 14" emerges from the fixture rather than
/// being asserted by it.
/// </summary>
public static class GroundingSeed
{
    // Incident identity is owned by FixedIncident (Application) -- the single
    // source of truth these seeded rows must agree with (ADR-034).
    public static string IncidentId => FixedIncident.IncidentId;

    public static int UnitId => FixedIncident.UnitId;

    /// <summary>Physical onset of the origin fault (historian anchor), chosen so the trip onset lands on the book's 17:12:14.</summary>
    public static DateTime FloodStartUtc => FixedIncident.FloodStartUtc;

    // Stable component ids for the fixture -- also the historian ChannelId for
    // each node (one channel per component in the skeleton).
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

    /// <summary>The flood: every component that raised at least one alarm (FT-7 raised none -- it was correlation-flagged and cross-checked healthy). Fourteen alarms across these nine nodes. Owned by FixedIncident; the local component-id constants above are [1..9] in the same order.</summary>
    public static IReadOnlyList<int> AlarmedComponentIds => FixedIncident.AlarmedComponentIds;

    public static IReadOnlyList<Component> Components() =>
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

    public static IReadOnlyList<Edge> Edges() =>
    [
        // Backbone (solid) -- the engineered fault-tree cascade FV-104 -> ... -> trip.
        // DelayMin == DelayMax: the console reproduces point delays, so an exact
        // window, not an invented tolerance band (Figure 2.1).
        Backbone(Fv104, Sg1Level, 36, "FT-FW-01"),
        Backbone(Sg1Level, FwPump2A, 18, "FT-FW-02"),
        Backbone(FwPump2A, Pump2ABearing, 42, "FT-FW-03"),
        Backbone(Pump2ABearing, LoopFlow, 12, "FT-TH-01"),
        Backbone(LoopFlow, CoreT, 30, "FT-TH-02"),
        Backbone(CoreT, ReactorTrip, 4, "FT-TH-03"),
        Backbone(Rcp1B, LoopFlow, 10, "FT-TH-04"),   // parallel strand into the loop oscillation
        // Learned (dashed violet) -- data-informed, subordinate, never walked as
        // backbone: the 4kV transient proposed as a minor stressor on the pump.
        new() { FromComponentId = Bus2A, ToComponentId = FwPump2A, Kind = "learned", DelayMinSeconds = 2, DelayMaxSeconds = 2, SourceRef = "learned-2026Q1" },
        // Rejected (struck-grey) -- FT-7 to loop flow, considered and ruled out;
        // kept visible, never walked, no meaningful window.
        new() { FromComponentId = Ft7, ToComponentId = LoopFlow, Kind = "rejected", DelayMinSeconds = 0, DelayMaxSeconds = 0, SourceRef = "corr-flagged" },
    ];

    /// <summary>
    /// One physical-onset historian sample per cascade channel, offset from the
    /// origin onset by the modelled propagation delays so every backbone edge on
    /// FV-104's forward path fits its window exactly. Quality 0 = good.
    /// </summary>
    public static IReadOnlyList<HistorianSample> Historian() =>
    [
        Onset(Fv104, 0),
        Onset(Sg1Level, 36),
        Onset(FwPump2A, 36 + 18),
        Onset(Pump2ABearing, 36 + 18 + 42),
        Onset(LoopFlow, 36 + 18 + 42 + 12),
        Onset(CoreT, 36 + 18 + 42 + 12 + 30),
        Onset(ReactorTrip, 36 + 18 + 42 + 12 + 30 + 4),
        Onset(Rcp1B, (36 + 18 + 42 + 12) - 10),      // 10s before loop flow -> parallel edge fits
        Onset(Bus2A, (36 + 18) - 2),                  // 2s before the pump -> learned edge fits
    ];

    /// <summary>
    /// The corpus: the book's own EVT-2026-0418 worked-example passage, labelled
    /// as exactly that. No fabricated plant procedure or vendor manual. Embedding
    /// left null -- the semantic path waits on the deferred embedding model; the
    /// lexical path (exact tag match) needs no embedding (ADR-032).
    /// </summary>
    public static IReadOnlyList<CorpusChunk> Corpus() =>
    [
        new()
        {
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
            DocId = 1,
            TrustTier = 0,
            Body = "Arrival order is not causal order: the FV-104 latency warning crosses its threshold late, well after "
                 + "the loop-flow oscillation it ultimately caused has already alarmed. The engine reconciles alarm times "
                 + "against the continuous historian, where onset is visible directly and fits the modelled delay windows.",
            SourceLabel = "From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.2 (delay windows)",
            EmbeddingJson = null,
        },
    ];

    private static Edge Backbone(int from, int to, int delaySeconds, string sourceRef) =>
        new() { FromComponentId = from, ToComponentId = to, Kind = "backbone", DelayMinSeconds = delaySeconds, DelayMaxSeconds = delaySeconds, SourceRef = sourceRef };

    private static HistorianSample Onset(int channelId, int offsetSeconds) =>
        new() { ChannelId = channelId, TimestampUtc = FloodStartUtc.AddSeconds(offsetSeconds), Value = 1.0, Quality = 0 };

    /// <summary>Idempotently seeds the full fixture into a RootCause database. Safe to call once per fresh database (component tests) or once at host startup.</summary>
    public static async Task SeedAsync(RootCauseDbContext db, CancellationToken cancellationToken = default)
    {
        if (db.Components.Any())
        {
            return;
        }

        db.Components.AddRange(Components());
        db.Edges.AddRange(Edges());
        db.HistorianSamples.AddRange(Historian());
        db.CorpusChunks.AddRange(Corpus());
        await db.SaveChangesAsync(cancellationToken);
    }
}
