using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The node test of Chapter 1/2, made real over the grounding store's Component and Edge
/// tables (ADR-032, ADR-037), with every candidate property derived from the authored graph
/// plus observables (ADR-042) -- the seeded IllustrativeWeight / IllustrativeRole fields are
/// presentation-only and are never read here.
///
/// Candidates are every component on the unit that is alarmed in the flood or is the source
/// of an authored edge. Each candidate's explanation is the set of alarmed nodes reachable
/// from it over backbone AND artefact edges (a cascade origin explains its downstream
/// alarms; an artefact source explains the channels it corrupts). Learned edges stay
/// subordinate and rejected edges are never walked. Coverage is that explanation's share of
/// the flood.
///
/// Ranking is a greedy set cover over the flood's alarms: repeatedly pick the candidate that
/// explains the most alarms not already explained, breaking ties by earlier historian onset
/// and then lower ComponentId. The first pick is the origin. A candidate's weight is the
/// share of the flood it newly explained when picked (its attribution share); candidates
/// never picked weigh 0. Roles are structural, relative to the origin: downstream (reachable
/// from it), ruled-out (explains nothing), parallel (converges into its reach), contributing
/// (a learned edge into its reach), independent (none of those).
/// </summary>
public sealed class EfGraphWalker(RootCauseDbContext db) : IGraphWalker
{
    public async Task<GraphWalkResult> WalkAsync(IncidentContext ctx, CancellationToken cancellationToken)
    {
        var components = await db.Components
            .Where(c => c.UnitId == ctx.UnitId)
            .Select(c => new { c.ComponentId, c.Tag, c.AlarmCount })
            .ToListAsync(cancellationToken);

        var componentIds = components.Select(c => c.ComponentId).ToHashSet();

        var edges = await db.Edges
            .Where(e => componentIds.Contains(e.FromComponentId) && componentIds.Contains(e.ToComponentId))
            .Select(e => new { e.FromComponentId, e.ToComponentId, e.Kind })
            .ToListAsync(cancellationToken);

        // Onset = the earliest Quality==0 historian sample on the node's channel -- the
        // corroborator's own definition. A node with no samples has no onset.
        var onsets = await db.HistorianSamples
            .Where(s => s.Quality == 0 && componentIds.Contains(s.ChannelId))
            .GroupBy(s => s.ChannelId)
            .Select(g => new { ChannelId = g.Key, Onset = g.Min(s => s.TimestampUtc) })
            .ToDictionaryAsync(x => x.ChannelId, x => x.Onset, cancellationToken);

        // Walkable causal edges: backbone (cascade) and artefact. Learned and rejected are never walked.
        var adjacency = edges
            .Where(e => e.Kind is "backbone" or "artefact")
            .GroupBy(e => e.FromComponentId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ToComponentId).ToArray());

        var learnedTargets = edges
            .Where(e => e.Kind == "learned")
            .GroupBy(e => e.FromComponentId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ToComponentId).ToArray());

        var alarmCount = components.ToDictionary(c => c.ComponentId, c => c.AlarmCount);
        var alarmed = ctx.AlarmedComponentIds.ToHashSet();
        var totalAlarms = alarmed.Sum(id => alarmCount.GetValueOrDefault(id));
        var edgeSources = edges.Select(e => e.FromComponentId).ToHashSet();

        // Candidate set: alarmed in this flood, or the source of any authored edge.
        var candidates = components
            .Where(c => alarmed.Contains(c.ComponentId) || edgeSources.Contains(c.ComponentId))
            .Select(c =>
            {
                var reach = Reachable(c.ComponentId, adjacency);
                var explained = reach.Where(alarmed.Contains).ToHashSet();
                var explainedAlarms = explained.Sum(id => alarmCount.GetValueOrDefault(id));
                return new Hypothesis(
                    c.ComponentId,
                    c.Tag,
                    reach,
                    explained,
                    totalAlarms == 0 ? 0d : (double)explainedAlarms / totalAlarms,
                    onsets.TryGetValue(c.ComponentId, out var onset) ? onset : null);
            })
            .ToList();

        // Greedy set cover: most not-yet-explained alarms first; ties -> earlier onset, then lower id.
        var picked = new List<(Hypothesis Hypothesis, int NewAlarms)>();
        var explainedSoFar = new HashSet<int>();
        var remaining = candidates.ToList();
        while (remaining.Count > 0)
        {
            var best = remaining
                .Select(h => (Hypothesis: h, NewAlarms: h.Explained.Where(id => !explainedSoFar.Contains(id)).Sum(id => alarmCount.GetValueOrDefault(id))))
                .OrderByDescending(x => x.NewAlarms)
                .ThenBy(x => x.Hypothesis.Onset ?? DateTime.MaxValue)
                .ThenBy(x => x.Hypothesis.ComponentId)
                .First();

            if (best.NewAlarms == 0)
            {
                break;
            }

            picked.Add(best);
            explainedSoFar.UnionWith(best.Hypothesis.Explained);
            remaining.Remove(best.Hypothesis);
        }

        var ordered = picked
            .Select(p => (p.Hypothesis, Weight: (double)p.NewAlarms / totalAlarms))
            .Concat(remaining
                .OrderByDescending(h => h.Coverage)
                .ThenBy(h => h.Onset ?? DateTime.MaxValue)
                .ThenBy(h => h.ComponentId)
                .Select(h => (Hypothesis: h, Weight: 0d)))
            .ToList();

        var origin = picked.Count > 0 ? picked[0].Hypothesis : null;
        var ranked = ordered
            .Select(x => new RankedCandidate(x.Hypothesis.ComponentId, x.Hypothesis.Tag, RoleOf(x.Hypothesis, origin, learnedTargets), x.Weight, x.Hypothesis.Coverage))
            .ToList();

        return new GraphWalkResult(ranked);
    }

    /// <summary>The structural role rules of ADR-042, evaluated in order, relative to the origin.</summary>
    private static string RoleOf(Hypothesis h, Hypothesis? origin, IReadOnlyDictionary<int, int[]> learnedTargets)
    {
        if (origin is null)
        {
            return "ruled-out";
        }

        if (ReferenceEquals(h, origin))
        {
            return "origin";
        }

        if (origin.Reach.Contains(h.ComponentId))
        {
            return "downstream";
        }

        if (h.Explained.Count == 0)
        {
            return "ruled-out";
        }

        if (h.Reach.Overlaps(origin.Reach))
        {
            return "parallel";
        }

        if (learnedTargets.TryGetValue(h.ComponentId, out var targets) && targets.Any(origin.Reach.Contains))
        {
            return "contributing";
        }

        return "independent";
    }

    /// <summary>Every node reachable forward over walkable edges from <paramref name="start"/>, the start included.</summary>
    private static HashSet<int> Reachable(int start, IReadOnlyDictionary<int, int[]> adjacency)
    {
        var seen = new HashSet<int> { start };
        var stack = new Stack<int>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            if (!adjacency.TryGetValue(stack.Pop(), out var next))
            {
                continue;
            }

            foreach (var to in next)
            {
                if (seen.Add(to))
                {
                    stack.Push(to);
                }
            }
        }

        return seen;
    }

    private sealed class Hypothesis(int componentId, string tag, HashSet<int> reach, HashSet<int> explained, double coverage, DateTime? onset)
    {
        public int ComponentId { get; } = componentId;

        public string Tag { get; } = tag;

        public HashSet<int> Reach { get; } = reach;

        public HashSet<int> Explained { get; } = explained;

        public double Coverage { get; } = coverage;

        public DateTime? Onset { get; } = onset;
    }
}
