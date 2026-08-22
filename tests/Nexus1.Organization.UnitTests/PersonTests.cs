using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class PersonTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var person = Person.Create(new PersonId(1), new PersonTypeId(1), "Ada", "Lovelace", "Ada Lovelace", NowUtc);

        Assert.Equal("Ada", person.GivenName);
        Assert.Null(person.ApplicationUserId);
        Assert.True(person.IsActive);
    }

    [Fact]
    public void Create_with_application_user_passport_id_succeeds()
    {
        var person = Person.Create(
            new PersonId(1), new PersonTypeId(1), "Ada", "Lovelace", "Ada Lovelace", NowUtc, applicationUserId: 77);

        Assert.Equal(77, person.ApplicationUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_given_name_throws(string givenName)
    {
        Assert.Throws<ArgumentException>(() => Person.Create(new PersonId(1), new PersonTypeId(1), givenName, "Lovelace", "Ada Lovelace", NowUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_display_name_throws(string displayName)
    {
        Assert.Throws<ArgumentException>(() => Person.Create(new PersonId(1), new PersonTypeId(1), "Ada", "Lovelace", displayName, NowUtc));
    }
}
