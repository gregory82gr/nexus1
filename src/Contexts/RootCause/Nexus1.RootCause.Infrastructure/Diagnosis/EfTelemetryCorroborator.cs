using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The telemetry witness of Chapter 5 (ADR-032, extended in ADR-037). Onset is read
/// from the continuous record -- the earliest good-quality sample per channel
/// (ChannelId == ComponentId in the skeleton) -- not from alarm timestamps, because
/// arrival order is not causal order. Everything is scoped to the incident's unit.
///
/// Two modes, selected from the data (no interface change):
///
/// - BACKBONE mode (0418, the cascade): for every backbone edge on the origin's
///   forward path, the observed onset gap must fall inside [DelayMin, DelayMax]; one
///   edge whose downstream onset is missing or out of window fails corroboration and
///   the pipeline abstains rather than guess. Movement, in the right order and time,
///   is the evidence.
///
/// - ARTEFACT mode (0419, the EMI artefact): selected when the origin has outgoing
///   artefact edges. The coupled channels (artefact-edge targets) must deflect
///   near-simultaneously (each onset gap fits the artefact edge's [0,0] window -- a
///   simultaneity a real hydraulic event cannot produce), AND the independent
///   witnesses (the targets of the rejected edges) must stay FLAT. Here the ABSENCE
///   of a deflection on the witnesses is the confirmation -- the exact inverse of
///   backbone mode's "missing data fails". A witness that moved means a real
///   excursion, not an artefact, and fails.
/// </summary>
public sealed class EfTelemetryCorroborator(RootCauseDbContext db) : ITelemetryCorroborator
{
    public async Task<Corroboration> CheckAsync(IncidentContext ctx, int originComponentId, CancellationToken cancellationToken)
    {
        // Scope to the incident's unit (ADR-037): only this incident's components,
        // edges and historian -- never another incident's rows.
        var componentIds = (await db.Components
            .Where(c => c.UnitId == ctx.UnitId)
            .Select(c => c.ComponentId)
            .ToListAsync(cancellationToken)).ToHashSet();

        var edges = await db.Edges
            .Where(e => componentIds.Contains(e.FromComponentId) && componentIds.Contains(e.ToComponentId))
            .Select(e => new EdgeRow(e.FromComponentId, e.ToComponentId, e.Kind, e.DelayMinSeconds, e.DelayMaxSeconds))
            .ToListAsync(cancellationToken);

        // Onset per channel: earliest good-quality sample. A witness that never
        // deflected has no row here -- that absence is meaningful in artefact mode.
        var onsets = await db.HistorianSamples
            .Where(s => s.Quality == 0 && componentIds.Contains(s.ChannelId))
            .GroupBy(s => s.ChannelId)
            .Select(g => new { ChannelId = g.Key, Onset = g.Min(s => s.TimestampUtc) })
            .ToDictionaryAsync(x => x.ChannelId, x => x.Onset, cancellationToken);

        var artefactEdges = edges.Where(e => e.Kind == "artefact" && e.FromComponentId == originComponentId).ToList();
        return artefactEdges.Count > 0
            ? CheckArtefact(artefactEdges, edges, onsets)
            : CheckBackbone(originComponentId, edges, onsets);
    }

