using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The node test of Chapter 1/2, made real over the grounding store's Component
/// and Edge tables (ADR-032). For each candidate node (one carrying an
/// illustrative weight -- the components the console ranks), walk the backbone
/// forward within the graph, collect every downstream node whose alarm is in the
/// current flood, and report coverage as the share of the flood explained. The
/// origin is the highest-coverage candidate with a non-empty explanation; its
/// role is COMPUTED here, never read from the registry. Every other candidate
/// keeps its seeded illustrative role.
///
/// Only backbone edges are walked. Learned edges are subordinate (they may
/// re-weight or propose, never define structure) and rejected edges are kept
/// visible but never traversed -- so the 4kV bus and FT-7 fall out with the low
/// or zero coverage the book shows, from the data rather than by fiat.
/// </summary>
public sealed class EfGraphWalker(RootCauseDbContext db) : IGraphWalker
{
    public async Task<GraphWalkResult> WalkAsync(IncidentContext ctx, CancellationToken cancellationToken)
    {
        var components = await db.Components
            .Where(c => c.UnitId == ctx.UnitId)
            .ToListAsync(cancellationToken);

        var componentIds = components.Select(c => c.ComponentId).ToHashSet();

        var backbone = await db.Edges
            .Where(e => e.Kind == "backbone")
            .Where(e => componentIds.Contains(e.FromComponentId) && componentIds.Contains(e.ToComponentId))
            .Select(e => new { e.FromComponentId, e.ToComponentId })
            .ToListAsync(cancellationToken);

        var adjacency = backbone
            .GroupBy(e => e.FromComponentId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ToComponentId).ToArray());

        var alarmCount = components.ToDictionary(c => c.ComponentId, c => c.AlarmCount);
        var alarmed = ctx.AlarmedComponentIds.ToHashSet();
        var totalAlarms = alarmed.Sum(id => alarmCount.GetValueOrDefault(id));

        var ranked = new List<RankedCandidate>();

        // Candidates are the components the graph blames or clears -- those the
        // book carries an illustrative weight for. The pure cascade-internal
        // nodes are walked over for coverage but are not themselves ranked.
        foreach (var candidate in components.Where(c => c.IllustrativeWeight is not null))
        {
            var explainedIds = ReachableAlarmed(candidate.ComponentId, adjacency, alarmed);
            var explainedAlarms = explainedIds.Sum(id => alarmCount.GetValueOrDefault(id));
            var coverage = totalAlarms == 0 ? 0d : (double)explainedAlarms / totalAlarms;

            ranked.Add(new RankedCandidate(
                candidate.ComponentId,
                candidate.Tag,
                candidate.IllustrativeRole ?? "candidate",
                candidate.IllustrativeWeight ?? 0d,
                coverage));
        }

        // Order by coverage, then illustrative weight -- the console's ordering.
        ranked = ranked
            .OrderByDescending(c => c.Coverage)
            .ThenByDescending(c => c.Weight)
            .ToList();

        // The origin is the top candidate that actually explains something. A
        // clean timing fit is the corroborator's job (the next gate); coverage
        // with a real explanation is enough to name the origin candidate here.
        var top = ranked.FirstOrDefault();
        if (top is not null && top.Coverage > 0d)
        {
            ranked[0] = top with { Role = "origin" };
        }

        return new GraphWalkResult(ranked);
    }

    /// <summary>The set of alarmed nodes reachable forward over backbone edges from <paramref name="start"/>, including the start itself when it is alarmed.</summary>
    private static HashSet<int> ReachableAlarmed(int start, IReadOnlyDictionary<int, int[]> adjacency, IReadOnlySet<int> alarmed)
    {
        var reached = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(start);
        var seen = new HashSet<int> { start };

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (alarmed.Contains(node))
            {
                reached.Add(node);
            }

            if (!adjacency.TryGetValue(node, out var next))
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

        return reached;
    }
}
