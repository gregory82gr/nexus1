using Nexus1.BuildingBlocks.Application;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Application;

public sealed class RecordDegradationCommandHandler(
    IRepository<Asset, AssetId> assetRepository,
    IRepository<DegradationRecord, DegradationRecordId> degradationRepository,
    IRepository<DegradationTrendPoint, DegradationTrendPointId> trendPointRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordDegradationCommand, long>
{
    public async Task<Result<long>> Handle(RecordDegradationCommand command, CancellationToken cancellationToken)
    {
        var assetId = new AssetId(command.AssetId);
        if (await assetRepository.GetByIdAsync(assetId, cancellationToken) is null)
        {
            return Result<long>.Failure($"Asset {command.AssetId} does not exist.");
        }

        AssetComponentId? assetComponentId = command.AssetComponentId.HasValue
            ? new AssetComponentId(command.AssetComponentId.Value)
            : null;
        ConditionGradeId? conditionGradeId = command.ConditionGradeId.HasValue
            ? new ConditionGradeId(command.ConditionGradeId.Value)
            : null;

        DegradationRecord degradationRecord;
        try
        {
            degradationRecord = DegradationRecord.Create(
                new DegradationRecordId(idGenerator.NextLong()), assetId, new DegradationMechanismId(command.DegradationMechanismId),
                new FindingSeverityId(command.FindingSeverityId), command.DetectedAtUtc, command.Description, assetComponentId,
                conditionGradeId, command.DetectedByUserId, command.EstimatedRatePerYear);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await degradationRepository.AddAsync(degradationRecord, cancellationToken);

        foreach (var trendPointRequest in command.TrendPoints)
        {
            var trendPoint = DegradationTrendPoint.Create(
                new DegradationTrendPointId(idGenerator.NextLong()), degradationRecord.Id, trendPointRequest.EngineeringUnitId,
                trendPointRequest.MeasuredAtUtc, trendPointRequest.Value, trendPointRequest.SourceSignalId, trendPointRequest.Note);

            await trendPointRepository.AddAsync(trendPoint, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(degradationRecord.Id.Value);
    }
}
