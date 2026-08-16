using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

/// <summary>Matches the atlas's own C.1.8 verification query verbatim: "Resolve a localized UI label with an English fallback."</summary>
public sealed record ResolveLocalizedTextQuery(string ResourceKey, string LanguageCode, string FallbackLanguageCode = "en-GB")
    : IQuery<string?>;
