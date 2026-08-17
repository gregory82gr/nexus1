using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.UnitTests;

public class EmergencyPlanTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var plan = EmergencyPlan.Create(
            new EmergencyPlanId(1), "EP-001", "Site Emergency Plan", new PlanStatusId(1), siteId: 100, ownerUserId: 501);

        Assert.Equal("EP-001", plan.Code);
        Assert.Equal("Site Emergency Plan", plan.Name);
        Assert.Equal(100, plan.SiteId);
        Assert.Null(plan.PlantId);
        Assert.Equal(501, plan.OwnerUserId);
        Assert.Equal(0, plan.CurrentRevisionNumber);
        Assert.Null(plan.EffectiveFromUtc);
        Assert.Null(plan.EffectiveToUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => EmergencyPlan.Create(
            new EmergencyPlanId(1), code, "Site Emergency Plan", new PlanStatusId(1), siteId: 100, ownerUserId: 501));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => EmergencyPlan.Create(
            new EmergencyPlanId(1), "EP-001", name, new PlanStatusId(1), siteId: 100, ownerUserId: 501));
    }

    [Fact]
    public void Create_with_passport_only_plant_id_sets_it_with_no_enforced_fk()
    {
        var plan = EmergencyPlan.Create(
            new EmergencyPlanId(1), "EP-001", "Site Emergency Plan", new PlanStatusId(1), siteId: 100,
            ownerUserId: 501, plantId: 42);

        Assert.Equal(42, plan.PlantId);
    }
}
