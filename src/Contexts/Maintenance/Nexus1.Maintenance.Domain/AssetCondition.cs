using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>
/// Point-in-time condition assessment, health score and useful-life
/// estimate (atlas C.9.2, C.9.5.2 query 4's subject). Real invariant:
/// HealthScorePercent, when set, must be in [0, 100]
/// (CK_Maintenance_AssetCondition_HealthScore).
///
/// AssessedByUserId is passport-only (Security.ApplicationUser lives in a
/// different physical database than this sector's own chosen home,
/// ADR-021).
///
/// Leaner audit shape than most tables in this sector — the atlas DDL gives
/// this table only CreatedAtUtc + CreatedBy (no ModifiedAtUtc/ModifiedBy/
/// IsDeleted/RowVersion), verified directly against the real DDL.
/// AssessedAtUtc is the real business timestamp and is domain-modeled;
/// CreatedAtUtc is pure row-insertion bookkeeping with a SQL DEFAULT and is
/// not modeled here, mirroring TwinSnapshot's own SnapshotAtUtc/CreatedAtUtc
/// split (ADR-020 precedent).
/// </summary>
public sealed class AssetCondition : Entity<AssetConditionId>, IAggregateRoot
{
    private AssetCondition(
        AssetConditionId id, AssetId assetId, ConditionGradeId conditionGradeId, DateTime assessedAtUtc,
        int? assessedByUserId, decimal? healthScorePercent, int? remainingUsefulLifeDays, string? basis, string? notes)
        : base(id)
    {
        AssetId = assetId;
        ConditionGradeId = conditionGradeId;
        AssessedAtUtc = assessedAtUtc;
        AssessedByUserId = assessedByUserId;
        HealthScorePercent = healthScorePercent;
        RemainingUsefulLifeDays = remainingUsefulLifeDays;
        Basis = basis;
        Notes = notes;
    }

    public AssetId AssetId { get; }

    public ConditionGradeId ConditionGradeId { get; }

    public DateTime AssessedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser (ADR-021).</summary>
    public int? AssessedByUserId { get; }

    public decimal? HealthScorePercent { get; }

    public int? RemainingUsefulLifeDays { get; }

    public string? Basis { get; }

    public string? Notes { get; }

    public static AssetCondition Create(
        AssetConditionId id, AssetId assetId, ConditionGradeId conditionGradeId, DateTime assessedAtUtc,
        int? assessedByUserId = null, decimal? healthScorePercent = null, int? remainingUsefulLifeDays = null,
        string? basis = null, string? notes = null)
    {
        if (healthScorePercent is < 0 or > 100)
        {
            throw new ArgumentException(
                "HealthScorePercent must be between 0 and 100 when set (CK_Maintenance_AssetCondition_HealthScore).",
                nameof(healthScorePercent));
        }

        return new AssetCondition(
            id, assetId, conditionGradeId, assessedAtUtc, assessedByUserId, healthScorePercent,
            remainingUsefulLifeDays, basis, notes);
    }
}
