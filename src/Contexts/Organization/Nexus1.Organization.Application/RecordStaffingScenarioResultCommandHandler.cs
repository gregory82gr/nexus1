using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Application;

public sealed class RecordStaffingScenarioResultCommandHandler(
    IRepository<StaffingScenario, StaffingScenarioId> scenarioRepository,
    IRepository<StaffingScenarioResult, StaffingScenarioResultId> resultRepository,
    IRepository<StaffingScenarioGap, StaffingScenarioGapId> gapRepository,
    [FromKeyedServices("Organization")] IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordStaffingScenarioResultCommand, int>
{
    public async Task<Result<int>> Handle(RecordStaffingScenarioResultCommand command, CancellationToken cancellationToken)
    {
        var scenarioId = new StaffingScenarioId(command.StaffingScenarioId);

        if (await scenarioRepository.GetByIdAsync(scenarioId, cancellationToken) is null)
        {
            return Result<int>.Failure($"StaffingScenario {command.StaffingScenarioId} does not exist.");
        }

        StaffingScenarioResult result;
        try
        {
            result = StaffingScenarioResult.Create(
                new StaffingScenarioResultId(idGenerator.NextInt()), scenarioId, command.EvaluatedAtUtc,
                command.OverallStatus, command.EvaluatedByUserId, command.Summary);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await resultRepository.AddAsync(result, cancellationToken);

        foreach (var gapRequest in command.Gaps)
        {
            StaffingScenarioGap gap;
            try
            {
                gap = StaffingScenarioGap.Create(
                    new StaffingScenarioGapId(idGenerator.NextInt()), result.Id, new PositionId(gapRequest.PositionId),
                    gapRequest.RequiredCount, gapRequest.AvailableCount, gapRequest.Notes);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Result<int>.Failure(ex.Message);
            }

            await gapRepository.AddAsync(gap, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(result.Id.Value);
    }
}
