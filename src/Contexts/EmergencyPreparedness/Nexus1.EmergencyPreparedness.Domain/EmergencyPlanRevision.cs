using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// A single revision of an EmergencyPlan (ADR-025). EmergencyPlanId and
/// PlanStatusId are real internal FKs, NOT NULL. RevisionNumber is unique
/// together with EmergencyPlanId (enforced at the EF configuration layer).
///
/// PreparedByUserId/ApprovedByUserId are deliberately downgraded to plain
/// passport ints, no enforced FK — Security.ApplicationUser lives in
/// SecurityDb, a different physical database than AlarmManagementDb
/// (ADR-025).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only, same treatment as EmergencyPlan.
/// </summary>
public sealed class EmergencyPlanRevision : Entity<EmergencyPlanRevisionId>, IAggregateRoot
{
    private EmergencyPlanRevision(
        EmergencyPlanRevisionId id, EmergencyPlanId emergencyPlanId, int revisionNumber, string title,
        PlanStatusId planStatusId, int preparedByUserId, DateTime preparedAtUtc, int? approvedByUserId,
        DateTime? approvedAtUtc, string? documentUri, string? changeSummary)
        : base(id)
    {
        EmergencyPlanId = emergencyPlanId;
        RevisionNumber = revisionNumber;
        Title = title;
        PlanStatusId = planStatusId;
        PreparedByUserId = preparedByUserId;
        PreparedAtUtc = preparedAtUtc;
        ApprovedByUserId = approvedByUserId;
        ApprovedAtUtc = approvedAtUtc;
        DocumentUri = documentUri;
        ChangeSummary = changeSummary;
    }

    public EmergencyPlanId EmergencyPlanId { get; }

    public int RevisionNumber { get; }

    public string Title { get; }

    public PlanStatusId PlanStatusId { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-025).</summary>
    public int PreparedByUserId { get; }

    public DateTime PreparedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-025).</summary>
    public int? ApprovedByUserId { get; }

    public DateTime? ApprovedAtUtc { get; }

    public string? DocumentUri { get; }

    public string? ChangeSummary { get; }

    public static EmergencyPlanRevision Create(
        EmergencyPlanRevisionId id, EmergencyPlanId emergencyPlanId, int revisionNumber, string title,
        PlanStatusId planStatusId, int preparedByUserId, DateTime preparedAtUtc, int? approvedByUserId = null,
        DateTime? approvedAtUtc = null, string? documentUri = null, string? changeSummary = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("EmergencyPlanRevision title must not be empty.", nameof(title));
        }

        return new EmergencyPlanRevision(
            id, emergencyPlanId, revisionNumber, title, planStatusId, preparedByUserId, preparedAtUtc,
            approvedByUserId, approvedAtUtc, documentUri, changeSummary);
    }
}
