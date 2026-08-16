using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Mutable key-value setting (atlas C.1.4.2: "Runtime values that can be
/// changed without redeploying the application") — the one CorePlatform
/// table whose entire purpose is post-deployment mutation, so it gets a
/// real UpdateValue behavior rather than being treated as static reference
/// data (ADR-015).
/// </summary>
public sealed class AppSetting : Entity<AppSettingId>, IAggregateRoot
{
    private AppSetting(
        AppSettingId id, string key, string value, AppSettingValueType valueType, bool isEncrypted,
        string? description, bool isSystem, DateTime createdAtUtc)
        : base(id)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
        IsEncrypted = isEncrypted;
        Description = description;
        IsSystem = isSystem;
        CreatedAtUtc = createdAtUtc;
    }

    public string Key { get; }

    public string Value { get; private set; }

    public AppSettingValueType ValueType { get; }

    public bool IsEncrypted { get; }

    public string? Description { get; }

    public bool IsSystem { get; }

    public DateTime CreatedAtUtc { get; }

    public static AppSetting Create(
        AppSettingId id, string key, string value, AppSettingValueType valueType, DateTime createdAtUtc,
        bool isEncrypted = false, string? description = null, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("AppSetting key must not be empty.", nameof(key));
        }

        if (key.Length > 200)
        {
            throw new ArgumentException("AppSetting key must not exceed 200 characters.", nameof(key));
        }

        if (value.Length > 2000)
        {
            throw new ArgumentException("AppSetting value must not exceed 2000 characters.", nameof(value));
        }

        return new AppSetting(id, key, value, valueType, isEncrypted, description, isSystem, createdAtUtc);
    }

    public void UpdateValue(string newValue)
    {
        if (newValue.Length > 2000)
        {
            throw new ArgumentException("AppSetting value must not exceed 2000 characters.", nameof(newValue));
        }

        Value = newValue;
    }
}
