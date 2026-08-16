using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Unit-of-measure passport for the whole platform (atlas C.1.4.8):
/// instrumentation signals, alarm thresholds, model variables, and reports
/// reference this table instead of storing free-text symbols. Also
/// From_Domain_to_Twin's own bounded-context naming-collision teaching
/// example (pp. 24, 45): CorePlatform.EngineeringUnit is not
/// ReactorFleet.Unit.
/// </summary>
public sealed class EngineeringUnit : Entity<EngineeringUnitId>, IAggregateRoot
{
    private EngineeringUnit(
        EngineeringUnitId id, string symbol, string name, EngineeringQuantityType quantityType,
        decimal? siConversionFactor, decimal? siConversionOffset, bool isDimensionless, bool isActive,
        int displayOrder, DateTime createdAtUtc)
        : base(id)
    {
        Symbol = symbol;
        Name = name;
        QuantityType = quantityType;
        SiConversionFactor = siConversionFactor;
        SiConversionOffset = siConversionOffset;
        IsDimensionless = isDimensionless;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }

    public string Symbol { get; }

    public string Name { get; }

    public EngineeringQuantityType QuantityType { get; }

    public decimal? SiConversionFactor { get; }

    public decimal? SiConversionOffset { get; }

    public bool IsDimensionless { get; }

    public bool IsActive { get; }

    public int DisplayOrder { get; }

    public DateTime CreatedAtUtc { get; }

    public static EngineeringUnit Create(
        EngineeringUnitId id, string symbol, string name, EngineeringQuantityType quantityType, DateTime createdAtUtc,
        decimal? siConversionFactor = null, decimal? siConversionOffset = null, bool isDimensionless = false,
        bool isActive = true, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("EngineeringUnit symbol must not be empty.", nameof(symbol));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EngineeringUnit name must not be empty.", nameof(name));
        }

        return new EngineeringUnit(
            id, symbol, name, quantityType, siConversionFactor, siConversionOffset, isDimensionless,
            isActive, displayOrder, createdAtUtc);
    }
}
