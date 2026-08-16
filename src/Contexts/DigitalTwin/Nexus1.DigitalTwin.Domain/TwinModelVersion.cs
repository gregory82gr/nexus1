using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Versioned model implementation, solver configuration, code reference,
/// hash and validation state (atlas C.6.2). Required by TwinRuntimeSession —
/// without a version, a runtime session has no valid parent.
///
/// ApprovedByUserId is a Security.ApplicationUser passport int — SecurityDb
/// is a separate physical database from AlarmManagementDb regardless of
/// where DigitalTwin lives, so no enforced FK (ADR-020, same downgrade
/// every prior sector's Security references has needed).
/// </summary>
public sealed class TwinModelVersion : Entity<TwinModelVersionId>, IAggregateRoot
{
    private TwinModelVersion(
        TwinModelVersionId id, TwinModelId twinModelId, SolverTypeId solverTypeId, ValidationStatusId validationStatusId,
        string versionCode, string modelVersion, string? sourceReference, byte[]? modelHash, string? configurationJson,
        DateTime? releasedAtUtc, int? approvedByUserId, DateTime createdAtUtc)
        : base(id)
    {
        TwinModelId = twinModelId;
        SolverTypeId = solverTypeId;
        ValidationStatusId = validationStatusId;
        VersionCode = versionCode;
        ModelVersion = modelVersion;
        SourceReference = sourceReference;
        ModelHash = modelHash;
        ConfigurationJson = configurationJson;
        ReleasedAtUtc = releasedAtUtc;
        ApprovedByUserId = approvedByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    public TwinModelId TwinModelId { get; }

    public SolverTypeId SolverTypeId { get; }

    public ValidationStatusId ValidationStatusId { get; }

    public string VersionCode { get; }

    public string ModelVersion { get; }

    public string? SourceReference { get; }

    public byte[]? ModelHash { get; }

    public string? ConfigurationJson { get; }

    public DateTime? ReleasedAtUtc { get; }

    /// <summary>Security.ApplicationUser passport id — no enforced FK (ADR-020).</summary>
    public int? ApprovedByUserId { get; }

    public DateTime CreatedAtUtc { get; }

    public static TwinModelVersion Create(
        TwinModelVersionId id, TwinModelId twinModelId, SolverTypeId solverTypeId, ValidationStatusId validationStatusId,
        string versionCode, string modelVersion, DateTime createdAtUtc, string? sourceReference = null,
        byte[]? modelHash = null, string? configurationJson = null, DateTime? releasedAtUtc = null,
        int? approvedByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(versionCode))
        {
            throw new ArgumentException("TwinModelVersion version code must not be empty.", nameof(versionCode));
        }

        if (string.IsNullOrWhiteSpace(modelVersion))
        {
            throw new ArgumentException("TwinModelVersion model version must not be empty.", nameof(modelVersion));
        }

        return new TwinModelVersion(
            id, twinModelId, solverTypeId, validationStatusId, versionCode, modelVersion, sourceReference, modelHash,
            configurationJson, releasedAtUtc, approvedByUserId, createdAtUtc);
    }
}
