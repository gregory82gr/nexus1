using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Variable catalogue for model state, inputs, outputs and diagnostics
/// (atlas C.6.2). Required by SignalBinding/TwinSnapshotValue/TwinDivergence.
/// Real invariant: LowerBound &lt;= UpperBound when both are set
/// (CK_DigitalTwin_TwinVariable_Bounds).
///
/// EngineeringUnitId is nullable per the atlas's own DDL and gets a real SQL
/// FOREIGN KEY at the Infrastructure layer to CorePlatform.EngineeringUnit
/// via the CorePlatformEngineeringUnitReference shadow-entity technique
/// (ADR-020, following ADR-019's precedent).
/// </summary>
public sealed class TwinVariable : Entity<TwinVariableId>, IAggregateRoot
{
    private TwinVariable(
        TwinVariableId id, TwinModelId twinModelId, ModelVariableTypeId modelVariableTypeId, int? engineeringUnitId,
        string code, string name, string? description, bool isStateVariable, bool isRequired, double? lowerBound,
        double? upperBound, DateTime createdAtUtc)
        : base(id)
    {
        TwinModelId = twinModelId;
        ModelVariableTypeId = modelVariableTypeId;
        EngineeringUnitId = engineeringUnitId;
        Code = code;
        Name = name;
        Description = description;
        IsStateVariable = isStateVariable;
        IsRequired = isRequired;
        LowerBound = lowerBound;
        UpperBound = upperBound;
        CreatedAtUtc = createdAtUtc;
    }

    public TwinModelId TwinModelId { get; }

    public ModelVariableTypeId ModelVariableTypeId { get; }

    /// <summary>CorePlatform.EngineeringUnit real FK, nullable (ADR-020).</summary>
    public int? EngineeringUnitId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public bool IsStateVariable { get; }

    public bool IsRequired { get; }

    public double? LowerBound { get; }

    public double? UpperBound { get; }

    public DateTime CreatedAtUtc { get; }

    public static TwinVariable Create(
        TwinVariableId id, TwinModelId twinModelId, ModelVariableTypeId modelVariableTypeId, string code, string name,
        DateTime createdAtUtc, int? engineeringUnitId = null, string? description = null, bool isStateVariable = false,
        bool isRequired = false, double? lowerBound = null, double? upperBound = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("TwinVariable code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("TwinVariable name must not be empty.", nameof(name));
        }

        if (lowerBound is not null && upperBound is not null && lowerBound > upperBound)
        {
            throw new ArgumentException(
                "LowerBound must be less than or equal to UpperBound when both are set (CK_DigitalTwin_TwinVariable_Bounds).",
                nameof(upperBound));
        }

        return new TwinVariable(
            id, twinModelId, modelVariableTypeId, engineeringUnitId, code, name, description, isStateVariable,
            isRequired, lowerBound, upperBound, createdAtUtc);
    }
}
