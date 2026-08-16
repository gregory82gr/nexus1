using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Application;

public interface ILocalizationFinder
{
    Task<Localization?> FindAsync(string resourceKey, LanguageId languageId, CancellationToken cancellationToken);
}
