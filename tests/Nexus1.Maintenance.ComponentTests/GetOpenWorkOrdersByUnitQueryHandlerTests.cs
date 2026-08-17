using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

public sealed class GetOpenWorkOrdersByUnitQueryHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_open_work_orders_and_excludes_closed_ones()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        var openWorkOrder = WorkOrder.Create(
            new WorkOrderId(1), seed.UnitId, new WorkOrderTypeId(seed.WorkOrderTypeId), new WorkOrderStatusId(seed.WorkOrderStatusId),
            new WorkPriorityId(seed.WorkPriorityId), "WO-2026-0001", "Replace pump seal", NowUtc, dueAtUtc: NowUtc.AddDays(7));

        var closedWorkOrder = WorkOrder.Create(
            new WorkOrderId(2), seed.UnitId, new WorkOrderTypeId(seed.WorkOrderTypeId), new WorkOrderStatusId(seed.WorkOrderStatusClosedId),
            new WorkPriorityId(seed.WorkPriorityId), "WO-2026-0002", "Inspect valve", NowUtc, dueAtUtc: NowUtc.AddDays(1));

        await using (var seedWorkOrderContext = CreateDbContext())
        {
            await seedWorkOrderContext.WorkOrders.AddAsync(openWorkOrder);
            await seedWorkOrderContext.WorkOrders.AddAsync(closedWorkOrder);
            await seedWorkOrderContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetOpenWorkOrdersByUnitQueryHandler(new EfOpenWorkOrdersFinder(dbContext));

        var result = await handler.Handle(new GetOpenWorkOrdersByUnitQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var workOrder = Assert.Single(result.Value);
        Assert.Equal("WO-2026-0001", workOrder.WorkOrderCode);
        Assert.Equal(MaintenanceSeedHelper.UnitCode, workOrder.UnitCode);
        Assert.Equal("PLANNED", workOrder.Status);
    }
}
