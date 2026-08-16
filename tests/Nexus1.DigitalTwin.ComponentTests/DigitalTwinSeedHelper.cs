using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

/// <summary>
/// Shared seed: one ReactorFleet.Unit row, one CorePlatform.EngineeringUnit
/// row, one Instrumentation.Signal row (the real cross-context FK targets,
/// ADR-020), the eleven DigitalTwin lookup rows, one TwinModel, one
/// TwinModelVersion, one TwinVariable, one SignalBinding wiring the variable
/// to the seeded signal, and one open TwinRuntimeSession. Every component
/// test below builds on this same shape so the six Application operations
/// are exercised against a realistic, FK-consistent graph rather than
/// isolated rows.
/// </summary>
internal static class DigitalTwinSeedHelper
{
    public const string UnitCode = "NX1-U1";
    public const string EngineeringUnitSymbol = "MW";
    public const string SignalTag = "NX1-U1.RX.POWER";
    public const string TwinModelCode = "NX1-U1-CORE-TWIN";
    public const string TwinVariableCode = "REACTOR_POWER";

    public sealed record SeedResult(
        int UnitId, int EngineeringUnitId, int SignalId, int TwinModelTypeId, int TwinModelStatusId,
        int TwinFidelityLevelId, int ModelVariableTypeId, int SolverTypeId, int ValidationStatusId, int BindingRoleId,
        int BindingStatusId, int SnapshotReasonId, int DivergenceSeverityId, int DivergenceStatusOpenId,
        int DivergenceStatusClosedId, int TwinModelId, int TwinModelVersionId, int TwinVariableId, int SignalBindingId,
        long TwinRuntimeSessionId);

    public static async Task<SeedResult> SeedCoreAsync(
        ReactorFleetDbContext reactorFleetContext, CorePlatformDbContext corePlatformContext,
        InstrumentationDbContext instrumentationContext, DigitalTwinDbContext digitalTwinContext, DateTime nowUtc)
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

        await instrumentationContext.SignalTypes.AddAsync(signalType);
        await instrumentationContext.SignalCategories.AddAsync(signalCategory);
        await instrumentationContext.SignalRoles.AddAsync(signalRole);
        await instrumentationContext.SamplingModes.AddAsync(samplingMode);
        await instrumentationContext.HistorianRetentionClasses.AddAsync(retentionClass);
        await instrumentationContext.SaveChangesAsync();

        var signal = Signal.Create(
            new SignalId(1), unit.Id.Value, signalType.Id, signalCategory.Id, signalRole.Id, engineeringUnit.Id.Value,
            samplingMode.Id, retentionClass.Id, SignalTag, "Reactor Power", nowUtc, isHistorized: true);
        await instrumentationContext.Signals.AddAsync(signal);
        await instrumentationContext.SaveChangesAsync();

        var twinModelType = TwinModelType.Create(new TwinModelTypeId(1), "POINT_KINETICS", "Point Kinetics", nowUtc);
        var twinModelStatus = TwinModelStatus.Create(new TwinModelStatusId(1), "ACTIVE", "Active", nowUtc);
        var twinFidelityLevel = TwinFidelityLevel.Create(new TwinFidelityLevelId(1), "VALIDATED", "Validated", nowUtc);
        var modelVariableType = ModelVariableType.Create(new ModelVariableTypeId(1), "STATE", "State", nowUtc);
        var solverType = SolverType.Create(new SolverTypeId(1), "RK4", "Runge-Kutta 4", nowUtc);
        var validationStatus = ValidationStatus.Create(new ValidationStatusId(1), "PASSED", "Passed", nowUtc);
        var bindingRole = BindingRole.Create(new BindingRoleId(1), "INPUT", "Input", nowUtc);
        var bindingStatus = BindingStatus.Create(new BindingStatusId(1), "ACTIVE", "Active", nowUtc);
        var snapshotReason = SnapshotReason.Create(new SnapshotReasonId(1), "SCHEDULED", "Scheduled", nowUtc);
        var divergenceSeverity = DivergenceSeverity.Create(new DivergenceSeverityId(1), "WARNING", "Warning", nowUtc);
        var divergenceStatusOpen = DivergenceStatus.Create(new DivergenceStatusId(1), "OPEN", "Open", nowUtc);
        var divergenceStatusClosed = DivergenceStatus.Create(new DivergenceStatusId(2), "CLOSED", "Closed", nowUtc);

