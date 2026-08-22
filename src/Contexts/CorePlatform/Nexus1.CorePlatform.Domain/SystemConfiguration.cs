using System.Text.Json;
using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Versioned configuration contract (atlas C.1.4.3: "the structured sibling
/// of AppSetting... stores versioned JSON contracts by module"). Immutable
/// once created except for the active/inactive switch — a new configuration
/// version is a new row (ModuleName, ConfigurationKey, SchemaVersion is the
/// atlas's own natural key), not an in-place edit.
/// </summary>
public sealed class SystemConfiguration : Entity<SystemConfigurationId>, IAggregateRoot
{
    private SystemConfiguration(
        SystemConfigurationId id, string moduleName, string configurationKey, string configurationJson,
        int schemaVersion, bool isActive, DateTime effectiveFromUtc, DateTime? effectiveToUtc,
        string? description, DateTime createdAtUtc)
        : base(id)
    {
        ModuleName = moduleName;
        ConfigurationKey = configurationKey;
        ConfigurationJson = configurationJson;
        SchemaVersion = schemaVersion;
        IsActive = isActive;
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        Description = description;
        CreatedAtUtc = createdAtUtc;
    }

    public string ModuleName { get; }

    public string ConfigurationKey { get; }

    public string ConfigurationJson { get; }

    public int SchemaVersion { get; }

    public bool IsActive { get; private set; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public string? Description { get; }

    public DateTime CreatedAtUtc { get; }

    public static SystemConfiguration Create(
        SystemConfigurationId id, string moduleName, string configurationKey, string configurationJson,
        int schemaVersion, DateTime effectiveFromUtc, DateTime createdAtUtc,
        DateTime? effectiveToUtc = null, string? description = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            throw new ArgumentException("ModuleName must not be empty.", nameof(moduleName));
        }

        if (string.IsNullOrWhiteSpace(configurationKey))
        {
            throw new ArgumentException("ConfigurationKey must not be empty.", nameof(configurationKey));
        }

        if (schemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), schemaVersion, "SchemaVersion must be positive.");
        }

        if (effectiveToUtc is not null && effectiveToUtc <= effectiveFromUtc)
        {
            throw new ArgumentException(
                "EffectiveToUtc must be later than EffectiveFromUtc when present.", nameof(effectiveToUtc));
        }

        try
        {
            using var _ = JsonDocument.Parse(configurationJson);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("ConfigurationJson must be valid JSON.", nameof(configurationJson), ex);
        }

        return new SystemConfiguration(
            id, moduleName, configurationKey, configurationJson, schemaVersion, isActive,
            effectiveFromUtc, effectiveToUtc, description, createdAtUtc);
    }

    public void Deactivate() => IsActive = false;
}
