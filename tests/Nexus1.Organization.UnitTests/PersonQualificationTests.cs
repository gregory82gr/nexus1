using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class PersonQualificationTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_expiry_after_issued_succeeds()
    {
        var personQualification = PersonQualification.Create(
            new PersonQualificationId(1), new PersonId(1), new QualificationId(1), new QualificationStatusId(1), NowUtc,
            issuedAtUtc: NowUtc, expiresAtUtc: NowUtc.AddYears(2));

        Assert.Equal(NowUtc.AddYears(2), personQualification.ExpiresAtUtc);
    }

    [Fact]
    public void Create_with_expiry_before_issued_throws()
    {
        Assert.Throws<ArgumentException>(() => PersonQualification.Create(
            new PersonQualificationId(1), new PersonId(1), new QualificationId(1), new QualificationStatusId(1), NowUtc,
            issuedAtUtc: NowUtc, expiresAtUtc: NowUtc.AddDays(-1)));
    }

    [Fact]
    public void Create_with_expiry_equal_to_issued_throws()
    {
        Assert.Throws<ArgumentException>(() => PersonQualification.Create(
            new PersonQualificationId(1), new PersonId(1), new QualificationId(1), new QualificationStatusId(1), NowUtc,
            issuedAtUtc: NowUtc, expiresAtUtc: NowUtc));
    }

    [Fact]
    public void Create_without_issued_or_expiry_succeeds()
    {
        var personQualification = PersonQualification.Create(
            new PersonQualificationId(1), new PersonId(1), new QualificationId(1), new QualificationStatusId(1), NowUtc);

        Assert.Null(personQualification.IssuedAtUtc);
        Assert.Null(personQualification.ExpiresAtUtc);
    }

    [Fact]
    public void Verify_records_verifier_and_timestamp()
    {
        var personQualification = PersonQualification.Create(
            new PersonQualificationId(1), new PersonId(1), new QualificationId(1), new QualificationStatusId(1), NowUtc);

        personQualification.Verify(verifiedByUserId: 7, verifiedAtUtc: NowUtc.AddDays(1));

        Assert.Equal(7, personQualification.VerifiedByUserId);
        Assert.Equal(NowUtc.AddDays(1), personQualification.VerifiedAtUtc);
    }
}
