using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class StateSpaceTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var stateSpace = StateSpace.Create(new StateSpaceId(1), new StateSpaceTypeId(1), "SS-001", "Deviation x Trend", 35);

        Assert.Equal("SS-001", stateSpace.Code);
        Assert.Equal(35, stateSpace.DimensionCount);
        Assert.True(stateSpace.IsDiscrete);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => StateSpace.Create(new StateSpaceId(1), new StateSpaceTypeId(1), code, "Name", 35));
    }

    [Fact]
    public void Create_with_non_positive_dimension_count_throws()
    {
        Assert.Throws<ArgumentException>(() => StateSpace.Create(new StateSpaceId(1), new StateSpaceTypeId(1), "SS-001", "Name", 0));
    }
}
