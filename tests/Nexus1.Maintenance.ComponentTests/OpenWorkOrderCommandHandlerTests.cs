using Nexus1.BuildingBlocks.Application;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

public sealed class OpenWorkOrderCommandHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static OpenWorkOrderCommandHandler CreateHandler(MaintenanceDbContext dbContext) => new(
        new EfRepository<Asset, AssetId>(dbContext),
        new EfRepository<WorkOrder, WorkOrderId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Opens_a_work_order_against_the_seeded_unit_and_asset()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new OpenWorkOrderCommand(
                seed.UnitId, seed.WorkOrderTypeId, seed.WorkOrderStatusId, seed.WorkPriorityId, "WO-2026-0001",
                "Replace feedwater pump seal", NowUtc, AssetId: seed.AssetId, DueAtUtc: NowUtc.AddDays(14)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var storedWorkOrder = await verifyContext.WorkOrders.FindAsync(new WorkOrderId(result.Value));
        Assert.NotNull(storedWorkOrder);
        Assert.Equal("WO-2026-0001", storedWorkOrder!.WorkOrderCode);
        Assert.Equal(new AssetId(seed.AssetId), storedWorkOrder.AssetId);
        Assert.False(storedWorkOrder.IsDeleted);
    }

    [Fact]
    public async Task Opens_a_work_order_with_no_asset_and_real_origin_ids()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new OpenWorkOrderCommand(
                seed.UnitId, seed.WorkOrderTypeId, seed.WorkOrderStatusId, seed.WorkPriorityId, "WO-2026-0002",
                "Investigate alarm flood", NowUtc, OriginOperationalEventId: seed.OperationalEventId,
                OriginIncidentActionId: seed.IncidentActionId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var storedWorkOrder = await verifyContext.WorkOrders.FindAsync(new WorkOrderId(result.Value));
        Assert.NotNull(storedWorkOrder);
        Assert.Null(storedWorkOrder!.AssetId);
        Assert.Equal(seed.OperationalEventId, storedWorkOrder.OriginOperationalEventId);
        Assert.Equal(seed.IncidentActionId, storedWorkOrder.OriginIncidentActionId);
    }

    [Fact]
    public async Task Fails_when_the_referenced_asset_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new OpenWorkOrderCommand(
                seed.UnitId, seed.WorkOrderTypeId, seed.WorkOrderStatusId, seed.WorkPriorityId, "WO-2026-0003",
                "Bad asset reference", NowUtc, AssetId: 999),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
