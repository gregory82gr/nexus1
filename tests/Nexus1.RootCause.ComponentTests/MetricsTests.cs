using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Contracts.AlarmManagement;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Infrastructure.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// Fast, deterministic proof of RootCause's InboxOutcomes/WorkflowDuration
/// recording via in-process MeterListener capture (ch.52 52-AB) —
/// complementary to, not a replacement for, the real-collector campaign.
/// Push-based Counter/Histogram measurements only fire synchronously during
/// the Add()/Record() call itself, unlike OutboxMetricState's pull-based
/// gauges, so (unlike that class's tests) a single-method-scoped
/// factory/listener needs no separate disposal wrapper here.
/// </summary>
public sealed class MetricsTests : RootCauseComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed record CapturedMeasurement(string InstrumentName, object Value, IReadOnlyDictionary<string, object?> Tags);

    private static async Task<List<CapturedMeasurement>> CaptureMeasurementsAsync(Func<Task> scenario)
    {
        var captured = new List<CapturedMeasurement>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, listenerHandle) =>
        {
            if (instrument.Meter.Name == NexusRuntimeMetrics.MeterName)
            {
                listenerHandle.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            captured.Add(new CapturedMeasurement(instrument.Name, value, ToDictionary(tags))));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            captured.Add(new CapturedMeasurement(instrument.Name, value, ToDictionary(tags))));
        listener.Start();

        await scenario();

        return captured;
    }

    private static Dictionary<string, object?> ToDictionary(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var tag in tags)
        {
            dictionary[tag.Key] = tag.Value;
        }
        return dictionary;
    }

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<RootCauseDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton<IIdGenerator, SequentialIdGenerator>();
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        services.AddScoped<IOutboxWriter, EfOutboxWriter>();
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private static byte[] BuildFloodEnvelope(Guid messageId, long alarmFloodId, int unitId, DateTime startedAtUtc)
    {
        var payload = new AlarmFloodDetectedV1(alarmFloodId, unitId, startedAtUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.alarm-management.alarm-flood-detected.v1", 1, startedAtUtc,
            "alarm-management", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    [Fact]
    public async Task First_seen_flood_delivery_records_a_committed_inbox_outcome()
    {
        var metrics = NewMetrics();
        var handler = new AlarmFloodMessageHandler(BuildScopeFactory(), metrics, NullLogger<AlarmFloodMessageHandler>.Instance);
        var messageId = Guid.NewGuid();

        var captured = await CaptureMeasurementsAsync(async () =>
        {
            var outcome = await handler.HandleAsync(messageId, BuildFloodEnvelope(messageId, 700, 1, NowUtc), CancellationToken.None);
            Assert.Equal(MessageHandlingOutcome.Ack, outcome);
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.InboxOutcomes);
        Assert.Equal(1L, measurement.Value);
        Assert.Equal("COMMITTED", measurement.Tags["nexus1.outcome"]);
        Assert.Equal(NexusActivitySources.RootCause, measurement.Tags["nexus1.component"]);
    }

    [Fact]
    public async Task Replaying_the_same_MessageId_records_a_duplicate_match_inbox_outcome()
    {
        var metrics = NewMetrics();
        var handler = new AlarmFloodMessageHandler(BuildScopeFactory(), metrics, NullLogger<AlarmFloodMessageHandler>.Instance);
        var messageId = Guid.NewGuid();
        var envelope = BuildFloodEnvelope(messageId, 701, 1, NowUtc);

        await handler.HandleAsync(messageId, envelope, CancellationToken.None);

        var captured = await CaptureMeasurementsAsync(async () =>
        {
            var outcome = await handler.HandleAsync(messageId, envelope, CancellationToken.None);
            Assert.Equal(MessageHandlingOutcome.Ack, outcome);
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.InboxOutcomes);
        Assert.Equal("DUPLICATE_MATCH", measurement.Tags["nexus1.outcome"]);
    }

    [Fact]
    public async Task Workflow_duration_is_recorded_when_the_analysis_carries_a_flood_started_timestamp()
    {
        var metrics = NewMetrics();
        var floodStartedAtUtc = NowUtc.AddSeconds(-42);
        var handler = new AlarmFloodMessageHandler(BuildScopeFactory(), metrics, NullLogger<AlarmFloodMessageHandler>.Instance);
        var floodMessageId = Guid.NewGuid();
        await handler.HandleAsync(floodMessageId, BuildFloodEnvelope(floodMessageId, 702, 1, floodStartedAtUtc), CancellationToken.None);

        long analysisId;
        await using (var dbContext = CreateDbContext())
        {
            analysisId = (await dbContext.RootCauseAnalyses.SingleAsync()).Id.Value;
        }

        int hypothesisId;
        await using (var dbContext = CreateDbContext())
        {
            var result = await new AddHypothesisCommandHandler(Repository(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator())
                .Handle(new AddHypothesisCommand(analysisId, "Loose fitting on primary loop."), CancellationToken.None);
            hypothesisId = result.Value;
        }

        await using (var dbContext = CreateDbContext())
        {
            await new AddEvidenceCommandHandler(Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator())
                .Handle(new AddEvidenceCommand(analysisId, hypothesisId, "Inspection photo."), CancellationToken.None);
        }

        var captured = await CaptureMeasurementsAsync(async () =>
        {
            await using var dbContext = CreateDbContext();
            var result = await new CloseAnalysisCommandHandler(
                    Repository(dbContext), UnitOfWork(dbContext), new FixedDateTimeProvider(NowUtc), new EfOutboxWriter(dbContext), metrics)
                .Handle(new CloseAnalysisCommand(analysisId, "Loose fitting confirmed as cause.", "operator.2"), CancellationToken.None);
            Assert.True(result.IsSuccess);
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.WorkflowDuration);
        Assert.Equal("alarm-to-verdict", measurement.Tags["nexus1.operation"]);
        Assert.Equal("COMMITTED", measurement.Tags["nexus1.outcome"]);
        Assert.InRange((double)measurement.Value, 41.5, 42.5);
    }

    [Fact]
    public async Task Workflow_duration_is_not_recorded_for_an_analysis_opened_through_the_manual_command_path()
    {
        long analysisId;
        await using (var dbContext = CreateDbContext())
        {
            var result = await new OpenAnalysisCommandHandler(
                    Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator(), new EfOutboxWriter(dbContext))
                .Handle(new OpenAnalysisCommand(1, 703, "operator.1"), CancellationToken.None);
            Assert.True(result.IsSuccess);
            analysisId = result.Value;
        }

        int hypothesisId;
        await using (var dbContext = CreateDbContext())
        {
            var result = await new AddHypothesisCommandHandler(Repository(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator())
                .Handle(new AddHypothesisCommand(analysisId, "Loose fitting on primary loop."), CancellationToken.None);
            hypothesisId = result.Value;
        }

        await using (var dbContext = CreateDbContext())
        {
            await new AddEvidenceCommandHandler(Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator())
                .Handle(new AddEvidenceCommand(analysisId, hypothesisId, "Inspection photo."), CancellationToken.None);
        }

        var metrics = NewMetrics();
        var captured = await CaptureMeasurementsAsync(async () =>
        {
            await using var dbContext = CreateDbContext();
            var result = await new CloseAnalysisCommandHandler(
                    Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new EfOutboxWriter(dbContext), metrics)
                .Handle(new CloseAnalysisCommand(analysisId, "Confirmed", "operator.1"), CancellationToken.None);
            Assert.True(result.IsSuccess);
        });

        Assert.DoesNotContain(captured, m => m.InstrumentName == MetricNames.WorkflowDuration);
    }
}
