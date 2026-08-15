namespace Nexus1.Contracts.ReactorFleet;

/// <summary>
/// The public shape of ReactorFleet.Domain.UnitPowerRecorded (ADR-003), carried
/// across the context boundary so consumers like AlarmManagement never take a
/// compile-time dependency on Nexus1.ReactorFleet.Domain (ADR-001-amend
/// correction, ADR-004). Plain primitives, not ReactorFleet.Domain's UnitId/
/// PowerPercent types — Contracts projects reference nothing else in the
/// solution (Nexus1.ArchitectureTests).
///
/// No .v1 suffix: unlike AlarmFloodDetected.v1/RootCauseVerdictIssued.v1, this
/// is never published to a broker in Phase 1, only referenced in-process.
/// </summary>
public sealed record UnitPowerSnapshotRecorded(int UnitId, decimal PowerPercent, DateTime RecordedAtUtc);
