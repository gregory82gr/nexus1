using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// A personal dose reading tied to an assignment window (ADR-024) — a dose
/// reading cannot exist without an assignment, matching the atlas's NOT
/// NULL PersonDosimeterAssignmentId chain exactly.
/// PersonDosimeterAssignmentId/DoseTypeId/MeasurementQualityId are all real
/// internal FKs and NOT NULL. EngineeringUnitId is a real SQL FOREIGN KEY
/// to CorePlatform.EngineeringUnit via the CorePlatformEngineeringUnitReference
/// shadow-entity technique and NOT NULL.
///
/// DoseValue &gt;= 0 is enforced as a SQL CHECK constraint at the EF
/// configuration layer, not in Domain (ADR-024). No audit columns beyond
/// row-insertion bookkeeping.
/// </summary>
public sealed class PersonDoseReading : Entity<PersonDoseReadingId>, IAggregateRoot
{
    private PersonDoseReading(
        PersonDoseReadingId id, PersonDosimeterAssignmentId personDosimeterAssignmentId, DoseTypeId doseTypeId,
        int engineeringUnitId, MeasurementQualityId measurementQualityId, DateTime readingAtUtc, decimal doseValue,
        bool isFinal)
        : base(id)
    {
        PersonDosimeterAssignmentId = personDosimeterAssignmentId;
        DoseTypeId = doseTypeId;
        EngineeringUnitId = engineeringUnitId;
        MeasurementQualityId = measurementQualityId;
        ReadingAtUtc = readingAtUtc;
        DoseValue = doseValue;
        IsFinal = isFinal;
    }

    public PersonDosimeterAssignmentId PersonDosimeterAssignmentId { get; }

    public DoseTypeId DoseTypeId { get; }

    /// <summary>Real FK to CorePlatform.EngineeringUnit (ADR-024), NOT NULL.</summary>
    public int EngineeringUnitId { get; }

    public MeasurementQualityId MeasurementQualityId { get; }

    public DateTime ReadingAtUtc { get; }

    public decimal DoseValue { get; }

    public bool IsFinal { get; }

    public static PersonDoseReading Create(
        PersonDoseReadingId id, PersonDosimeterAssignmentId personDosimeterAssignmentId, DoseTypeId doseTypeId,
        int engineeringUnitId, MeasurementQualityId measurementQualityId, DateTime readingAtUtc, decimal doseValue,
        bool isFinal = false)
    {
        return new PersonDoseReading(
            id, personDosimeterAssignmentId, doseTypeId, engineeringUnitId, measurementQualityId, readingAtUtc,
            doseValue, isFinal);
    }
}
