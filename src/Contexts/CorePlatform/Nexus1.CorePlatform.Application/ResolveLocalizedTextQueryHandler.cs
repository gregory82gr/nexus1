using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

public sealed class ResolveLocalizedTextQueryHandler(ILanguageFinder languageFinder, ILocalizationFinder localizationFinder)
    : IQueryHandler<ResolveLocalizedTextQuery, string?>
{
    public async Task<Result<string?>> Handle(ResolveLocalizedTextQuery query, CancellationToken cancellationToken)
    {
        var targetLanguage = await languageFinder.FindByCodeAsync(query.LanguageCode, cancellationToken);
        if (targetLanguage is not null)
        {
            var targetText = await localizationFinder.FindAsync(query.ResourceKey, targetLanguage.Id, cancellationToken);
            if (targetText is not null)
            {
                return Result<string?>.Success(targetText.Value);
            }
        }

        var fallbackLanguage = await languageFinder.FindByCodeAsync(query.FallbackLanguageCode, cancellationToken);
        if (fallbackLanguage is not null)
        {
            var fallbackText = await localizationFinder.FindAsync(query.ResourceKey, fallbackLanguage.Id, cancellationToken);
            if (fallbackText is not null)
            {
                return Result<string?>.Success(fallbackText.Value);
            }
        }

        // Matches the atlas's own COALESCE(target, fallback) query: neither found -> null, not a failure.
        return Result<string?>.Success(null);
    }
}
