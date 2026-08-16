using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Environment feature gate (atlas C.1.4.4: "Switches capabilities on or
/// off per environment, optionally with expiry"). The other table (besides
/// AppSetting) with real runtime mutation behavior in this Phase 2 slice
/// (ADR-015) — Enable/Disable, and IsActiveAt as the actual evaluation rule
/// EvaluateFeatureFlagQuery uses.
/// </summary>
public sealed class FeatureFlag : Entity<FeatureFlagId>, IAggregateRoot
{
    private FeatureFlag(
        FeatureFlagId id, string code, string name, string environmentName, string? description,
        bool isEnabled, DateTime? enabledFromUtc, DateTime? expiresAtUtc, string? owner, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        EnvironmentName = environmentName;
        Description = description;
        IsEnabled = isEnabled;
        EnabledFromUtc = enabledFromUtc;
        ExpiresAtUtc = expiresAtUtc;
        Owner = owner;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string EnvironmentName { get; }

    public string? Description { get; }

    public bool IsEnabled { get; private set; }

    public DateTime? EnabledFromUtc { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public string? Owner { get; }

    public DateTime CreatedAtUtc { get; }

    public static FeatureFlag Create(
        FeatureFlagId id, string code, string name, DateTime createdAtUtc, string environmentName = "All",
        string? description = null, bool isEnabled = false, DateTime? enabledFromUtc = null,
        DateTime? expiresAtUtc = null, string? owner = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("FeatureFlag code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("FeatureFlag name must not be empty.", nameof(name));
        }

        ValidateExpiryWindow(enabledFromUtc, expiresAtUtc);

        return new FeatureFlag(
            id, code, name, environmentName, description, isEnabled, enabledFromUtc, expiresAtUtc, owner, createdAtUtc);
    }

    public void Enable(DateTime atUtc)
    {
        IsEnabled = true;
        EnabledFromUtc = atUtc;
        ValidateExpiryWindow(EnabledFromUtc, ExpiresAtUtc);
    }

    public void Disable() => IsEnabled = false;

    /// <summary>The real evaluation rule (atlas: "optionally with expiry") — not just the raw IsEnabled flag.</summary>
    public bool IsActiveAt(DateTime nowUtc) =>
        IsEnabled
        && (EnabledFromUtc is null || EnabledFromUtc <= nowUtc)
        && (ExpiresAtUtc is null || ExpiresAtUtc > nowUtc);

    private static void ValidateExpiryWindow(DateTime? enabledFromUtc, DateTime? expiresAtUtc)
    {
        if (expiresAtUtc is not null && enabledFromUtc is not null && expiresAtUtc <= enabledFromUtc)
        {
            throw new ArgumentException("ExpiresAtUtc must be later than EnabledFromUtc when both are present.");
        }
    }
}
