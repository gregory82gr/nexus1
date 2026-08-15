using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed class DetectFloodCommandHandler(
    IAlarmEventFinder eventFinder,
    IRepository<AlarmFlood, AlarmFloodId> floodRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator)
    : ICommandHandler<DetectFloodCommand, long?>
{
    public async Task<Result<long?>> Handle(DetectFloodCommand command, CancellationToken cancellationToken)
    {
        if (command.CountThreshold < 1)
        {
            return Result<long?>.Failure("Count threshold must be at least 1.");
        }

        var window = TimeSpan.FromSeconds(command.WindowSeconds);
        var nowUtc = dateTimeProvider.UtcNow;
        var unitId = new UnitId(command.UnitId);

        var recentRaisedAtUtc = await eventFinder.GetRaisedAtUtcSinceAsync(unitId, nowUtc - window, cancellationToken);

        var shouldDetect = AlarmFloodDetector.ShouldDetectFlood(
            recentRaisedAtUtc, nowUtc, command.CountThreshold, window);

        if (!shouldDetect)
        {
            return Result<long?>.Success(null);
        }

        var flood = AlarmFlood.Detect(new AlarmFloodId(idGenerator.NextLong()), unitId, nowUtc);
        await floodRepository.AddAsync(flood, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long?>.Success(flood.Id.Value);
    }
}
