namespace Nexus1.Contracts.ReactorFleet;

/// <summary>
/// The public shape of ReactorFleet.Domain.UnitPowerRecorded (ADR-003), carried
/// across the context boundary so consumers like AlarmManagement never take a
/// compile-time dependency on Nexus1.ReactorFleet.Domain (ADR-001-amend
/// correction, ADR-004). Plain primitives, not ReactorFleet.Domain's UnitId/
/// PowerPercent types — Contracts projects reference nothing else in the
/// solution (Nexus1.ArchitectureTests).
///
/// Versioned (V1 suffix on the type name — C# identifiers can't hold a literal
/// dot) for consistency with every public contract crossing a context
/// boundary in this repo, regardless of transport: this one is only ever
/// referenced in-process today, but it is still a public contract the moment
/// it lives in Contracts.ReactorFleet, the same reasoning ADR-001-amend's
/// correction already established for why it needs a Contracts project at
/// all. AlarmFloodDetected.v1/RootCauseVerdictIssued.v1 (named in the book,
/// not yet built as C# types — deferred to broker wiring, §5 step 7) should
/// follow this same EventNameV1 convention when they are.
/// </summary>
public sealed record UnitPowerSnapshotRecordedV1(int UnitId, decimal PowerPercent, DateTime RecordedAtUtc);
