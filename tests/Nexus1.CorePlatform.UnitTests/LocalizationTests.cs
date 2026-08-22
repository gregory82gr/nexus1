using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class LocalizationTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var localization = Localization.Create(
            new LocalizationId(1), "nav.digitalTwin", new LanguageId(2), "Ψηφιακό Δίδυμο", NowUtc);

        Assert.Equal("nav.digitalTwin", localization.ResourceKey);
        Assert.Equal("Ψηφιακό Δίδυμο", localization.Value);
        Assert.False(localization.IsMachineTranslated);
    }

    [Fact]
    public void Create_with_empty_value_throws()
    {
        Assert.Throws<ArgumentException>(() => Localization.Create(
            new LocalizationId(1), "nav.digitalTwin", new LanguageId(2), "", NowUtc));
    }

    [Fact]
    public void UpdateValue_revises_the_translation_and_marks_reviewed()
    {
        var localization = Localization.Create(
            new LocalizationId(1), "nav.digitalTwin", new LanguageId(2), "Ψηφιακό Δίδυμο", NowUtc);

        localization.UpdateValue("Ψηφιακό Δίδυμο (αναθεωρημένο)", NowUtc.AddDays(1));

        Assert.Equal("Ψηφιακό Δίδυμο (αναθεωρημένο)", localization.Value);
        Assert.Equal(NowUtc.AddDays(1), localization.LastReviewedAtUtc);
        Assert.False(localization.IsMachineTranslated);
    }
}
