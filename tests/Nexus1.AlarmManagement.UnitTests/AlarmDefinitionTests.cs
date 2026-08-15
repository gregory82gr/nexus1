using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.UnitTests;

public class AlarmDefinitionTests
{
    private static AlarmDefinition CreateThresholdOf100() =>
        AlarmDefinition.Create(new AlarmDefinitionId(1), new UnitId(1), "HIGH-POWER", "High Power", AlarmSeverity.High, 100m);

    [Fact]
    public void Evaluate_below_threshold_returns_null()
    {
        var definition = CreateThresholdOf100();

        var result = definition.Evaluate(99.99m, new AlarmEventId(1), DateTime.UtcNow);

        Assert.Null(result);
    }

    [Fact]
    public void Evaluate_at_threshold_raises_alarm_event()
    {
        var definition = CreateThresholdOf100();
        var raisedAtUtc = new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

        var result = definition.Evaluate(100m, new AlarmEventId(1), raisedAtUtc);

        Assert.NotNull(result);
        Assert.Equal(new AlarmEventId(1), result.Id);
        Assert.Equal(definition.Id, result.AlarmDefinitionId);
        Assert.Equal(definition.UnitId, result.UnitId);
        Assert.Equal(AlarmState.Active, result.State);
        Assert.Equal(100m, result.SourceValue);
        Assert.Equal(100m, result.ThresholdValue);
    }

    [Fact]
    public void Evaluate_above_threshold_raises_alarm_event()
    {
        var definition = CreateThresholdOf100();

        var result = definition.Evaluate(150m, new AlarmEventId(2), DateTime.UtcNow);

        Assert.NotNull(result);
        Assert.Equal(150m, result.SourceValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() =>
            AlarmDefinition.Create(new AlarmDefinitionId(1), new UnitId(1), code, "Name", AlarmSeverity.High, 100m));
    }
}
