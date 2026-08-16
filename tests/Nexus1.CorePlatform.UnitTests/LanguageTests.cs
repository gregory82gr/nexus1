using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class LanguageTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var language = Language.Create(new LanguageId(1), "en-GB", "English (United Kingdom)", "English", NowUtc, isDefault: true);

        Assert.Equal("en-GB", language.Code);
        Assert.True(language.IsDefault);
        Assert.True(language.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Language.Create(new LanguageId(1), code, "English", "English", NowUtc));
    }
}
