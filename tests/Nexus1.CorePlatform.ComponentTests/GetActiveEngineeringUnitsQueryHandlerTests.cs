using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;

namespace Nexus1.CorePlatform.ComponentTests;

public sealed class GetActiveEngineeringUnitsQueryHandlerTests : CorePlatformComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static GetActiveEngineeringUnitsQueryHandler CreateHandler(CorePlatformDbContext dbContext) =>
        new(new EfEngineeringUnitFinder(dbContext));

    [Fact]
    public async Task Only_active_units_are_returned_ordered_by_quantity_type_then_display_order()
    {
        await using (var seedContext = CreateDbContext())
        {
            await seedContext.EngineeringUnits.AddRangeAsync(
                EngineeringUnit.Create(new EngineeringUnitId(1), "MW", "megawatt electric", EngineeringQuantityType.Power, NowUtc, displayOrder: 10),
                EngineeringUnit.Create(new EngineeringUnitId(2), "MWt", "megawatt thermal", EngineeringQuantityType.ThermalPower, NowUtc, displayOrder: 20),
                EngineeringUnit.Create(new EngineeringUnitId(3), "bar", "bar", EngineeringQuantityType.Pressure, NowUtc, isActive: false));
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetActiveEngineeringUnitsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.DoesNotContain(result.Value, u => u.Symbol == "bar");
    }
}
