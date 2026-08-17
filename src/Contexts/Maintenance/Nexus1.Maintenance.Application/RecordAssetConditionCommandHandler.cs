using Nexus1.BuildingBlocks.Application;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Application;

public sealed class RecordAssetConditionCommandHandler(
    IRepository<Asset, AssetId> assetRepository,
    IRepository<AssetCondition, AssetConditionId> conditionRepository,
    IRepository<AssetConditionMeasurement, AssetConditionMeasurementId> measurementRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordAssetConditionCommand, long>
{
    public async Task<Result<long>> Handle(RecordAssetConditionCommand command, CancellationToken cancellationToken)
    {
        var assetId = new AssetId(command.AssetId);
        if (await assetRepository.GetByIdAsync(assetId, cancellationToken) is null)
        {
            return Result<long>.Failure($"Asset {command.AssetId} does not exist.");
        }

        AssetCondition condition;
        try
        {
            condition = AssetCondition.Create(
                new AssetConditionId(idGenerator.NextLong()), assetId, new ConditionGradeId(command.ConditionGradeId),
                command.AssessedAtUtc, command.AssessedByUserId, command.HealthScorePercent,
                command.RemainingUsefulLifeDays, command.Basis, command.Notes);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await conditionRepository.AddAsync(condition, cancellationToken);

        foreach (var measurementRequest in command.Measurements)
        {
            var measurement = AssetConditionMeasurement.Create(
                new AssetConditionMeasurementId(idGenerator.NextLong()), condition.Id, measurementRequest.EngineeringUnitId,
                measurementRequest.MeasuredValue, measurementRequest.MeasuredAtUtc, measurementRequest.SignalId,
                measurementRequest.MeasurementNote);

            await measurementRepository.AddAsync(measurement, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(condition.Id.Value);
    }
}
