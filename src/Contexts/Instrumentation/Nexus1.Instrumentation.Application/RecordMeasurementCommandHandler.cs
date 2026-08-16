using Nexus1.BuildingBlocks.Application;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Application;

public sealed class RecordMeasurementCommandHandler(
    IRepository<Signal, SignalId> signalRepository,
    IRepository<Measurement, MeasurementId> measurementRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordMeasurementCommand, long>
{
    public async Task<Result<long>> Handle(RecordMeasurementCommand command, CancellationToken cancellationToken)
    {
        var signalId = new SignalId(command.SignalId);
        if (await signalRepository.GetByIdAsync(signalId, cancellationToken) is null)
        {
            return Result<long>.Failure($"Signal {command.SignalId} does not exist.");
        }

        Measurement measurement;
        try
        {
            measurement = Measurement.Create(
                new MeasurementId(idGenerator.NextLong()), signalId, new SignalQualityId(command.SignalQualityId),
                new MeasurementSourceId(command.MeasurementSourceId), command.TimestampUtc, dateTimeProvider.UtcNow,
                command.NumericValue, command.TextValue, command.IsEstimated);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await measurementRepository.AddAsync(measurement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(measurement.Id.Value);
    }
}
