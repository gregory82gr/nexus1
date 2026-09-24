using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application;

/// <summary>
/// Opens a provenance-originated root-cause case (ADR-040) — one that did NOT begin from
/// an alarm flood (no alarm, no telemetry), e.g. a fault exposed only by a provenance/QA
/// audit (EVT-2026-0420). No AlarmFloodId. Parallel to OpenAnalysisCommand.
/// </summary>
public sealed record OpenProvenanceAnalysisCommand(int UnitId, string OpenedBy) : ICommand<long>;
