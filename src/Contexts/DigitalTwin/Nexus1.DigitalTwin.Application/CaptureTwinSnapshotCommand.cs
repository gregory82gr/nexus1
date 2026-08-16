using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

/// <summary>Writes one TwinSnapshot plus its TwinSnapshotValue rows in a single operation (ADR-020) — matching how a snapshot actually gets produced (all variable values at once, not one at a time).</summary>
public sealed record CaptureTwinSnapshotCommand(
    long TwinRuntimeSessionId, int SnapshotReasonId, DateTime SnapshotAtUtc, IReadOnlyList<TwinSnapshotValueRequest> Values,
    long? TimeStepIndex = null, byte[]? StateVectorHash = null, string? SummaryJson = null)
    : ICommand<long>;
