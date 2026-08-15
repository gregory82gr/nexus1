using Nexus1.BuildingBlocks.Application;
using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.Application;

public sealed class RecordUnitPowerSnapshotCommandHandler(
    IRepository<Unit, UnitId> unitRepository,
    IRepository<UnitPowerSnapshot, UnitPowerSnapshotId> snapshotRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordUnitPowerSnapshotCommand, long>
{
    public async Task<Result<long>> Handle(RecordUnitPowerSnapshotCommand command, CancellationToken cancellationToken)
    {
        var unitId = new UnitId(command.UnitId);
        var unit = await unitRepository.GetByIdAsync(unitId, cancellationToken);
        if (unit is null)
        {
            return Result<long>.Failure($"Unit {command.UnitId} does not exist.");
        }

        PowerPercent powerPercent;
        try
        {
            powerPercent = new PowerPercent(command.PowerPercent);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        var snapshotId = new UnitPowerSnapshotId(idGenerator.NextLong());
        var snapshot = UnitPowerSnapshot.Record(snapshotId, unitId, powerPercent, dateTimeProvider.UtcNow);

        await snapshotRepository.AddAsync(snapshot, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(snapshotId.Value);
    }
}
