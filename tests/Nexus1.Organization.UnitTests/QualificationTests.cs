using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class QualificationTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var qualification = Qualification.Create(
            new QualificationId(1), "RAD-PROT-1", "Radiation Protection Level 1", NowUtc, validityMonths: 24, isSafetyCritical: true);

        Assert.Equal(24, qualification.ValidityMonths);
        Assert.True(qualification.IsSafetyCritical);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_with_non_positive_validity_months_throws(int validityMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Qualification.Create(
            new QualificationId(1), "RAD-PROT-1", "Radiation Protection Level 1", NowUtc, validityMonths: validityMonths));
    }

    [Fact]
    public void Create_with_null_validity_months_succeeds()
    {
        var qualification = Qualification.Create(new QualificationId(1), "RAD-PROT-1", "Radiation Protection Level 1", NowUtc);
        Assert.Null(qualification.ValidityMonths);
    }
}
