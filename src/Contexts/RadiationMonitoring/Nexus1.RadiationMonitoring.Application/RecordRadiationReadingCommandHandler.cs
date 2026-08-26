using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.Application;

public sealed class RecordRadiationReadingCommandHandler(
    IRepository<RadiationReading, RadiationReadingId> radiationReadingRepository, [FromKeyedServices("RadiationMonitoring")] IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordRadiationReadingCommand, long>
{
    public async Task<Result<long>> Handle(RecordRadiationReadingCommand command, CancellationToken cancellationToken)
    {
        var reading = RadiationReading.Create(
            new RadiationReadingId(idGenerator.NextLong()), new RadiationMonitorId(command.RadiationMonitorId),
            new MeasurementTypeId(command.MeasurementTypeId), command.EngineeringUnitId,
            new MeasurementQualityId(command.MeasurementQualityId), command.TimestampUtc, command.Value,
            command.IsAlarmRelevant, command.SourceTimestampUtc);

        await radiationReadingRepository.AddAsync(reading, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(reading.Id.Value);
    }
}
