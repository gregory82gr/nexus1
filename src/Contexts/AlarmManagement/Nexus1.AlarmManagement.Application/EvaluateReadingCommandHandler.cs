using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed class EvaluateReadingCommandHandler(
    IAlarmDefinitionFinder definitionFinder,
    IRepository<AlarmEvent, AlarmEventId> eventRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<EvaluateReadingCommand, int>
{
    public async Task<Result<int>> Handle(EvaluateReadingCommand command, CancellationToken cancellationToken)
    {
        var unitId = new UnitId(command.Reading.UnitId);
        var definitions = await definitionFinder.GetForUnitAsync(unitId, cancellationToken);

        var raisedCount = 0;
        foreach (var definition in definitions)
        {
            var alarmEvent = definition.Evaluate(
                command.Reading.PowerPercent, new AlarmEventId(idGenerator.NextLong()), command.Reading.RecordedAtUtc);

            if (alarmEvent is null)
            {
                continue;
            }

            await eventRepository.AddAsync(alarmEvent, cancellationToken);
            raisedCount++;
        }

        if (raisedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(raisedCount);
    }
}
