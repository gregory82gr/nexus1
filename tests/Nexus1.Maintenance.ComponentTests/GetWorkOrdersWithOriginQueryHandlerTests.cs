using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

/// <summary>Proves the adapted atlas C.9.5.2 query 3 returns the raw origin passport ids without attempting any join against EventManagement (which does not exist in this database, ADR-021).</summary>
public sealed class GetWorkOrdersWithOriginQueryHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_raw_origin_passport_ids_with_no_join_and_excludes_work_orders_with_no_origin()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        var workOrderWithOrigin = WorkOrder.Create(
            new WorkOrderId(1), seed.UnitId, new WorkOrderTypeId(seed.WorkOrderTypeId), new WorkOrderStatusId(seed.WorkOrderStatusId),
            new WorkPriorityId(seed.WorkPriorityId), "WO-2026-0001", "Investigate alarm flood", NowUtc,
            originOperationalEventId: 555L, originIncidentActionId: 777L);

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
        Assert.Equal(555L, row.OriginOperationalEventId);
        Assert.Equal(777L, row.OriginIncidentActionId);
    }
}
