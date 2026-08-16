using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;

namespace Nexus1.CorePlatform.ComponentTests;

/// <summary>Matches the atlas's own C.1.8 verification query verbatim: resolve nav.digitalTwin with an English fallback.</summary>
public sealed class ResolveLocalizedTextQueryHandlerTests : CorePlatformComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static ResolveLocalizedTextQueryHandler CreateHandler(CorePlatformDbContext dbContext) =>
        new(new EfLanguageFinder(dbContext), new EfLocalizationFinder(dbContext));

    private async Task SeedLanguagesAndTranslationAsync(bool seedTargetTranslation)
    {
        await using var seedContext = CreateDbContext();
        var english = Language.Create(new LanguageId(1), "en-GB", "English (United Kingdom)", "English", NowUtc, isDefault: true);
        var greek = Language.Create(new LanguageId(2), "el-GR", "Greek", "Ελληνικά", NowUtc);
        await seedContext.Languages.AddRangeAsync(english, greek);

        await seedContext.Localizations.AddAsync(
            Localization.Create(new LocalizationId(1), "nav.digitalTwin", english.Id, "Digital Twin", NowUtc));

        if (seedTargetTranslation)
        {
            await seedContext.Localizations.AddAsync(
                Localization.Create(new LocalizationId(2), "nav.digitalTwin", greek.Id, "Ψηφιακό Δίδυμο", NowUtc));
        }

        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Resolving_with_a_target_translation_present_returns_the_target_text()
    {
        await SeedLanguagesAndTranslationAsync(seedTargetTranslation: true);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ResolveLocalizedTextQuery("nav.digitalTwin", "el-GR"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ψηφιακό Δίδυμο", result.Value);
    }

    [Fact]
    public async Task Resolving_without_a_target_translation_falls_back_to_english()
    {
        await SeedLanguagesAndTranslationAsync(seedTargetTranslation: false);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ResolveLocalizedTextQuery("nav.digitalTwin", "el-GR"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Digital Twin", result.Value);
    }

    [Fact]
    public async Task Resolving_an_unknown_resource_key_returns_null_not_a_failure()
    {
        await SeedLanguagesAndTranslationAsync(seedTargetTranslation: true);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ResolveLocalizedTextQuery("nav.doesNotExist", "el-GR"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }
}