        await digitalTwinContext.TwinModelTypes.AddAsync(twinModelType);
        await digitalTwinContext.TwinModelStatuses.AddAsync(twinModelStatus);
        await digitalTwinContext.TwinFidelityLevels.AddAsync(twinFidelityLevel);
        await digitalTwinContext.ModelVariableTypes.AddAsync(modelVariableType);
        await digitalTwinContext.SolverTypes.AddAsync(solverType);
        await digitalTwinContext.ValidationStatuses.AddAsync(validationStatus);
        await digitalTwinContext.BindingRoles.AddAsync(bindingRole);
        await digitalTwinContext.BindingStatuses.AddAsync(bindingStatus);
        await digitalTwinContext.SnapshotReasons.AddAsync(snapshotReason);
        await digitalTwinContext.DivergenceSeverities.AddAsync(divergenceSeverity);
        await digitalTwinContext.DivergenceStatuses.AddAsync(divergenceStatusOpen);
        await digitalTwinContext.DivergenceStatuses.AddAsync(divergenceStatusClosed);
        await digitalTwinContext.SaveChangesAsync();

        var twinModel = TwinModel.Create(
            new TwinModelId(1), unit.Id.Value, twinModelType.Id, twinModelStatus.Id, twinFidelityLevel.Id,
            TwinModelCode, "Unit 1 Core Twin", nowUtc);
        await digitalTwinContext.TwinModels.AddAsync(twinModel);
        await digitalTwinContext.SaveChangesAsync();

        var twinModelVersion = TwinModelVersion.Create(
            new TwinModelVersionId(1), twinModel.Id, solverType.Id, validationStatus.Id, "V1", "1.0.0", nowUtc);
        await digitalTwinContext.TwinModelVersions.AddAsync(twinModelVersion);
        await digitalTwinContext.SaveChangesAsync();

        var twinVariable = TwinVariable.Create(
            new TwinVariableId(1), twinModel.Id, modelVariableType.Id, TwinVariableCode, "Reactor Power", nowUtc,
            engineeringUnitId: engineeringUnit.Id.Value, isStateVariable: true);
        await digitalTwinContext.TwinVariables.AddAsync(twinVariable);
        await digitalTwinContext.SaveChangesAsync();

        var signalBinding = SignalBinding.Create(
            new SignalBindingId(1), twinModel.Id, twinVariable.Id, signal.Id.Value, bindingRole.Id, bindingStatus.Id,
            TwinVariableCode, nowUtc.AddDays(-1), nowUtc);
        await digitalTwinContext.SignalBindings.AddAsync(signalBinding);
        await digitalTwinContext.SaveChangesAsync();

        var runtimeSession = TwinRuntimeSession.Create(
            new TwinRuntimeSessionId(1), twinModelVersion.Id, "SESSION-1", "SHADOW", nowUtc.AddHours(-1), nowUtc);
        await digitalTwinContext.TwinRuntimeSessions.AddAsync(runtimeSession);
        await digitalTwinContext.SaveChangesAsync();

        return new SeedResult(
            unit.Id.Value, engineeringUnit.Id.Value, signal.Id.Value, twinModelType.Id.Value, twinModelStatus.Id.Value,
            twinFidelityLevel.Id.Value, modelVariableType.Id.Value, solverType.Id.Value, validationStatus.Id.Value,
            bindingRole.Id.Value, bindingStatus.Id.Value, snapshotReason.Id.Value, divergenceSeverity.Id.Value,
            divergenceStatusOpen.Id.Value, divergenceStatusClosed.Id.Value, twinModel.Id.Value, twinModelVersion.Id.Value,
            twinVariable.Id.Value, signalBinding.Id.Value, runtimeSession.Id.Value);
    }
}
