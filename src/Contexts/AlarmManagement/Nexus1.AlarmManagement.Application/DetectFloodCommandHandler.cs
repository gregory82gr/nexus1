using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Contracts.AlarmManagement;

namespace Nexus1.AlarmManagement.Application;

public sealed class DetectFloodCommandHandler(
    IAlarmEventFinder eventFinder,
    IRepository<AlarmFlood, AlarmFloodId> floodRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator,
    IOutboxWriter outboxWriter)
    : ICommandHandler<DetectFloodCommand, long?>
{
    /// <summary>Routing key and eventType per ADR-008 — adopted exactly from From_Services_To_Runtime ch.25.2.</summary>
    private const string RoutingKey = "alarm-management.alarm-flood-detected.v1";
    private const string EventType = "nexus1.alarm-management.alarm-flood-detected.v1";

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

        // Same transaction as the AlarmFlood commit — outbox row and aggregate
        // write land together in the single SaveChangesAsync below, or neither
        // does (ADR-008: transactional outbox, not fire-and-forget).
        outboxWriter.Enqueue(
            EventType, schemaVersion: 1, RoutingKey, nowUtc,
            new AlarmFloodDetectedV1(flood.Id.Value, flood.UnitId.Value, flood.StartedAtUtc));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long?>.Success(flood.Id.Value);
    }
}
