using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Application;

public sealed class CaptureTwinSnapshotCommandHandler(
    IRepository<TwinRuntimeSession, TwinRuntimeSessionId> sessionRepository,
    IRepository<TwinSnapshot, TwinSnapshotId> snapshotRepository,
    IRepository<TwinSnapshotValue, TwinSnapshotValueId> snapshotValueRepository,
    [FromKeyedServices("DigitalTwin")] IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<CaptureTwinSnapshotCommand, long>
{
    public async Task<Result<long>> Handle(CaptureTwinSnapshotCommand command, CancellationToken cancellationToken)
    {
        var sessionId = new TwinRuntimeSessionId(command.TwinRuntimeSessionId);
        if (await sessionRepository.GetByIdAsync(sessionId, cancellationToken) is null)
        {
            return Result<long>.Failure($"TwinRuntimeSession {command.TwinRuntimeSessionId} does not exist.");
        }

        var snapshot = TwinSnapshot.Create(
            new TwinSnapshotId(idGenerator.NextLong()), sessionId, new SnapshotReasonId(command.SnapshotReasonId),
            command.SnapshotAtUtc, command.TimeStepIndex, command.StateVectorHash, command.SummaryJson);

        await snapshotRepository.AddAsync(snapshot, cancellationToken);

        foreach (var valueRequest in command.Values)
        {
            TwinSnapshotValue value;
            try
            {
                value = TwinSnapshotValue.Create(
                    new TwinSnapshotValueId(idGenerator.NextLong()), snapshot.Id, new TwinVariableId(valueRequest.TwinVariableId),
                    valueRequest.NumericValue, valueRequest.TextValue, valueRequest.JsonValue);
            }
            catch (ArgumentException ex)
            {
                return Result<long>.Failure(ex.Message);
            }

            await snapshotValueRepository.AddAsync(value, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(snapshot.Id.Value);
    }
}
