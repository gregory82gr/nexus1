using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// The discretized action space a TrainingRun learns over (atlas C.11.2).
/// EngineeringUnitId is a plain nullable int, not a shared type from
/// CorePlatform's Domain — Domain never references another context's
/// Domain (dependency law). It gets a real SQL FOREIGN KEY at the
/// Infrastructure layer via the CorePlatformEngineeringUnitReference
/// shadow-entity technique. Full audit shape is NOT modeled in Domain —
/// EF shadow properties only.
/// </summary>
public sealed class ActionSpace : Entity<ActionSpaceId>, IAggregateRoot
{
    private ActionSpace(
        ActionSpaceId id, ActionSpaceTypeId actionSpaceTypeId, int? engineeringUnitId, string code, string name,
        string? description)
        : base(id)
    {
        ActionSpaceTypeId = actionSpaceTypeId;
        EngineeringUnitId = engineeringUnitId;
        Code = code;
        Name = name;
        Description = description;
    }

    public ActionSpaceTypeId ActionSpaceTypeId { get; }

    /// <summary>CorePlatform.EngineeringUnit real FK, nullable (ADR-026).</summary>
    public int? EngineeringUnitId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public static ActionSpace Create(
        ActionSpaceId id, ActionSpaceTypeId actionSpaceTypeId, string code, string name,
        int? engineeringUnitId = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("ActionSpace code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("ActionSpace name must not be empty.", nameof(name));
        }

        return new ActionSpace(id, actionSpaceTypeId, engineeringUnitId, code, name, description);
    }
}
