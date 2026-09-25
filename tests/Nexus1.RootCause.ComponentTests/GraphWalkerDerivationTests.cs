using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;
using Nexus1.RootCause.Infrastructure.Diagnosis;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// ADR-042: the graph walker derives candidate set, order, weight and role from the authored
/// graph plus observables (alarm counts, historian onsets) -- never from the seeded
/// IllustrativeWeight / IllustrativeRole fields. Two proofs, both against real LocalDB:
/// poisoning those fields changes nothing, and a fixture with none of them at all is ranked
/// correctly, hand-worked below.
/// </summary>
public class GraphWalkerDerivationTests : RootCauseComponentTestDatabase
{
    // ----- Poisoned seed: the walker genuinely no longer reads the illustrative fields -----

    [Fact]
    public async Task Walker_output_is_byte_identical_when_the_illustrative_fields_are_poisoned()
    {
        await using (var seed = CreateDbContext())
        {
            await GroundingSeed.SeedAsync(seed);
        }

        var clean = await WalkAsJsonAsync(IncidentRegistry.Evt20260418);

        await using (var poison = CreateDbContext())
        {
            // Adversarial answer key: crown the ruled-out correlate, erase the real origin.
            await poison.Components.Where(c => c.ComponentId == GroundingSeed.Ft7)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IllustrativeWeight, 0.99).SetProperty(c => c.IllustrativeRole, "origin"));
            await poison.Components.Where(c => c.ComponentId == GroundingSeed.Fv104)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IllustrativeWeight, (double?)null).SetProperty(c => c.IllustrativeRole, (string?)null));
        }

        await using (var check = CreateDbContext())
        {
            var ft7 = await check.Components.SingleAsync(c => c.ComponentId == GroundingSeed.Ft7);
            Assert.Equal(0.99, ft7.IllustrativeWeight);
            Assert.Equal("origin", ft7.IllustrativeRole);
            Assert.Null((await check.Components.SingleAsync(c => c.ComponentId == GroundingSeed.Fv104)).IllustrativeWeight);
        }

        var poisoned = await WalkAsJsonAsync(IncidentRegistry.Evt20260418);

        Assert.Equal(clean, poisoned);
        Assert.Contains("\"Tag\":\"FV-104\",\"Role\":\"origin\"", poisoned);
    }

    // ----- Pure observables: a fixture with NO illustrative values at all -----

    // Test data only, on an unused unit -- never in IncidentRegistry, never provisioned.
    private const int PureUnit = 90;
    private const int SrcA = 901, NodeB = 902, Loud = 903, NodeD = 904, Strand = 905, Learn = 906, Corr = 907, Solo = 908, Target = 909;
    private static readonly DateTime T0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IncidentContext PureIncident = new(
        IncidentId: "TEST-PURE-OBSERVABLES",
        UnitId: PureUnit,
        AlarmedComponentIds: [SrcA, NodeB, Loud, NodeD, Strand, Learn, Solo],
        FloodStartUtc: T0,
        QueryText: "unused",
        CorpusVersion: "unused");

    /// <summary>
    /// Hand-worked (ADR-042). Alarms: A 1, B 2, LOUD 6, D 1, STRAND 2, LEARN 2, SOLO 2 = 16.
    /// Walkable reach: A->{A,B,LOUD,D,TGT}; B->{B,LOUD,D,TGT}; LOUD->{LOUD,D,TGT}; D->{D,TGT};
    /// STRAND->{STRAND,D,TGT}; LEARN, SOLO, CORR -> themselves (learned/rejected never walked).
    /// Coverage: A 10/16, B 9/16, LOUD 7/16, D 1/16, STRAND 3/16, LEARN 2/16, SOLO 2/16, CORR 0.
    /// Greedy: A adds 10 (origin). Then SOLO, STRAND, LEARN each add 2 -- a three-way tie.
    /// Onset breaks it: SOLO t5 beats STRAND and LEARN at t25, even though SOLO has the
    /// HIGHEST id (onset outranks id). STRAND and LEARN tie on onset too -> lower
    /// ComponentId, STRAND (905) before LEARN (906) -- the opposite of their tag order.
    /// Never picked: B, LOUD, D by coverage; CORR last. TGT is only an edge target -> not a candidate.
    /// Roles: B, LOUD, D reachable from A -> downstream (LOUD, the loudest node, weighs 0);
    /// CORR explains nothing -> ruled-out; STRAND reaches D in A's reach -> parallel;
    /// LEARN has a learned edge into B -> contributing; SOLO touches nothing -> independent.
    /// </summary>
    [Fact]
    public async Task Walker_ranks_a_fixture_with_no_illustrative_values_purely_from_observables()
    {
        await SeedPureFixtureAsync();

        await using var db = CreateDbContext();
        var walk = await new EfGraphWalker(db).WalkAsync(PureIncident, CancellationToken.None);

        var expected = new (string Tag, string Role, double Weight, double Coverage)[]
        {
            ("SRC-A", "origin", 10 / 16d, 10 / 16d),
            ("M-SOLO", "independent", 2 / 16d, 2 / 16d),
            ("Z-STRAND", "parallel", 2 / 16d, 3 / 16d),
            ("A-LEARN", "contributing", 2 / 16d, 2 / 16d),
            ("NODE-B", "downstream", 0d, 9 / 16d),
            ("LOUD", "downstream", 0d, 7 / 16d),
            ("NODE-D", "downstream", 0d, 1 / 16d),
            ("CORR", "ruled-out", 0d, 0d),
        };

        Assert.Equal(expected.Select(e => e.Tag), walk.Ranked.Select(c => c.Tag));
        foreach (var (e, actual) in expected.Zip(walk.Ranked))
        {
            Assert.Equal(e.Role, actual.Role);
            Assert.Equal(e.Weight, actual.Weight, 6);
            Assert.Equal(e.Coverage, actual.Coverage, 6);
        }

        Assert.DoesNotContain(walk.Ranked, c => c.Tag == "TGT");
        Assert.Equal(1d, walk.Ranked.Sum(c => c.Weight), 6); // every alarm attributed exactly once
    }

    // ----- helpers -----

    private async Task<string> WalkAsJsonAsync(IncidentContext incident)
    {
        await using var db = CreateDbContext();
        var walk = await new EfGraphWalker(db).WalkAsync(incident, CancellationToken.None);
        return JsonSerializer.Serialize(walk.Ranked);
    }

    private async Task SeedPureFixtureAsync()
    {
        await using var db = CreateDbContext();

        db.Components.AddRange(
            Node(SrcA, "SRC-A", alarms: 1),
            Node(NodeB, "NODE-B", alarms: 2),
            Node(Loud, "LOUD", alarms: 6),
            Node(NodeD, "NODE-D", alarms: 1),
            Node(Strand, "Z-STRAND", alarms: 2),
            Node(Learn, "A-LEARN", alarms: 2),
            Node(Corr, "CORR", alarms: 0),
            Node(Solo, "M-SOLO", alarms: 2),
            Node(Target, "TGT", alarms: 0));

        db.Edges.AddRange(
            Link(SrcA, NodeB, "backbone", 10),
            Link(NodeB, Loud, "backbone", 10),
            Link(Loud, NodeD, "backbone", 10),
            Link(NodeD, Target, "backbone", 5),
            Link(Strand, NodeD, "backbone", 5),
            Link(Learn, NodeB, "learned", 0),
            Link(Corr, Loud, "rejected", 0));

        db.HistorianSamples.AddRange(
            Onset(SrcA, 0),
            Onset(Solo, 5),
            Onset(NodeB, 10),
            Onset(Loud, 20),
            Onset(Strand, 25),
            Onset(Learn, 25), // a genuine onset tie with Z-STRAND
            Onset(NodeD, 30));

        await db.SaveChangesAsync();
    }

    // No IllustrativeWeight, no IllustrativeRole -- anywhere in this fixture.
    private static Component Node(int id, string tag, int alarms) =>
        new() { ComponentId = id, UnitId = PureUnit, Tag = tag, Kind = "sensor", HealthScore = null, Status = "observed", AlarmCount = alarms, IllustrativeWeight = null, IllustrativeRole = null };

    private static Edge Link(int from, int to, string kind, int delaySeconds) =>
        new() { FromComponentId = from, ToComponentId = to, Kind = kind, DelayMinSeconds = delaySeconds, DelayMaxSeconds = delaySeconds, SourceRef = "test-fixture" };

    private static HistorianSample Onset(int channelId, int offsetSeconds) =>
        new() { ChannelId = channelId, TimestampUtc = T0.AddSeconds(offsetSeconds), Value = 1.0, Quality = 0 };
}
