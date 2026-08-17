using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

/// <summary>
/// Shared seed: one ReactorFleet.Unit row, one CorePlatform.EngineeringUnit
/// row, one Instrumentation.Signal row (the real cross-context FK targets,
/// ADR-021), the nine Maintenance lookup rows, and one Asset. Every
/// component test below builds on this same shape so the eight Application
/// operations are exercised against a realistic, FK-consistent graph rather
/// than isolated rows — copying DigitalTwinSeedHelper's own precedent
/// pattern exactly (ADR-020).
/// </summary>
internal static class MaintenanceSeedHelper
{
    public const string UnitCode = "NX1-U1";
    public const string EngineeringUnitSymbol = "PCT";
    public const string SignalTag = "NX1-U1.PMP01.VIBRATION";
    public const string AssetCode = "NX1-U1-PMP-001";

    public sealed record SeedResult(
        int UnitId, int EngineeringUnitId, int SignalId, int AssetCategoryId, int AssetStatusId, int AssetCriticalityId,
        int ConditionGradeId, int DegradationMechanismId, int FindingSeverityId, int WorkOrderTypeId, int WorkOrderStatusId,
        int WorkOrderStatusClosedId, int WorkPriorityId, int AssetId);

    public static async Task<SeedResult> SeedCoreAsync(
        ReactorFleetDbContext reactorFleetContext, CorePlatformDbContext corePlatformContext,
        InstrumentationDbContext instrumentationContext, MaintenanceDbContext maintenanceContext, DateTime nowUtc)
    {
        var unit = Unit.Create(new UnitId(1), UnitCode, "Unit 1");
        await reactorFleetContext.Units.AddAsync(unit);
        await reactorFleetContext.SaveChangesAsync();

        var engineeringUnit = EngineeringUnit.Create(
            new EngineeringUnitId(1), EngineeringUnitSymbol, "Percent", EngineeringQuantityType.Percentage, nowUtc);
        await corePlatformContext.EngineeringUnits.AddAsync(engineeringUnit);
        await corePlatformContext.SaveChangesAsync();

        var signalType = SignalType.Create(new SignalTypeId(1), "MEASURED", "Measured", nowUtc);
        var signalCategory = SignalCategory.Create(new SignalCategoryId(1), "VIBRATION", "Vibration", nowUtc);
        var signalRole = SignalRole.Create(new SignalRoleId(1), "TELEMETRY", "Telemetry", nowUtc);
        var samplingMode = SamplingMode.Create(new SamplingModeId(1), "PERIODIC", "Periodic", nowUtc);
        var retentionClass = HistorianRetentionClass.Create(new HistorianRetentionClassId(1), "STANDARD", "Standard", nowUtc);

        await instrumentationContext.SignalTypes.AddAsync(signalType);
        await instrumentationContext.SignalCategories.AddAsync(signalCategory);
        await instrumentationContext.SignalRoles.AddAsync(signalRole);
        await instrumentationContext.SamplingModes.AddAsync(samplingMode);
        await instrumentationContext.HistorianRetentionClasses.AddAsync(retentionClass);
        await instrumentationContext.SaveChangesAsync();

        var signal = Signal.Create(
            new SignalId(1), unit.Id.Value, signalType.Id, signalCategory.Id, signalRole.Id, engineeringUnit.Id.Value,
            samplingMode.Id, retentionClass.Id, SignalTag, "Pump 1 Vibration", nowUtc, isHistorized: true);
        await instrumentationContext.Signals.AddAsync(signal);
        await instrumentationContext.SaveChangesAsync();

        var assetCategory = AssetCategory.Create(new AssetCategoryId(1), "PUMP", "Pump", nowUtc);
        var assetStatus = AssetStatus.Create(new AssetStatusId(1), "IN_SERVICE", "In Service", nowUtc);
        var assetCriticality = AssetCriticality.Create(new AssetCriticalityId(1), "HIGH", "High", nowUtc);
        var conditionGrade = ConditionGrade.Create(new ConditionGradeId(1), "B", "Good", nowUtc);
        var degradationMechanism = DegradationMechanism.Create(new DegradationMechanismId(1), "VIBRATION", "Vibration", nowUtc);
        var findingSeverity = FindingSeverity.Create(new FindingSeverityId(1), "MAJOR", "Major", nowUtc);
        var workOrderType = WorkOrderType.Create(new WorkOrderTypeId(1), "CORRECTIVE", "Corrective", nowUtc);
        var workOrderStatus = WorkOrderStatus.Create(new WorkOrderStatusId(1), "PLANNED", "Planned", nowUtc);
        var workOrderStatusClosed = WorkOrderStatus.Create(new WorkOrderStatusId(2), "CLOSED", "Closed", nowUtc);
        var workPriority = WorkPriority.Create(new WorkPriorityId(1), "HIGH", "High", nowUtc);

        await maintenanceContext.AssetCategories.AddAsync(assetCategory);
        await maintenanceContext.AssetStatuses.AddAsync(assetStatus);
        await maintenanceContext.AssetCriticalities.AddAsync(assetCriticality);
        await maintenanceContext.ConditionGrades.AddAsync(conditionGrade);
        await maintenanceContext.DegradationMechanisms.AddAsync(degradationMechanism);
        await maintenanceContext.FindingSeverities.AddAsync(findingSeverity);
        await maintenanceContext.WorkOrderTypes.AddAsync(workOrderType);
        await maintenanceContext.WorkOrderStatuses.AddAsync(workOrderStatus);
        await maintenanceContext.WorkOrderStatuses.AddAsync(workOrderStatusClosed);
        await maintenanceContext.WorkPriorities.AddAsync(workPriority);
        await maintenanceContext.SaveChangesAsync();

        var asset = Asset.Create(
            new AssetId(1), unit.Id.Value, assetCategory.Id, assetStatus.Id, assetCriticality.Id, AssetCode,
            "Feedwater Pump 1", nowUtc);
        await maintenanceContext.Assets.AddAsync(asset);
        await maintenanceContext.SaveChangesAsync();

        return new SeedResult(
            unit.Id.Value, engineeringUnit.Id.Value, signal.Id.Value, assetCategory.Id.Value, assetStatus.Id.Value,
            assetCriticality.Id.Value, conditionGrade.Id.Value, degradationMechanism.Id.Value, findingSeverity.Id.Value,
            workOrderType.Id.Value, workOrderStatus.Id.Value, workOrderStatusClosed.Id.Value, workPriority.Id.Value,
            asset.Id.Value);
    }
}
