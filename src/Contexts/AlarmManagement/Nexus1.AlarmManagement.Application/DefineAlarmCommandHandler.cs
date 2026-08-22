using System.Diagnostics;
using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.AlarmManagement.Application;

public sealed class DefineAlarmCommandHandler(
    IRepository<AlarmDefinition, AlarmDefinitionId> definitionRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<DefineAlarmCommand, int>
{
    public async Task<Result<int>> Handle(DefineAlarmCommand command, CancellationToken cancellationToken)
    {
        using var activity = NexusActivitySources.AlarmManagementSource.StartActivity(
            SpanNames.AlarmDefine, ActivityKind.Internal, parentContext: default,
            tags: SafeTags.ForOwnerOperation(messageId: null, "ATTEMPTED"));

        AlarmDefinition definition;
        try
        {
            definition = AlarmDefinition.Create(
                new AlarmDefinitionId(idGenerator.NextInt()),
                new UnitId(command.UnitId),
                command.Code,
                command.Name,
                command.Severity,
                command.ThresholdValue);
        }
        catch (ArgumentException ex)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result<int>.Failure(ex.Message);
        }

        try
        {
            await definitionRepository.AddAsync(definition, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            SafeError.Record(activity, ex);
            throw;
        }

        activity?.SetTag("nexus1.outcome.code", "COMMITTED");
        return Result<int>.Success(definition.Id.Value);
    }
}
