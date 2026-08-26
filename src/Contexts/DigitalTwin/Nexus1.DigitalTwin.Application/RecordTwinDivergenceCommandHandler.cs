using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Application;

public sealed class RecordTwinDivergenceCommandHandler(
    IRepository<TwinSnapshot, TwinSnapshotId> snapshotRepository,
    IRepository<TwinDivergence, TwinDivergenceId> divergenceRepository,
    [FromKeyedServices("DigitalTwin")] IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordTwinDivergenceCommand, long>
{
    public async Task<Result<long>> Handle(RecordTwinDivergenceCommand command, CancellationToken cancellationToken)
    {
        var snapshotId = new TwinSnapshotId(command.TwinSnapshotId);
        if (await snapshotRepository.GetByIdAsync(snapshotId, cancellationToken) is null)
        {
            return Result<long>.Failure($"TwinSnapshot {command.TwinSnapshotId} does not exist.");
        }

        // DeltaValue is computed by TwinDivergence.Create itself (MeasuredValue - ModeledValue) — never
        // accepted as a caller-supplied value here, matching ADR-020's real invariant.
        var divergence = TwinDivergence.Create(
            new TwinDivergenceId(idGenerator.NextLong()), snapshotId, command.SignalId,
            new DivergenceSeverityId(command.DivergenceSeverityId), new DivergenceStatusId(command.DivergenceStatusId),
            command.DetectedAtUtc, command.ModeledValue, command.MeasuredValue,
            command.TwinVariableId.HasValue ? new TwinVariableId(command.TwinVariableId.Value) : null,
            command.DeltaPercent, command.ThresholdAbs, command.ThresholdPercent, command.Explanation);

        await divergenceRepository.AddAsync(divergence, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(divergence.Id.Value);
    }
}
