using System.Diagnostics;
using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;

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
        using var activity = NexusActivitySources.AlarmManagementSource.StartActivity(
            SpanNames.AlarmEvaluateReading, ActivityKind.Internal, parentContext: default,
            tags: SafeTags.ForOwnerOperation(messageId: null, "ATTEMPTED"));

        try
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

            activity?.SetTag("nexus1.outcome.code", raisedCount > 0 ? "COMMITTED" : "ABSTAINED");
            return Result<int>.Success(raisedCount);
        }
        catch (Exception ex)
        {
            SafeError.Record(activity, ex);
            throw;
        }
    }
}
