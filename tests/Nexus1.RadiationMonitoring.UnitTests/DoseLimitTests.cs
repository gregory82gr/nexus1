using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class DoseLimitTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var limit = DoseLimit.Create(
            new DoseLimitId(1), new DoseTypeId(1), new LimitTypeId(1), 2, "ANNUAL-EFF", "Annual Effective Dose Limit",
            20m, 365);

        Assert.Equal("ANNUAL-EFF", limit.Code);
        Assert.Equal(20m, limit.LimitValue);
        Assert.Equal(365, limit.PeriodDays);
        Assert.True(limit.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => DoseLimit.Create(
            new DoseLimitId(1), new DoseTypeId(1), new LimitTypeId(1), 2, code, "Annual Effective Dose Limit", 20m, 365));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => DoseLimit.Create(
            new DoseLimitId(1), new DoseTypeId(1), new LimitTypeId(1), 2, "ANNUAL-EFF", name, 20m, 365));
    }

    [Fact]
    public void Create_with_engineering_unit_id_sets_it_with_no_enforced_fk_at_the_domain_layer()
    {
        var limit = DoseLimit.Create(
            new DoseLimitId(1), new DoseTypeId(1), new LimitTypeId(1), 9, "ANNUAL-EFF", "Annual Effective Dose Limit",
            20m, 365, isActive: false);

        Assert.Equal(9, limit.EngineeringUnitId);
        Assert.False(limit.IsActive);
    }
}
