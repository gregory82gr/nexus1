using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Application;

public interface ILanguageFinder
{
    Task<Language?> FindByCodeAsync(string code, CancellationToken cancellationToken);
}
