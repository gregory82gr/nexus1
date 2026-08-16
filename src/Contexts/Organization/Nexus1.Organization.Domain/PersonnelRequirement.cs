using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Baseline staffing rule: required count for a site, plant, department, or position (atlas C.3.4.9).</summary>
public sealed class PersonnelRequirement : Entity<PersonnelRequirementId>, IAggregateRoot
{
    private PersonnelRequirement(
        PersonnelRequirementId id, SiteId siteId, PlantId? plantId, DepartmentId? departmentId, PositionId positionId,
        int minRequiredCount, QualificationId? requiredQualificationId, bool isSafetyCritical, DateTime validFromUtc,
        DateTime? validToUtc, DateTime createdAtUtc)
        : base(id)
    {
        SiteId = siteId;
        PlantId = plantId;
        DepartmentId = departmentId;
        PositionId = positionId;
        MinRequiredCount = minRequiredCount;
        RequiredQualificationId = requiredQualificationId;
        IsSafetyCritical = isSafetyCritical;
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public SiteId SiteId { get; }

    public PlantId? PlantId { get; }

    public DepartmentId? DepartmentId { get; }

    public PositionId PositionId { get; }

    public int MinRequiredCount { get; }

    public QualificationId? RequiredQualificationId { get; }

    public bool IsSafetyCritical { get; }

    public DateTime ValidFromUtc { get; }

    public DateTime? ValidToUtc { get; }

    public DateTime CreatedAtUtc { get; }

    public static PersonnelRequirement Create(
        PersonnelRequirementId id, SiteId siteId, PositionId positionId, int minRequiredCount, DateTime validFromUtc,
        DateTime createdAtUtc, PlantId? plantId = null, DepartmentId? departmentId = null,
        QualificationId? requiredQualificationId = null, bool isSafetyCritical = false, DateTime? validToUtc = null)
    {
        if (minRequiredCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minRequiredCount), minRequiredCount, "MinRequiredCount must be >= 0.");
        }

        if (validToUtc is { } validTo && validTo <= validFromUtc)
        {
            throw new ArgumentException("ValidToUtc must be later than ValidFromUtc when present.", nameof(validToUtc));
        }

        return new PersonnelRequirement(
            id, siteId, plantId, departmentId, positionId, minRequiredCount, requiredQualificationId, isSafetyCritical,
            validFromUtc, validToUtc, createdAtUtc);
    }
}
