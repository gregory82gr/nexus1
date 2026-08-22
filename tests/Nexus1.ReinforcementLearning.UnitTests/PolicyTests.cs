using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class PolicyTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var policy = Policy.Create(new PolicyId(1), new QTableId(1), new PolicyStatusId(1), "POL-001", "Extracted Policy v1", NowUtc, 35);

        Assert.Equal("POL-001", policy.Code);
        Assert.Equal(35, policy.EntryCount);
        Assert.Equal(NowUtc, policy.ExtractedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Policy.Create(new PolicyId(1), new QTableId(1), new PolicyStatusId(1), "POL-001", name, NowUtc, 35));
    }
}
