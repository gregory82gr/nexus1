using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

public sealed class GetAssetsByUnitQueryHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_asset_with_its_unit_category_and_status()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetAssetsByUnitQueryHandler(new EfAssetsByUnitFinder(dbContext));

        var result = await handler.Handle(new GetAssetsByUnitQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var asset = Assert.Single(result.Value);
        Assert.Equal(MaintenanceSeedHelper.UnitCode, asset.UnitCode);
        Assert.Equal(MaintenanceSeedHelper.AssetCode, asset.AssetCode);
        Assert.Equal("PUMP", asset.Category);
        Assert.Equal("IN_SERVICE", asset.Status);
        Assert.Null(asset.EquipmentId);
    }
}
