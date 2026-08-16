using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Contracts.RootCause;
using Nexus1.Reporting.Infrastructure.Messaging;
using Nexus1.Reporting.Infrastructure.Persistence;

namespace Nexus1.Reporting.ComponentTests;

/// <summary>
/// Fast, deterministic proof of the local span graph via in-process
/// ActivityListener capture — mirrors Nexus1.Audit/Compliance.ComponentTests.
/// TracingTests (ADR-013 step 5). Reporting has two owner spans (one per
/// reducer) rather than one, so this also proves the out-of-order buffering
/// path emits its own distinct outcome code (ABSTAINED, not COMMITTED).
/// </summary>
public sealed class TracingTests : ReportingComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed record CapturedSpan(string SourceName, string Name, IReadOnlyDictionary<string, object?> Tags);

    private static List<CapturedSpan> CaptureSpans(Func<Task> scenario)
    {
        var captured = new List<CapturedSpan>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == NexusActivitySources.Reporting,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => captured.Add(new CapturedSpan(
                activity.Source.Name, activity.DisplayName, activity.TagObjects.ToDictionary(t => t.Key, t => t.Value))),
        };
        ActivitySource.AddActivityListener(listener);

        scenario().GetAwaiter().GetResult();
        return captured;
    }

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ReportingDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private ReportingProjectionMessageHandler BuildHandler() =>
        new(BuildScopeFactory(), NullLogger<ReportingProjectionMessageHandler>.Instance);

    private static byte[] BuildOpenedEnvelope(Guid messageId, long analysisId)
    {
        var payload = new RootCauseCaseOpenedV1(analysisId, 1, 500, NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-case-opened.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    private static byte[] BuildVerdictEnvelope(Guid messageId, long analysisId)
    {
        var payload = new RootCauseVerdictIssuedV1(analysisId, 1, 500, "Loose fitting confirmed as cause.", NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-verdict-issued.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    [Fact]
    public void In_order_delivery_emits_two_committed_owner_spans()
    {
        var openedMessageId = Guid.NewGuid();
        var verdictMessageId = Guid.NewGuid();

        var captured = CaptureSpans(async () =>
        {
            var handler = BuildHandler();
            await handler.HandleAsync(openedMessageId, BuildOpenedEnvelope(openedMessageId, analysisId: 700), CancellationToken.None);
            await handler.HandleAsync(verdictMessageId, BuildVerdictEnvelope(verdictMessageId, analysisId: 700), CancellationToken.None);
        });

        var openedSpan = Assert.Single(captured, s => s.Name == SpanNames.ReportingApplyOpened);
        Assert.Equal(NexusActivitySources.Reporting, openedSpan.SourceName);
        Assert.Equal("COMMITTED", openedSpan.Tags["nexus1.outcome.code"]);

        var verdictSpan = Assert.Single(captured, s => s.Name == SpanNames.ReportingApplyVerdictIssued);
        Assert.Equal("COMMITTED", verdictSpan.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void Out_of_order_verdict_delivery_emits_an_abstained_outcome()
    {
        var verdictMessageId = Guid.NewGuid();

        var captured = CaptureSpans(async () =>
        {
            var outcome = await BuildHandler().HandleAsync(verdictMessageId, BuildVerdictEnvelope(verdictMessageId, analysisId: 701), CancellationToken.None);
            Assert.Equal(MessageHandlingOutcome.Ack, outcome);
        });

        var verdictSpan = Assert.Single(captured, s => s.Name == SpanNames.ReportingApplyVerdictIssued);
        Assert.Equal("ABSTAINED", verdictSpan.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void Duplicate_CaseOpened_delivery_emits_a_duplicate_match_outcome()
    {
        var firstMessageId = Guid.NewGuid();
        var replayMessageId = Guid.NewGuid();

        var captured = CaptureSpans(async () =>
        {
            var handler = BuildHandler();
            await handler.HandleAsync(firstMessageId, BuildOpenedEnvelope(firstMessageId, analysisId: 700), CancellationToken.None);
            await handler.HandleAsync(replayMessageId, BuildOpenedEnvelope(replayMessageId, analysisId: 700), CancellationToken.None);
        });

        var openedSpans = captured.Where(s => s.Name == SpanNames.ReportingApplyOpened).ToList();
        Assert.Equal(2, openedSpans.Count);
        Assert.Equal("DUPLICATE_MATCH", openedSpans[1].Tags["nexus1.outcome.code"]);
    }
}
