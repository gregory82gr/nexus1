using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>
/// Active or historical degradation case for an asset (atlas C.9.2,
/// C.9.5.2 query 5's subject). Real behavior, the sector's defining
/// lifecycle (ADR-021): starts IsActive = true / ClosedAtUtc = null via
/// Create, and is closed via <see cref="Close"/>, which sets
/// IsActive = false and ClosedAtUtc.
///
/// AssetComponentId is a real, nullable internal FK to
/// Maintenance.AssetComponent. DetectedByUserId is passport-only
/// (Security.ApplicationUser, different physical database).
///
/// DetectedAtUtc is the real business timestamp and is domain-modeled;
/// CreatedAtUtc (present in the full-audit DDL alongside CreatedBy/
/// ModifiedAtUtc/ModifiedBy/RowVersion) is pure row-insertion bookkeeping
/// and is not modeled here, same restraint as every prior sector.
/// </summary>
public sealed class DegradationRecord : Entity<DegradationRecordId>, IAggregateRoot
{
    private DegradationRecord(
        DegradationRecordId id, AssetId assetId, AssetComponentId? assetComponentId,
        DegradationMechanismId degradationMechanismId, FindingSeverityId findingSeverityId,
        ConditionGradeId? conditionGradeId, DateTime detectedAtUtc, int? detectedByUserId, string description,
        decimal? estimatedRatePerYear, bool isActive, DateTime? closedAtUtc)
        : base(id)
    {
        AssetId = assetId;
        AssetComponentId = assetComponentId;
        DegradationMechanismId = degradationMechanismId;
        FindingSeverityId = findingSeverityId;
        ConditionGradeId = conditionGradeId;
        DetectedAtUtc = detectedAtUtc;
        DetectedByUserId = detectedByUserId;
        Description = description;
        EstimatedRatePerYear = estimatedRatePerYear;
        IsActive = isActive;
        ClosedAtUtc = closedAtUtc;
    }

    public AssetId AssetId { get; }

    /// <summary>Real, nullable internal FK to Maintenance.AssetComponent.</summary>
    public AssetComponentId? AssetComponentId { get; }

    public DegradationMechanismId DegradationMechanismId { get; }

    public FindingSeverityId FindingSeverityId { get; }

    public ConditionGradeId? ConditionGradeId { get; }

    public DateTime DetectedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser (ADR-021).</summary>
    public int? DetectedByUserId { get; }

    public string Description { get; }

    public decimal? EstimatedRatePerYear { get; }

    public bool IsActive { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public static DegradationRecord Create(
        DegradationRecordId id, AssetId assetId, DegradationMechanismId degradationMechanismId,
        FindingSeverityId findingSeverityId, DateTime detectedAtUtc, string description,
        AssetComponentId? assetComponentId = null, ConditionGradeId? conditionGradeId = null,
        int? detectedByUserId = null, decimal? estimatedRatePerYear = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("DegradationRecord description must not be empty.", nameof(description));
        }

        return new DegradationRecord(
            id, assetId, assetComponentId, degradationMechanismId, findingSeverityId, conditionGradeId, detectedAtUtc,
            detectedByUserId, description, estimatedRatePerYear, isActive: true, closedAtUtc: null);
    }

    /// <summary>Closes an active degradation case: sets IsActive = false and ClosedAtUtc.</summary>
    public void Close(DateTime closedAtUtc)
    {
        IsActive = false;
        ClosedAtUtc = closedAtUtc;
    }
}
