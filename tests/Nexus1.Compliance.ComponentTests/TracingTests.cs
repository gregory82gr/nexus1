using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Compliance.Infrastructure.Messaging;
using Nexus1.Compliance.Infrastructure.Persistence;
using Nexus1.Contracts.RootCause;

namespace Nexus1.Compliance.ComponentTests;

/// <summary>
/// Fast, deterministic proof of the local span graph via in-process
/// ActivityListener capture — mirrors Nexus1.Audit.ComponentTests.
/// TracingTests (ADR-013 step 5).
/// </summary>
public sealed class TracingTests : ComplianceComponentTestDatabase
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
            ShouldListenTo = source => source.Name == NexusActivitySources.Compliance,
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
        services.AddDbContext<ComplianceDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private ComplianceVerdictMessageHandler BuildHandler() =>
        new(BuildScopeFactory(), NullLogger<ComplianceVerdictMessageHandler>.Instance);

    private static byte[] BuildEnvelope(Guid messageId, long analysisId)
    {
        var payload = new RootCauseVerdictIssuedV1(analysisId, 1, 500, "Loose fitting confirmed as cause.", NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-verdict-issued.v1", 1, NowUtc,
            "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    [Fact]
    public void First_delivery_emits_a_committed_owner_span()
    {
        var messageId = Guid.NewGuid();

        var captured = CaptureSpans(async () =>
        {
            var outcome = await BuildHandler().HandleAsync(messageId, BuildEnvelope(messageId, analysisId: 700), CancellationToken.None);
            Assert.Equal(MessageHandlingOutcome.Ack, outcome);
        });

        var span = Assert.Single(captured, s => s.Name == SpanNames.ComplianceReviewOpen);
        Assert.Equal(NexusActivitySources.Compliance, span.SourceName);
        Assert.Equal(messageId.ToString("D"), span.Tags["nexus1.message.id"]);
        Assert.Equal("COMMITTED", span.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void A_replay_under_a_new_MessageId_emits_a_duplicate_match_outcome()
    {
        var firstMessageId = Guid.NewGuid();
        var replayMessageId = Guid.NewGuid();

        var captured = CaptureSpans(async () =>
        {
            var handler = BuildHandler();
            await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, analysisId: 700), CancellationToken.None);
            await handler.HandleAsync(replayMessageId, BuildEnvelope(replayMessageId, analysisId: 700), CancellationToken.None);
        });

        var replaySpan = Assert.Single(captured, s => s.Tags["nexus1.message.id"] as string == replayMessageId.ToString("D"));
        Assert.Equal("DUPLICATE_MATCH", replaySpan.Tags["nexus1.outcome.code"]);
    }
}
