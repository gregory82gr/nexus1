using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// Named what-if or stress-test staffing scenario (atlas C.3.4.9).
/// CreatedByUserId is a plain passport int, not a real FK: Security lives
/// in its own SecurityDb while Organization gets its own OrganizationDb
/// (ADR-017), so a real cross-database FOREIGN KEY is not possible.
/// </summary>
public sealed class StaffingScenario : Entity<StaffingScenarioId>, IAggregateRoot
{
    private StaffingScenario(
        StaffingScenarioId id, SiteId siteId, string scenarioCode, string name, string? description,
        int? createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        SiteId = siteId;
        ScenarioCode = scenarioCode;
        Name = name;
        Description = description;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    public SiteId SiteId { get; }

    public string ScenarioCode { get; }

    public string Name { get; }

    public string? Description { get; }

    /// <summary>Security.ApplicationUser passport id — no enforced FK (ADR-017).</summary>
    public int? CreatedByUserId { get; }

    public DateTime CreatedAtUtc { get; }

    public static StaffingScenario Create(
        StaffingScenarioId id, SiteId siteId, string scenarioCode, string name, DateTime createdAtUtc,
        string? description = null, int? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(scenarioCode))
        {
            throw new ArgumentException("StaffingScenario code must not be empty.", nameof(scenarioCode));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("StaffingScenario name must not be empty.", nameof(name));
        }

        return new StaffingScenario(id, siteId, scenarioCode, name, description, createdByUserId, createdAtUtc);
    }
}
