using System.Diagnostics;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Infrastructure.Messaging;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// Fast, deterministic proof of the local span graph via in-process
/// ActivityListener capture (ch.51 Verification Assets 51-A/51-E) —
/// complementary to, not a replacement for, the real-collector campaign
/// (ADR-013 task: "complete + broken trace campaign proof").
/// </summary>
public sealed class TracingTests : RootCauseComponentTestDatabase
{
    private sealed record CapturedSpan(string SourceName, string Name, ActivityKind Kind, string? ParentSpanId, string SpanId, IReadOnlyDictionary<string, object?> Tags);

    private static List<CapturedSpan> CaptureSpans(Func<Task> scenario)
    {
        var captured = new List<CapturedSpan>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == NexusActivitySources.RootCause || source.Name == NexusActivitySources.Messaging,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => captured.Add(new CapturedSpan(
                activity.Source.Name, activity.DisplayName, activity.Kind, activity.ParentSpanId.ToHexString(),
                activity.SpanId.ToHexString(), activity.TagObjects.ToDictionary(t => t.Key, t => t.Value))),
        };
        ActivitySource.AddActivityListener(listener);

        scenario().GetAwaiter().GetResult();
        return captured;
    }

    [Fact]
    public void Opening_an_analysis_emits_a_recorded_owner_span_with_a_committed_outcome()
    {
        var captured = CaptureSpans(async () =>
        {
            await using var dbContext = CreateDbContext();
            var result = await new OpenAnalysisCommandHandler(
                    Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator(), new EfOutboxWriter(dbContext))
                .Handle(new OpenAnalysisCommand(1, 100, "operator.1"), CancellationToken.None);
            Assert.True(result.IsSuccess);
        });

        var span = Assert.Single(captured, s => s.Name == SpanNames.RootCauseCaseOpen);
        Assert.Equal(NexusActivitySources.RootCause, span.SourceName);
        Assert.Equal(ActivityKind.Internal, span.Kind);
        Assert.Equal("COMMITTED", span.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void A_rejected_open_still_emits_a_span_with_a_rejected_outcome_not_a_missing_one()
    {
        var captured = CaptureSpans(async () =>
        {
            await using var dbContext = CreateDbContext();
            var result = await new OpenAnalysisCommandHandler(
                    Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator(), new EfOutboxWriter(dbContext))
                .Handle(new OpenAnalysisCommand(1, 100, ""), CancellationToken.None);
            Assert.True(result.IsFailure);
        });

        var span = Assert.Single(captured, s => s.Name == SpanNames.RootCauseCaseOpen);
        Assert.Equal("REJECTED", span.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void Full_workflow_produces_the_expected_owner_span_set_with_no_forbidden_tag_keys()
    {
        long analysisId = 0;
        int hypothesisId = 0;

        var captured = CaptureSpans(async () =>
        {
            await using (var openContext = CreateDbContext())
            {
                var openResult = await new OpenAnalysisCommandHandler(
                        Repository(openContext), UnitOfWork(openContext), new SystemDateTimeProvider(), new SequentialIdGenerator(), new EfOutboxWriter(openContext))
                    .Handle(new OpenAnalysisCommand(1, 100, "operator.1"), CancellationToken.None);
                analysisId = openResult.Value;
            }

            await using (var hypothesisContext = CreateDbContext())
            {
                var hypothesisResult = await new AddHypothesisCommandHandler(Repository(hypothesisContext), UnitOfWork(hypothesisContext), new SequentialIdGenerator())
                    .Handle(new AddHypothesisCommand(analysisId, "Loose fitting on primary loop."), CancellationToken.None);
                hypothesisId = hypothesisResult.Value;
            }

            await using (var evidenceContext = CreateDbContext())
            {
                await new AddEvidenceCommandHandler(Repository(evidenceContext), UnitOfWork(evidenceContext), new SystemDateTimeProvider(), new SequentialIdGenerator())
                    .Handle(new AddEvidenceCommand(analysisId, hypothesisId, "Inspection photo."), CancellationToken.None);
            }

            await using var closeContext = CreateDbContext();
            var closeResult = await new CloseAnalysisCommandHandler(
                    Repository(closeContext), UnitOfWork(closeContext), new SystemDateTimeProvider(), new EfOutboxWriter(closeContext))
                .Handle(new CloseAnalysisCommand(analysisId, "Loose fitting confirmed as cause.", "operator.2"), CancellationToken.None);
            Assert.True(closeResult.IsSuccess);
        });

        Assert.Contains(captured, s => s.Name == SpanNames.RootCauseCaseOpen);
        Assert.Contains(captured, s => s.Name == SpanNames.AddHypothesis);
        Assert.Contains(captured, s => s.Name == SpanNames.AddEvidence);
        Assert.Contains(captured, s => s.Name == SpanNames.RootCauseVerdictCommit);
        Assert.All(captured, s => Assert.Equal("COMMITTED", s.Tags["nexus1.outcome.code"]));

        string[] forbidden = ["payload", "authorization", "connection.string", "exception.message"];
        Assert.All(captured, s => Assert.All(forbidden, key => Assert.False(s.Tags.ContainsKey(key))));
    }
}
