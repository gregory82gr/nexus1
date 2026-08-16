using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class SignalMappingTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EffectiveFrom = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_without_effective_to_succeeds()
    {
        var mapping = SignalMapping.Create(new SignalMappingId(1), new SignalId(1), new AcquisitionPointId(1), EffectiveFrom, NowUtc);

        Assert.Null(mapping.EffectiveToUtc);
    }

    [Fact]
    public void Create_with_effective_to_after_effective_from_succeeds()
    {
        var mapping = SignalMapping.Create(
            new SignalMappingId(1), new SignalId(1), new AcquisitionPointId(1), EffectiveFrom, NowUtc,
            effectiveToUtc: EffectiveFrom.AddDays(1));

        Assert.Equal(EffectiveFrom.AddDays(1), mapping.EffectiveToUtc);
    }

    [Fact]
    public void Create_with_effective_to_equal_to_effective_from_throws()
    {
        Assert.Throws<ArgumentException>(() => SignalMapping.Create(
            new SignalMappingId(1), new SignalId(1), new AcquisitionPointId(1), EffectiveFrom, NowUtc,
            effectiveToUtc: EffectiveFrom));
    }

    [Fact]
    public void Create_with_effective_to_before_effective_from_throws()
    {
        Assert.Throws<ArgumentException>(() => SignalMapping.Create(
            new SignalMappingId(1), new SignalId(1), new AcquisitionPointId(1), EffectiveFrom, NowUtc,
            effectiveToUtc: EffectiveFrom.AddDays(-1)));
    }

    [Fact]
    public void End_with_valid_date_closes_the_mapping()
    {
        var mapping = SignalMapping.Create(new SignalMappingId(1), new SignalId(1), new AcquisitionPointId(1), EffectiveFrom, NowUtc);

        mapping.End(EffectiveFrom.AddMonths(6));

        Assert.Equal(EffectiveFrom.AddMonths(6), mapping.EffectiveToUtc);
    }

    [Fact]
    public void End_with_date_before_effective_from_throws()
    {
        var mapping = SignalMapping.Create(new SignalMappingId(1), new SignalId(1), new AcquisitionPointId(1), EffectiveFrom, NowUtc);

        Assert.Throws<ArgumentException>(() => mapping.End(EffectiveFrom.AddDays(-1)));
    }
}
