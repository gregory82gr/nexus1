using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReactorFleet.Application;

public sealed record RecordUnitPowerSnapshotCommand(int UnitId, decimal PowerPercent) : ICommand<long>;
