using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// A regulatory/administrative dose threshold (ADR-024) — the
/// threshold-and-alert layer alongside DoseAlert. DoseTypeId/LimitTypeId
/// are real internal FKs and NOT NULL. EngineeringUnitId is a real SQL
/// FOREIGN KEY to CorePlatform.EngineeringUnit via the
/// CorePlatformEngineeringUnitReference shadow-entity technique and NOT
/// NULL.
///
/// LimitValue &gt;= 0 and PeriodDays &gt; 0 are enforced as SQL CHECK
/// constraints at the EF configuration layer, not in Domain (ADR-024).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only, same treatment as RadiationZone/RadiationMonitor/Dosimeter.
/// </summary>
public sealed class DoseLimit : Entity<DoseLimitId>, IAggregateRoot
{
    private DoseLimit(
        DoseLimitId id, DoseTypeId doseTypeId, LimitTypeId limitTypeId, int engineeringUnitId, string code,
        string name, decimal limitValue, int periodDays, bool isActive)
        : base(id)
    {
        DoseTypeId = doseTypeId;
        LimitTypeId = limitTypeId;
        EngineeringUnitId = engineeringUnitId;
        Code = code;
        Name = name;
        LimitValue = limitValue;
        PeriodDays = periodDays;
        IsActive = isActive;
    }

    public DoseTypeId DoseTypeId { get; }

    public LimitTypeId LimitTypeId { get; }

    /// <summary>Real FK to CorePlatform.EngineeringUnit (ADR-024), NOT NULL.</summary>
    public int EngineeringUnitId { get; }

    public string Code { get; }

    public string Name { get; }

    public decimal LimitValue { get; }

    public int PeriodDays { get; }

    public bool IsActive { get; }

    public static DoseLimit Create(
        DoseLimitId id, DoseTypeId doseTypeId, LimitTypeId limitTypeId, int engineeringUnitId, string code,
        string name, decimal limitValue, int periodDays, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("DoseLimit code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DoseLimit name must not be empty.", nameof(name));
        }

        return new DoseLimit(id, doseTypeId, limitTypeId, engineeringUnitId, code, name, limitValue, periodDays, isActive);
    }
}
