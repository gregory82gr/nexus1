using Nexus1.BuildingBlocks.Application;
using Nexus1.Contracts.ReactorFleet;

namespace Nexus1.AlarmManagement.Application;

/// <summary>
/// The seam where AlarmManagement actually consumes ReactorFleet's public
/// contract (ADR-001-amend's correction, ADR-004) — takes the Contracts DTO
/// directly, never Nexus1.ReactorFleet.Domain.PowerPercent/UnitId.
/// </summary>
public sealed record EvaluateReadingCommand(UnitPowerSnapshotRecordedV1 Reading) : ICommand<int>;
