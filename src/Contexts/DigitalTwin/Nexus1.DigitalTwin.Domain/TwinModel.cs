using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Names the physical unit mirrored by the twin and declares its model
/// type, status and fidelity (atlas C.6.1/C.6.2, C.6.8 query 1's own
/// subject). UnitId is a plain int, not a shared type from ReactorFleet's
/// Domain — Domain never references another context's Domain (dependency
/// law). It gets a real SQL FOREIGN KEY at the Infrastructure layer to
/// ReactorFleet.Unit via the ReactorFleetUnitReference shadow-entity
/// technique (ADR-020, following ADR-019's precedent).
///
/// Audit columns (CreatedBy, ModifiedAtUtc, ModifiedBy, RowVersion) are not
/// modeled in Domain, same restraint as every prior sector — only
/// CreatedAtUtc is domain-visible. IsDeleted IS modeled here despite that
/// restraint: C.6.8 query 1 (GetActiveTwinsForFleetQuery) filters on
/// "IsDeleted = 0" as its own defining behavior, so this is real business
/// data the Application layer needs, not pure bookkeeping.
/// </summary>
public sealed class TwinModel : Entity<TwinModelId>, IAggregateRoot
{
    private TwinModel(
        TwinModelId id, int unitId, TwinModelTypeId twinModelTypeId, TwinModelStatusId twinModelStatusId,
        TwinFidelityLevelId twinFidelityLevelId, string code, string name, string? description, string? modelOwner,
        bool isAuthoritative, bool isDeleted, DateTime createdAtUtc)
        : base(id)
    {
        UnitId = unitId;
        TwinModelTypeId = twinModelTypeId;
        TwinModelStatusId = twinModelStatusId;
        TwinFidelityLevelId = twinFidelityLevelId;
        Code = code;
        Name = name;
        Description = description;
        ModelOwner = modelOwner;
        IsAuthoritative = isAuthoritative;
        IsDeleted = isDeleted;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>ReactorFleet.Unit real FK (ADR-020).</summary>
    public int UnitId { get; }

    public TwinModelTypeId TwinModelTypeId { get; }

    public TwinModelStatusId TwinModelStatusId { get; }

    public TwinFidelityLevelId TwinFidelityLevelId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public string? ModelOwner { get; }

    public bool IsAuthoritative { get; }

    public bool IsDeleted { get; }

    public DateTime CreatedAtUtc { get; }

    public static TwinModel Create(
        TwinModelId id, int unitId, TwinModelTypeId twinModelTypeId, TwinModelStatusId twinModelStatusId,
        TwinFidelityLevelId twinFidelityLevelId, string code, string name, DateTime createdAtUtc,
        string? description = null, string? modelOwner = null, bool isAuthoritative = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("TwinModel code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("TwinModel name must not be empty.", nameof(name));
        }

        return new TwinModel(
            id, unitId, twinModelTypeId, twinModelStatusId, twinFidelityLevelId, code, name, description, modelOwner,
            isAuthoritative, isDeleted: false, createdAtUtc);
    }
}
