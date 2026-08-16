using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>Reference table (atlas C.1.3): supported BCP 47 languages. Seeded, immutable in this Phase 2 slice.</summary>
public sealed class Language : Entity<LanguageId>, IAggregateRoot
{
    private Language(
        LanguageId id, string code, string name, string nativeName, bool isRightToLeft, bool isDefault,
        bool isActive, int displayOrder, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        NativeName = nativeName;
        IsRightToLeft = isRightToLeft;
        IsDefault = isDefault;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string NativeName { get; }

    public bool IsRightToLeft { get; }

    public bool IsDefault { get; }

    public bool IsActive { get; }

    public int DisplayOrder { get; }

    public DateTime CreatedAtUtc { get; }

    public static Language Create(
        LanguageId id, string code, string name, string nativeName, DateTime createdAtUtc,
        bool isRightToLeft = false, bool isDefault = false, bool isActive = true, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Language code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Language name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(nativeName))
        {
            throw new ArgumentException("Language native name must not be empty.", nameof(nativeName));
        }

        return new Language(id, code, name, nativeName, isRightToLeft, isDefault, isActive, displayOrder, createdAtUtc);
    }
}
