using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed class DefineAlarmCommandHandler(
    IRepository<AlarmDefinition, AlarmDefinitionId> definitionRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<DefineAlarmCommand, int>
{
    public async Task<Result<int>> Handle(DefineAlarmCommand command, CancellationToken cancellationToken)
    {
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
            return Result<int>.Failure(ex.Message);
        }

        await definitionRepository.AddAsync(definition, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(definition.Id.Value);
    }
}
