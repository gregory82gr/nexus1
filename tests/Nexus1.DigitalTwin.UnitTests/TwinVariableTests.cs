using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class TwinVariableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var variable = TwinVariable.Create(
            new TwinVariableId(1), new TwinModelId(1), new ModelVariableTypeId(1), "REACTOR_POWER", "Reactor Power", NowUtc);

        Assert.Equal("REACTOR_POWER", variable.Code);
    }

    [Fact]
    public void Create_with_lower_bound_equal_to_upper_bound_succeeds()
    {
        var variable = TwinVariable.Create(
            new TwinVariableId(1), new TwinModelId(1), new ModelVariableTypeId(1), "REACTOR_POWER", "Reactor Power", NowUtc,
            lowerBound: 100, upperBound: 100);

        Assert.Equal(100, variable.LowerBound);
        Assert.Equal(100, variable.UpperBound);
    }

    [Fact]
    public void Create_with_lower_bound_greater_than_upper_bound_throws()
    {
        Assert.Throws<ArgumentException>(() => TwinVariable.Create(
            new TwinVariableId(1), new TwinModelId(1), new ModelVariableTypeId(1), "REACTOR_POWER", "Reactor Power", NowUtc,
            lowerBound: 100, upperBound: 0));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => TwinVariable.Create(
            new TwinVariableId(1), new TwinModelId(1), new ModelVariableTypeId(1), code, "Reactor Power", NowUtc));
    }
}
