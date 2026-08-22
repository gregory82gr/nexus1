using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class EnvironmentModelTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var model = EnvironmentModel.Create(
            new EnvironmentModelId(1), new EnvironmentModelTypeId(1), unitId: 10, "ENV-001", "Point Kinetics Surrogate",
            "v1.0", 1.0m, twinModelId: 20);

        Assert.Equal("ENV-001", model.Code);
        Assert.Equal("Point Kinetics Surrogate", model.Name);
        Assert.Equal(10, model.UnitId);
        Assert.Equal(20, model.TwinModelId);
        Assert.Equal(1.0m, model.TimeStepSeconds);
        Assert.True(model.IsDeterministic);
        Assert.Null(model.RandomSeed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => EnvironmentModel.Create(
            new EnvironmentModelId(1), new EnvironmentModelTypeId(1), unitId: 10, code, "Name", "v1.0", 1.0m));
    }

    [Fact]
    public void Create_with_non_positive_time_step_throws()
    {
        Assert.Throws<ArgumentException>(() => EnvironmentModel.Create(
            new EnvironmentModelId(1), new EnvironmentModelTypeId(1), unitId: 10, "ENV-001", "Name", "v1.0", 0m));
    }

    [Fact]
    public void Create_with_passport_only_unit_id_sets_it_with_no_enforced_fk()
    {
        var model = EnvironmentModel.Create(
            new EnvironmentModelId(1), new EnvironmentModelTypeId(1), unitId: 42, "ENV-001", "Name", "v1.0", 1.0m);

        Assert.Equal(42, model.UnitId);
        Assert.Null(model.TwinModelId);
    }
}
