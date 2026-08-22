using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class ActionSpaceTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var actionSpace = ActionSpace.Create(new ActionSpaceId(1), new ActionSpaceTypeId(1), "AS-001", "Rod Moves", engineeringUnitId: 5);

        Assert.Equal("AS-001", actionSpace.Code);
        Assert.Equal(5, actionSpace.EngineeringUnitId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => ActionSpace.Create(new ActionSpaceId(1), new ActionSpaceTypeId(1), "AS-001", name));
    }

    [Fact]
    public void Create_with_no_engineering_unit_leaves_it_null()
    {
        var actionSpace = ActionSpace.Create(new ActionSpaceId(1), new ActionSpaceTypeId(1), "AS-001", "Rod Moves");

        Assert.Null(actionSpace.EngineeringUnitId);
    }
}
