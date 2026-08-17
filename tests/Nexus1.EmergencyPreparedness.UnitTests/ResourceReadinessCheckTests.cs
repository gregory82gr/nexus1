using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.UnitTests;

public class ResourceReadinessCheckTests
{
    private static readonly DateTime CheckedAtUtc = new(2026, 8, 17, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var check = ResourceReadinessCheck.Create(
            new ResourceReadinessCheckId(1), new EmergencyResourceId(1), new ReadinessStatusId(1), CheckedAtUtc,
            checkedByUserId: 501, "Compressor started and ran for ten minutes without fault.");

        Assert.Equal(new EmergencyResourceId(1), check.EmergencyResourceId);
        Assert.Equal(501, check.CheckedByUserId);
        Assert.Null(check.NextCheckDueUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_condition_summary_throws(string conditionSummary)
    {
        Assert.Throws<ArgumentException>(() => ResourceReadinessCheck.Create(
            new ResourceReadinessCheckId(1), new EmergencyResourceId(1), new ReadinessStatusId(1), CheckedAtUtc,
            checkedByUserId: 501, conditionSummary));
    }

    [Fact]
    public void Create_with_bigint_id_and_next_check_due_sets_both()
    {
        var check = ResourceReadinessCheck.Create(
            new ResourceReadinessCheckId(9999999999), new EmergencyResourceId(1), new ReadinessStatusId(1),
            CheckedAtUtc, checkedByUserId: 501, "Compressor started and ran for ten minutes without fault.",
            nextCheckDueUtc: CheckedAtUtc.AddMonths(6));

        Assert.Equal(9999999999L, check.Id.Value);
        Assert.Equal(CheckedAtUtc.AddMonths(6), check.NextCheckDueUtc);
    }
}
