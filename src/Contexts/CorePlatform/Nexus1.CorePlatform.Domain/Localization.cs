using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Translated resource text (atlas C.1.4.5): one value per resource key and
/// language, depends on Language (C.1.6's only cross-table dependency
/// besides Region/Calendar). Real UpdateValue behavior — a translation
/// genuinely gets revised — mirrors the atlas's own ResolveLocalizedText
/// verification query (C.1.8), which this Phase 2 slice's Application layer
/// implements directly (ADR-015).
/// </summary>
public sealed class Localization : Entity<LocalizationId>, IAggregateRoot
{
    private Localization(
        LocalizationId id, string resourceKey, LanguageId languageId, string value, string? sourceText,
        bool isMachineTranslated, DateTime? lastReviewedAtUtc, DateTime createdAtUtc)
        : base(id)
    {
        ResourceKey = resourceKey;
        LanguageId = languageId;
        Value = value;
        SourceText = sourceText;
        IsMachineTranslated = isMachineTranslated;
        LastReviewedAtUtc = lastReviewedAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public string ResourceKey { get; }

    public LanguageId LanguageId { get; }

    public string Value { get; private set; }

    public string? SourceText { get; }

    public bool IsMachineTranslated { get; private set; }

    public DateTime? LastReviewedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public static Localization Create(
        LocalizationId id, string resourceKey, LanguageId languageId, string value, DateTime createdAtUtc,
        string? sourceText = null, bool isMachineTranslated = false, DateTime? lastReviewedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(resourceKey))
        {
            throw new ArgumentException("ResourceKey must not be empty.", nameof(resourceKey));
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Localization value must not be empty.", nameof(value));
        }

        return new Localization(
            id, resourceKey, languageId, value, sourceText, isMachineTranslated, lastReviewedAtUtc, createdAtUtc);
    }

    public void UpdateValue(string newValue, DateTime reviewedAtUtc, bool isMachineTranslated = false)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            throw new ArgumentException("Localization value must not be empty.", nameof(newValue));
        }

        Value = newValue;
        IsMachineTranslated = isMachineTranslated;
        LastReviewedAtUtc = reviewedAtUtc;
    }
}
