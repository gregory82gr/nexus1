using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class EngineeringUnitTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var unit = EngineeringUnit.Create(
            new EngineeringUnitId(1), "MW", "megawatt electric", EngineeringQuantityType.Power, NowUtc,
            siConversionFactor: 1_000_000m, siConversionOffset: 0m);

        Assert.Equal("MW", unit.Symbol);
        Assert.Equal(EngineeringQuantityType.Power, unit.QuantityType);
    }

    [Fact]
    public void Create_dimensionless_unit_succeeds()
    {
        var unit = EngineeringUnit.Create(
            new EngineeringUnitId(1), "pcm", "per cent mille", EngineeringQuantityType.Reactivity, NowUtc,
            isDimensionless: true);

        Assert.True(unit.IsDimensionless);
        Assert.Null(unit.SiConversionFactor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_symbol_throws(string symbol)
    {
        Assert.Throws<ArgumentException>(() => EngineeringUnit.Create(
            new EngineeringUnitId(1), symbol, "megawatt electric", EngineeringQuantityType.Power, NowUtc));
    }
}
