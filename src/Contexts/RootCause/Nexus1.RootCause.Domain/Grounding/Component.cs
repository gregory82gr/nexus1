namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// The registry corner of the grounding triangle (From Flood to Cause, App C.1)
/// -- component identity, wiring and live health. A grounding-store entity, not
/// a domain aggregate: plain int key, no invariants of its own, seeded per
/// incident (ADR-032). The engine reads it; H3 validates that every entity a
/// draft answer names exists here.
/// </summary>
public sealed class Component
{
    public int ComponentId { get; init; }

    public int UnitId { get; init; }

    /// <summary>Business tag, e.g. "FV-104".</summary>
    public required string Tag { get; init; }

    /// <summary>valve | pump | sensor | bus | condition (App C.1 Kind).</summary>
    public required string Kind { get; init; }

    /// <summary>Live health 0..1, nullable when not scored.</summary>
    public double? HealthScore { get; init; }

    /// <summary>ok | degrading | failed (App C.1 Status).</summary>
    public required string Status { get; init; }

    /// <summary>
    /// How many alarms this node raised in the flood (the console's per-node
    /// count for EVT-2026-0418). The graph walk sums this over a node's
    /// backbone-reachable set to compute coverage as "explains X of N alarms" --
    /// so FV-104 genuinely computes to 11 of 14, the book's own figure, rather
    /// than a hard-coded result. 0 for a node that raised no alarm (e.g. the
    /// ruled-out FT-7).
    /// </summary>
    public int AlarmCount { get; init; }

    /// <summary>
    /// The console's illustrative causal weight for this fixed incident
    /// (FV-104 = 0.66, ...). Illustrative, derived from edge weights -- NOT a
    /// probability identified from plant data (the book's own repeated
    /// caveat). The walk carries it onto each candidate unchanged; only
    /// coverage and the origin verdict are computed. In a general multi-incident
    /// system this would be a per-run input, not on the registry -- a
    /// deliberate skeleton simplification (ADR-032).
    /// </summary>
    public double? IllustrativeWeight { get; init; }

    /// <summary>Console's role label for this fixed incident (proximate | parallel | contributing | ruled-out); the origin is computed, not read from here.</summary>
    public string? IllustrativeRole { get; init; }
}
