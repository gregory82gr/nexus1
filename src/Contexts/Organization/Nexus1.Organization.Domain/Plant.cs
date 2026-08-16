using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// A plant within a site — not a reactor unit. It is the organizational
/// plant container under a physical site; ReactorFleet.Unit will later
/// carry its passport to this table through PlantId (atlas C.3.1 design
/// choice, C.3.4.3). That wiring is not performed by this ADR (ADR-017).
/// </summary>
public sealed class Plant : Entity<PlantId>, IAggregateRoot
{
    private Plant(
        PlantId id, SiteId siteId, PlantTypeId plantTypeId, string code, string name, string? description,
        DateOnly? operationalStartDate, bool isOperational, DateTime createdAtUtc)
        : base(id)
    {
        SiteId = siteId;
        PlantTypeId = plantTypeId;
        Code = code;
        Name = name;
        Description = description;
        OperationalStartDate = operationalStartDate;
        IsOperational = isOperational;
        CreatedAtUtc = createdAtUtc;
    }

    public SiteId SiteId { get; }

    public PlantTypeId PlantTypeId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public DateOnly? OperationalStartDate { get; }

    public bool IsOperational { get; }

    public DateTime CreatedAtUtc { get; }

    public static Plant Create(
        PlantId id, SiteId siteId, PlantTypeId plantTypeId, string code, string name, DateTime createdAtUtc,
        string? description = null, DateOnly? operationalStartDate = null, bool isOperational = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Plant code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Plant name must not be empty.", nameof(name));
        }

        return new Plant(id, siteId, plantTypeId, code, name, description, operationalStartDate, isOperational, createdAtUtc);
    }
}
