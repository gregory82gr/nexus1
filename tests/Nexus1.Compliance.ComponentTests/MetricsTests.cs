using System.Diagnostics.Metrics;
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

/// <summary>Fast, deterministic proof of Compliance's InboxOutcomes recording via in-process MeterListener capture (ch.52 52-AB) — mirrors Nexus1.Audit.ComponentTests.MetricsTests.</summary>
public sealed class MetricsTests : ComplianceComponentTestDatabase
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
        services.AddDbContext<ComplianceDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private static byte[] BuildEnvelope(Guid messageId, long analysisId)
    {
        var payload = new RootCauseVerdictIssuedV1(analysisId, 1, 500, "Loose fitting confirmed as cause.", NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-verdict-issued.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    [Fact]
    public async Task First_delivery_records_a_committed_inbox_outcome()
    {
        var metrics = NewMetrics();
        var handler = new ComplianceVerdictMessageHandler(BuildScopeFactory(), metrics, NullLogger<ComplianceVerdictMessageHandler>.Instance);
        var messageId = Guid.NewGuid();

        var captured = await CaptureMeasurementsAsync(async () =>
        {
            var outcome = await handler.HandleAsync(messageId, BuildEnvelope(messageId, 700), CancellationToken.None);
            Assert.Equal(MessageHandlingOutcome.Ack, outcome);
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.InboxOutcomes);
        Assert.Equal(1L, measurement.Value);
        Assert.Equal("COMMITTED", measurement.Tags["nexus1.outcome"]);
        Assert.Equal(NexusActivitySources.Compliance, measurement.Tags["nexus1.component"]);
    }

    [Fact]
    public async Task A_replay_under_a_new_MessageId_records_a_duplicate_match_inbox_outcome()
    {
        var metrics = NewMetrics();
        var handler = new ComplianceVerdictMessageHandler(BuildScopeFactory(), metrics, NullLogger<ComplianceVerdictMessageHandler>.Instance);
        var firstMessageId = Guid.NewGuid();
        var replayMessageId = Guid.NewGuid();

        await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, 701), CancellationToken.None);

        var captured = await CaptureMeasurementsAsync(async () =>
        {
            var outcome = await handler.HandleAsync(replayMessageId, BuildEnvelope(replayMessageId, 701), CancellationToken.None);
            Assert.Equal(MessageHandlingOutcome.Ack, outcome);
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.InboxOutcomes);
        Assert.Equal("DUPLICATE_MATCH", measurement.Tags["nexus1.outcome"]);
    }
}
