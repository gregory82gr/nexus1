using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class SignalBindingTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EffectiveFromUtc = new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_no_end_date_succeeds()
    {
        var binding = SignalBinding.Create(
            new SignalBindingId(1), new TwinModelId(1), new TwinVariableId(1), signalId: 1, new BindingRoleId(1),
            new BindingStatusId(1), "ReactorPower", EffectiveFromUtc, NowUtc);

        Assert.Null(binding.EffectiveToUtc);
    }

    [Fact]
    public void Create_with_end_date_after_start_date_succeeds()
    {
        var binding = SignalBinding.Create(
            new SignalBindingId(1), new TwinModelId(1), new TwinVariableId(1), signalId: 1, new BindingRoleId(1),
            new BindingStatusId(1), "ReactorPower", EffectiveFromUtc, NowUtc, effectiveToUtc: EffectiveFromUtc.AddDays(30));

        Assert.Equal(EffectiveFromUtc.AddDays(30), binding.EffectiveToUtc);
    }

    [Fact]
    public void Create_with_end_date_equal_to_start_date_throws()
    {
        Assert.Throws<ArgumentException>(() => SignalBinding.Create(
            new SignalBindingId(1), new TwinModelId(1), new TwinVariableId(1), signalId: 1, new BindingRoleId(1),
            new BindingStatusId(1), "ReactorPower", EffectiveFromUtc, NowUtc, effectiveToUtc: EffectiveFromUtc));
    }

    [Fact]
    public void Create_with_end_date_before_start_date_throws()
    {
        Assert.Throws<ArgumentException>(() => SignalBinding.Create(
            new SignalBindingId(1), new TwinModelId(1), new TwinVariableId(1), signalId: 1, new BindingRoleId(1),
            new BindingStatusId(1), "ReactorPower", EffectiveFromUtc, NowUtc, effectiveToUtc: EffectiveFromUtc.AddDays(-1)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_model_variable_throws(string modelVariable)
    {
        Assert.Throws<ArgumentException>(() => SignalBinding.Create(
            new SignalBindingId(1), new TwinModelId(1), new TwinVariableId(1), signalId: 1, new BindingRoleId(1),
            new BindingStatusId(1), modelVariable, EffectiveFromUtc, NowUtc));
    }
}
