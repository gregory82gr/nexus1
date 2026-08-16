using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Deployment metadata (atlas C.1.4.9, table name CorePlatform.Version):
/// the console, database schema, seed data, API services, workers, and
/// documentation bundles. Named DeploymentVersion, not Version —
/// <see cref="System.Version"/> already exists in the BCL and is used
/// constantly; the same CS0104 ambiguity risk as TimeZoneReference
/// (ADR-015), avoided the same way. IsCurrent is real behavior (atlas: a
/// support engineer queries "what is deployed now"), matched by
/// MarkCurrent/MarkNotCurrent — the "only one current row per component"
/// rule itself is enforced by the atlas's own filtered unique index
/// (UX_CorePlatform_Version_Current_Component), a persistence-layer
/// concern, not a Domain-layer cross-row invariant (same division of
/// responsibility this codebase already uses for AppSetting.Key and every
/// other atlas UNIQUE constraint).
/// </summary>
public sealed class DeploymentVersion : Entity<DeploymentVersionId>, IAggregateRoot
{
    private DeploymentVersion(
        DeploymentVersionId id, string componentName, DeploymentComponentType componentType, string versionNumber,
        string? buildSignature, string? gitCommit, string? schemaMigration, DateTime? releaseDateUtc,
        string? changelogSummary, bool isCurrent, DateTime createdAtUtc)
        : base(id)
    {
        ComponentName = componentName;
        ComponentType = componentType;
        VersionNumber = versionNumber;
        BuildSignature = buildSignature;
        GitCommit = gitCommit;
        SchemaMigration = schemaMigration;
        ReleaseDateUtc = releaseDateUtc;
        ChangelogSummary = changelogSummary;
        IsCurrent = isCurrent;
        CreatedAtUtc = createdAtUtc;
    }

    public string ComponentName { get; }

    public DeploymentComponentType ComponentType { get; }

    public string VersionNumber { get; }

    public string? BuildSignature { get; }

    public string? GitCommit { get; }

    public string? SchemaMigration { get; }

    public DateTime? ReleaseDateUtc { get; }

    public string? ChangelogSummary { get; }

    public bool IsCurrent { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public static DeploymentVersion Create(
        DeploymentVersionId id, string componentName, DeploymentComponentType componentType, string versionNumber,
        DateTime createdAtUtc, string? buildSignature = null, string? gitCommit = null, string? schemaMigration = null,
        DateTime? releaseDateUtc = null, string? changelogSummary = null, bool isCurrent = false)
    {
        if (string.IsNullOrWhiteSpace(componentName))
        {
            throw new ArgumentException("ComponentName must not be empty.", nameof(componentName));
        }

        if (string.IsNullOrWhiteSpace(versionNumber))
        {
            throw new ArgumentException("VersionNumber must not be empty.", nameof(versionNumber));
        }

        if (gitCommit is not null && gitCommit.Length != 40)
        {
            throw new ArgumentException("GitCommit must be exactly 40 characters when present.", nameof(gitCommit));
        }

        return new DeploymentVersion(
            id, componentName, componentType, versionNumber, buildSignature, gitCommit, schemaMigration,
            releaseDateUtc, changelogSummary, isCurrent, createdAtUtc);
    }

    public void MarkCurrent() => IsCurrent = true;

    public void MarkNotCurrent() => IsCurrent = false;
}
