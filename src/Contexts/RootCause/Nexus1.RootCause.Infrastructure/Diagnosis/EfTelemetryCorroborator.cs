using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The telemetry witness of Chapter 5 (ADR-032): confirm the origin hypothesis's
/// cascade fits the modelled delay windows against the historian. Onset is read
/// from the continuous record -- the earliest good-quality sample per channel
/// (ChannelId == ComponentId in the skeleton) -- not from alarm timestamps,
/// because arrival order is not causal order. For every backbone edge on the
/// origin's forward path, the observed onset gap must fall inside
/// [DelayMin, DelayMax]; one edge whose downstream onset is missing or out of
/// window fails corroboration, and the pipeline abstains rather than guess.
/// </summary>
public sealed class EfTelemetryCorroborator(RootCauseDbContext db) : ITelemetryCorroborator
{
    public async Task<Corroboration> CheckAsync(IncidentContext ctx, int originComponentId, CancellationToken cancellationToken)
    {
        var backbone = await db.Edges
            .Where(e => e.Kind == "backbone")
            .Select(e => new { e.FromComponentId, e.ToComponentId, e.DelayMinSeconds, e.DelayMaxSeconds })
            .ToListAsync(cancellationToken);

        var adjacency = backbone
            .GroupBy(e => e.FromComponentId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        // Onset per channel: earliest good-quality sample.
        var onsets = await db.HistorianSamples
            .Where(s => s.Quality == 0)
            .GroupBy(s => s.ChannelId)
            .Select(g => new { ChannelId = g.Key, Onset = g.Min(s => s.TimestampUtc) })
            .ToDictionaryAsync(x => x.ChannelId, x => x.Onset, cancellationToken);

        // Walk the origin's forward backbone path and check every edge's timing.
        var seen = new HashSet<int> { originComponentId };
        var stack = new Stack<int>();
        stack.Push(originComponentId);

        var edgesChecked = 0;

        while (stack.Count > 0)
        {
            var from = stack.Pop();
            if (!adjacency.TryGetValue(from, out var edges))
            {
                continue;
            }

            foreach (var edge in edges)
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
}
