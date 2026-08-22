using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

/// <summary>
/// Shared seed: one ReactorFleet.Unit row, one CorePlatform.EngineeringUnit
/// row (the real cross-context FK targets, ADR-019), the eight
/// Instrumentation lookup rows, one full acquisition chain
/// (DataAcquisitionNode -> AcquisitionConnection -> AcquisitionPoint), and
/// one Signal mapped to that chain via SignalMapping. Every component test
/// below builds on this same shape so the six Application operations are
/// exercised against a realistic, FK-consistent graph rather than isolated
/// rows.
/// </summary>
internal static class InstrumentationSeedHelper
{
    public const string UnitCode = "NX1-U1";
    public const string EngineeringUnitSymbol = "MW";
    public const string SignalTag = "NX1-U1.RX.POWER";

    public sealed record SeedResult(
        int UnitId, int EngineeringUnitId, int SignalTypeId, int SignalCategoryId, int SignalRoleId,
        int SamplingModeId, int HistorianRetentionClassId, int SignalQualityGoodId, int SignalQualityBadId,
        int MeasurementSourceId, int ChannelStatusId, int DataAcquisitionNodeId, int AcquisitionConnectionId,
        int AcquisitionPointId, int SignalId);

    public static async Task<SeedResult> SeedCoreAsync(
        ReactorFleetDbContext reactorFleetContext, CorePlatformDbContext corePlatformContext,
        InstrumentationDbContext instrumentationContext, DateTime nowUtc)
    {
        var unit = Unit.Create(new UnitId(1), UnitCode, "Unit 1");
        await reactorFleetContext.Units.AddAsync(unit);
        await reactorFleetContext.SaveChangesAsync();

        var engineeringUnit = EngineeringUnit.Create(
            new EngineeringUnitId(1), EngineeringUnitSymbol, "Megawatt", EngineeringQuantityType.Power, nowUtc);
        await corePlatformContext.EngineeringUnits.AddAsync(engineeringUnit);
        await corePlatformContext.SaveChangesAsync();

        var signalType = SignalType.Create(new SignalTypeId(1), "MEASURED", "Measured", nowUtc);
        var signalCategory = SignalCategory.Create(new SignalCategoryId(1), "POWER", "Power", nowUtc);
        var signalRole = SignalRole.Create(new SignalRoleId(1), "TELEMETRY", "Telemetry", nowUtc);
        var samplingMode = SamplingMode.Create(new SamplingModeId(1), "PERIODIC", "Periodic", nowUtc);
        var retentionClass = HistorianRetentionClass.Create(new HistorianRetentionClassId(1), "STANDARD", "Standard", nowUtc);
        var qualityGood = SignalQuality.Create(new SignalQualityId(1), "GOOD", "Good", nowUtc);
        var qualityBad = SignalQuality.Create(new SignalQualityId(2), "BAD", "Bad", nowUtc);
        var measurementSource = MeasurementSource.Create(new MeasurementSourceId(1), "HISTORIAN", "Historian", nowUtc);
        var channelStatus = ChannelStatus.Create(new ChannelStatusId(1), "ONLINE", "Online", nowUtc);

        await instrumentationContext.SignalTypes.AddAsync(signalType);
        await instrumentationContext.SignalCategories.AddAsync(signalCategory);
        await instrumentationContext.SignalRoles.AddAsync(signalRole);
        await instrumentationContext.SamplingModes.AddAsync(samplingMode);
        await instrumentationContext.HistorianRetentionClasses.AddAsync(retentionClass);
        await instrumentationContext.SignalQualities.AddAsync(qualityGood);
        await instrumentationContext.SignalQualities.AddAsync(qualityBad);
        await instrumentationContext.MeasurementSources.AddAsync(measurementSource);
        await instrumentationContext.ChannelStatuses.AddAsync(channelStatus);
        await instrumentationContext.SaveChangesAsync();

        var node = DataAcquisitionNode.Create(new DataAcquisitionNodeId(1), unit.Id.Value, channelStatus.Id, "NODE-1", "Node One", nowUtc);
        await instrumentationContext.DataAcquisitionNodes.AddAsync(node);
        await instrumentationContext.SaveChangesAsync();

        var connection = AcquisitionConnection.Create(
            new AcquisitionConnectionId(1), node.Id, channelStatus.Id, "CONN-1", "OPC-UA", nowUtc);
        await instrumentationContext.AcquisitionConnections.AddAsync(connection);
        await instrumentationContext.SaveChangesAsync();

        var point = AcquisitionPoint.Create(new AcquisitionPointId(1), connection.Id, "PT-1", "ns=2;s=Reactor.Power", nowUtc);
        await instrumentationContext.AcquisitionPoints.AddAsync(point);
        await instrumentationContext.SaveChangesAsync();

        var signal = Signal.Create(
            new SignalId(1), unit.Id.Value, signalType.Id, signalCategory.Id, signalRole.Id, engineeringUnit.Id.Value,
            samplingMode.Id, retentionClass.Id, SignalTag, "Reactor Power", nowUtc, isHistorized: true);
        await instrumentationContext.Signals.AddAsync(signal);
        await instrumentationContext.SaveChangesAsync();

        var mapping = SignalMapping.Create(new SignalMappingId(1), signal.Id, point.Id, nowUtc.AddDays(-1), nowUtc);
        await instrumentationContext.SignalMappings.AddAsync(mapping);
        await instrumentationContext.SaveChangesAsync();

        return new SeedResult(
            unit.Id.Value, engineeringUnit.Id.Value, signalType.Id.Value, signalCategory.Id.Value, signalRole.Id.Value,
            samplingMode.Id.Value, retentionClass.Id.Value, qualityGood.Id.Value, qualityBad.Id.Value,
            measurementSource.Id.Value, channelStatus.Id.Value, node.Id.Value, connection.Id.Value, point.Id.Value,
            signal.Id.Value);
    }
}
