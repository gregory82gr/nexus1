using Nexus1.BuildingBlocks.Application;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Application;

public sealed class OpenWorkOrderCommandHandler(
    IRepository<Asset, AssetId> assetRepository,
    IRepository<WorkOrder, WorkOrderId> workOrderRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<OpenWorkOrderCommand, long>
{
    public async Task<Result<long>> Handle(OpenWorkOrderCommand command, CancellationToken cancellationToken)
    {
        AssetId? assetId = null;
        if (command.AssetId.HasValue)
        {
            assetId = new AssetId(command.AssetId.Value);
            if (await assetRepository.GetByIdAsync(assetId.Value, cancellationToken) is null)
            {
                return Result<long>.Failure($"Asset {command.AssetId} does not exist.");
            }
        }

        WorkOrder workOrder;
        try
        {
            workOrder = WorkOrder.Create(
                new WorkOrderId(idGenerator.NextLong()), command.UnitId, new WorkOrderTypeId(command.WorkOrderTypeId),
                new WorkOrderStatusId(command.WorkOrderStatusId), new WorkPriorityId(command.WorkPriorityId),
                command.WorkOrderCode, command.Title, command.RequestedAtUtc, assetId, command.MaintenancePlanId,
                command.MaintenanceWindowId, command.Description, command.DueAtUtc, requestedByUserId: command.RequestedByUserId,
                assignedTeamId: command.AssignedTeamId, assignedPersonId: command.AssignedPersonId,
                originOperationalEventId: command.OriginOperationalEventId, originIncidentActionId: command.OriginIncidentActionId);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await workOrderRepository.AddAsync(workOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(workOrder.Id.Value);
    }
}
