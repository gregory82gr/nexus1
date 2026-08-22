using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

/// <summary>Proves the reconnected atlas C.9.5.2 query 3 (ADR-022) performs real LEFT JOINs against EventManagement.OperationalEvent/IncidentAction and excludes work orders with no origin.</summary>
public sealed class GetWorkOrdersWithOriginQueryHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_joined_event_code_and_incident_action_title_and_excludes_work_orders_with_no_origin()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        var workOrderWithOrigin = WorkOrder.Create(
            new WorkOrderId(1), seed.UnitId, new WorkOrderTypeId(seed.WorkOrderTypeId), new WorkOrderStatusId(seed.WorkOrderStatusId),
            new WorkPriorityId(seed.WorkPriorityId), "WO-2026-0001", "Investigate alarm flood", NowUtc,
            originOperationalEventId: seed.OperationalEventId, originIncidentActionId: seed.IncidentActionId);

        var workOrderWithoutOrigin = WorkOrder.Create(
            new WorkOrderId(2), seed.UnitId, new WorkOrderTypeId(seed.WorkOrderTypeId), new WorkOrderStatusId(seed.WorkOrderStatusId),
            new WorkPriorityId(seed.WorkPriorityId), "WO-2026-0002", "Routine calibration", NowUtc);

        await using (var seedWorkOrderContext = CreateDbContext())
        {
            await seedWorkOrderContext.WorkOrders.AddAsync(workOrderWithOrigin);
            await seedWorkOrderContext.WorkOrders.AddAsync(workOrderWithoutOrigin);
            await seedWorkOrderContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetWorkOrdersWithOriginQueryHandler(new EfWorkOrdersWithOriginFinder(dbContext));

        var result = await handler.Handle(new GetWorkOrdersWithOriginQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value);
        Assert.Equal("WO-2026-0001", row.WorkOrderCode);
        Assert.Equal(MaintenanceSeedHelper.EventCode, row.EventCode);
        Assert.Equal(MaintenanceSeedHelper.IncidentActionTitle, row.IncidentActionTitle);
    }
}
