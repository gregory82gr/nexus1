namespace Nexus1.RootCause.ComponentTests.Evaluation;

/// <summary>
/// The H9 discovery guard (ADR-041). A harness whose case files silently failed to copy,
/// or whose theories enumerate zero cases, would report green while evaluating nothing --
/// the "green suite discovering zero tests" anti-pattern. These facts fail loudly instead,
/// and hold every case to the known grammar so a typo cannot quietly drop a case.
/// </summary>
public class EvaluationCaseDiscoveryTests
{
    [Fact]
    public void Case_files_are_present_and_both_tiers_enumerate_cases()
    {
        Assert.True(Directory.Exists(EvalCaseCatalog.CasesDirectory), $"missing {EvalCaseCatalog.CasesDirectory}");
        Assert.NotEmpty(EvalCaseCatalog.All);
        Assert.NotEmpty(EvalCaseCatalog.IdsFor("deterministic"));
        Assert.NotEmpty(EvalCaseCatalog.IdsFor("live"));
        Assert.Equal(EvalCaseCatalog.All.Count, EvalCaseCatalog.IdsFor("deterministic").Count() + EvalCaseCatalog.IdsFor("live").Count());
    }

    [Fact]
    public void Every_case_uses_the_known_grammar_and_targets_a_built_incident()
    {
        foreach (var (file, c) in EvalCaseCatalog.AllWithFiles)
        {
            Assert.False(string.IsNullOrWhiteSpace(c.Id), $"{file}: id is missing");
            Assert.Equal(file, c.Id);
            Assert.Contains(c.Category, EvalCaseCatalog.Categories);
            Assert.Contains(c.Tier, EvalCaseCatalog.Tiers);
            Assert.Contains(c.Seam, EvalCaseCatalog.Seams);
            Assert.Contains(c.Expect, EvalCaseCatalog.Expectations);
            Assert.Contains(c.Target.IncidentId, EvalCaseCatalog.Targets);
            Assert.False(string.IsNullOrWhiteSpace(c.Intent), $"{c.Id}: intent is missing");

            if (c.Expect is "verdict" or "verdict_or_safe_abstain")
            {
                Assert.False(string.IsNullOrWhiteSpace(c.ExpectVerdict), $"{c.Id}: expectVerdict is required");
            }

            if (c.Expect == "abstain" && c.Seam == "pipeline")
            {
                Assert.False(string.IsNullOrWhiteSpace(c.ExpectAbstainReasonContains), $"{c.Id}: expectAbstainReasonContains is required");
            }

            if (c.Seam == "retriever")
            {
                Assert.False(string.IsNullOrWhiteSpace(c.RetrieverQuery), $"{c.Id}: retrieverQuery is required");
                Assert.False(string.IsNullOrWhiteSpace(c.RetrieverControlQuery), $"{c.Id}: retrieverControlQuery is required");
            }
        }

        Assert.Equal(EvalCaseCatalog.All.Count, EvalCaseCatalog.All.Select(c => c.Id).Distinct().Count());
    }

    [Fact]
    public void Every_trap_category_and_all_three_incidents_are_represented()
    {
        foreach (var category in EvalCaseCatalog.Categories)
        {
            Assert.Contains(EvalCaseCatalog.All, c => c.Category == category);
        }

        foreach (var target in EvalCaseCatalog.Targets)
        {
            Assert.Contains(EvalCaseCatalog.All, c => c.Target.IncidentId == target);
        }
    }

    [Fact]
    public void Skipped_cases_carry_a_named_reason_rather_than_being_left_out()
    {
        // The leading-question trap is authored but blocked on a missing engine check; it
        // must stay in the catalogue, visible as Skipped by name, with its stated reason.
        var leading = EvalCaseCatalog.Get("trap-leading-question");
        Assert.Equal("trap-leading", leading.Category);
        Assert.Equal("blocked: no CauseTag-vs-origin check exists — separate engine slice", leading.Skip);
        Assert.Contains("trap-leading-question", EvalCaseCatalog.IdsFor("deterministic").Select(row => (string)row[0]));
    }
}
