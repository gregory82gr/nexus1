using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Application;

public sealed class ApproveEmergencyPlanCommandHandler(
    IRepository<EmergencyPlan, EmergencyPlanId> emergencyPlanRepository, [FromKeyedServices("EmergencyPreparedness")] IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<ApproveEmergencyPlanCommand, int>
{
    public async Task<Result<int>> Handle(ApproveEmergencyPlanCommand command, CancellationToken cancellationToken)
    {
        EmergencyPlan plan;
        try
        {
            plan = EmergencyPlan.Create(
                new EmergencyPlanId(idGenerator.NextInt()), command.Code, command.Name,
                new PlanStatusId(command.PlanStatusId), command.SiteId, command.OwnerUserId, command.PlantId,
                command.CurrentRevisionNumber, command.EffectiveFromUtc, command.EffectiveToUtc,
                command.Description);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await emergencyPlanRepository.AddAsync(plan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(plan.Id.Value);
    }
}