    /// <summary>
    /// Artefact mode: coupled channels deflect together and the independent witnesses
    /// stay flat. Absence of witness movement is confirmation (the inverse of
    /// backbone mode).
    /// </summary>
    private static Corroboration CheckArtefact(
        IReadOnlyList<EdgeRow> artefactEdges,
        IReadOnlyList<EdgeRow> edges,
        IReadOnlyDictionary<int, DateTime> onsets)
    {
        // 1. The coupled channels must have deflected, near-simultaneously with the source.
        foreach (var edge in artefactEdges)
        {
            if (!onsets.TryGetValue(edge.FromComponentId, out var sourceOnset))
            {
                return new Corroboration(false, $"missing historian data for artefact source {edge.FromComponentId}");
            }

            if (!onsets.TryGetValue(edge.ToComponentId, out var channelOnset))
            {
                return new Corroboration(false, $"coupled channel {edge.ToComponentId} shows no deflection -- the artefact story needs the coupled channels to move");
            }

            var gapSeconds = Math.Abs((channelOnset - sourceOnset).TotalSeconds);
            if (gapSeconds < edge.DelayMinSeconds || gapSeconds > edge.DelayMaxSeconds)
            {
                return new Corroboration(
                    false,
                    $"coupled channel {edge.ToComponentId} onset gap {gapSeconds:0.#}s outside the simultaneity window [{edge.DelayMinSeconds},{edge.DelayMaxSeconds}]s");
            }
        }

        // 2. The independent witnesses (rejected-edge targets) must be flat -- absence
        //    of a deflection is the decisive confirmation. A witness that moved means
        //    a real excursion, so this is NOT an artefact.
        var witnessIds = edges.Where(e => e.Kind == "rejected").Select(e => e.ToComponentId).Distinct().ToList();
        if (witnessIds.Count == 0)
        {
            return new Corroboration(false, "artefact hypothesis has no witness (rejected) edges to test for silence");
        }

        foreach (var witnessId in witnessIds)
        {
            if (onsets.ContainsKey(witnessId))
            {
                return new Corroboration(false, $"witness {witnessId} deflected -- an independent channel moved, so this is a real excursion, not an artefact");
            }
        }

        return new Corroboration(
            true,
            $"{artefactEdges.Count} coupled channel(s) deflected together and {witnessIds.Count} independent witness(es) stayed flat");
    }

    /// <summary>
    /// Backbone mode: walk the origin's forward backbone path; every edge's onset gap
    /// must fit its delay window against the historian. Missing or out-of-window data
    /// fails (the pipeline then abstains).
    /// </summary>
    private static Corroboration CheckBackbone(
        int originComponentId,
        IReadOnlyList<EdgeRow> edges,
        IReadOnlyDictionary<int, DateTime> onsets)
    {
        var adjacency = edges
            .Where(e => e.Kind == "backbone")
            .GroupBy(e => e.FromComponentId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var seen = new HashSet<int> { originComponentId };
        var stack = new Stack<int>();
        stack.Push(originComponentId);

        var edgesChecked = 0;

        while (stack.Count > 0)
        {
            var from = stack.Pop();
            if (!adjacency.TryGetValue(from, out var outgoing))
            {
                continue;
            }

            foreach (var edge in outgoing)
            {
                if (!onsets.TryGetValue(edge.FromComponentId, out var fromOnset))
                {
                    return new Corroboration(false, $"missing historian data for component {edge.FromComponentId}");
                }

                if (!onsets.TryGetValue(edge.ToComponentId, out var toOnset))
                {
                    return new Corroboration(false, $"missing historian data for component {edge.ToComponentId}");
                }

                var gapSeconds = (toOnset - fromOnset).TotalSeconds;
                if (gapSeconds < edge.DelayMinSeconds || gapSeconds > edge.DelayMaxSeconds)
                {
                    return new Corroboration(
                        false,
                        $"edge {edge.FromComponentId}->{edge.ToComponentId} onset gap {gapSeconds:0.#}s outside window [{edge.DelayMinSeconds},{edge.DelayMaxSeconds}]s");
                }

                edgesChecked++;
                if (seen.Add(edge.ToComponentId))
                {
                    stack.Push(edge.ToComponentId);
                }
            }
        }

        return edgesChecked == 0
            ? new Corroboration(false, "no backbone cascade to corroborate from the origin")
            : new Corroboration(true, $"{edgesChecked} backbone edge(s) fit their delay windows against the historian");
    }

    private readonly record struct EdgeRow(int FromComponentId, int ToComponentId, string Kind, int DelayMinSeconds, int DelayMaxSeconds);
}
