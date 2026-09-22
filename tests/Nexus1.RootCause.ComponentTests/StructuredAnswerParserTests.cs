using Nexus1.RootCause.Explain;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// Pure unit tests for the H5 structured-output parser (ADR-033) -- no model, no
/// database. Proves the parse-or-abstain contract: a well-formed answer becomes a
/// draft; anything else (fenced or prose-wrapped JSON is tolerated, but garbage,
/// an explicit abstention, or a missing cause) becomes a named abstention, never
/// a salvaged guess.
/// </summary>
public class StructuredAnswerParserTests
{
    [Fact]
    public void Parses_a_clean_h5_answer()
    {
        const string raw = "{\"cause\":\"FV-104\",\"entities\":[\"FV-104\"],\"claims\":[{\"text\":\"valve latency\",\"chunkId\":7}],\"confidence\":\"illustrative\",\"abstain\":false,\"abstain_reason\":null}";

        var outcome = StructuredAnswerParser.Parse(raw);

        Assert.False(outcome.Abstained);
        Assert.Equal("FV-104", outcome.Draft!.CauseTag);
        Assert.Equal(["FV-104"], outcome.Draft.Entities);
        Assert.Equal(7, outcome.Draft.Claims.Single().CitationChunkId);
    }

    [Fact]
    public void Tolerates_code_fences_and_surrounding_prose()
    {
        const string raw = "Here is the answer:\n```json\n{\"cause\":\"FV-104\",\"entities\":[\"FV-104\"],\"claims\":[],\"confidence\":\"illustrative\",\"abstain\":false}\n```\nDone.";

        var outcome = StructuredAnswerParser.Parse(raw);

        Assert.False(outcome.Abstained);
        Assert.Equal("FV-104", outcome.Draft!.CauseTag);
    }

    [Fact]
    public void An_explicit_model_abstention_is_honoured()
    {
        const string raw = "{\"cause\":null,\"entities\":[],\"claims\":[],\"confidence\":\"illustrative\",\"abstain\":true,\"abstain_reason\":\"context does not support the origin\"}";

        var outcome = StructuredAnswerParser.Parse(raw);

        Assert.True(outcome.Abstained);
        Assert.Contains("does not support", outcome.AbstainReason);
    }

    [Fact]
    public void A_missing_cause_abstains()
    {
        const string raw = "{\"cause\":\"\",\"entities\":[],\"claims\":[],\"confidence\":\"illustrative\",\"abstain\":false}";

        var outcome = StructuredAnswerParser.Parse(raw);

        Assert.True(outcome.Abstained);
        Assert.Contains("no cause", outcome.AbstainReason);
    }

    [Fact]
    public void Non_json_output_abstains_rather_than_salvaging()
    {
        var outcome = StructuredAnswerParser.Parse("I think the cause is probably the valve, FV-104.");

        Assert.True(outcome.Abstained);
        Assert.Contains("not the H5 JSON schema", outcome.AbstainReason);
    }

    [Fact]
    public void Empty_output_abstains()
    {
        Assert.True(StructuredAnswerParser.Parse("").Abstained);
        Assert.True(StructuredAnswerParser.Parse("   ").Abstained);
        Assert.True(StructuredAnswerParser.Parse(null).Abstained);
    }
}
