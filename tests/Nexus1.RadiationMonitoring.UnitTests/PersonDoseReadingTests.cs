using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class PersonDoseReadingTests
{
    private static readonly DateTime ReadingAtUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_is_final_false_by_default()
    {
        var reading = PersonDoseReading.Create(
            new PersonDoseReadingId(1), new PersonDosimeterAssignmentId(1), new DoseTypeId(1), 2,
            new MeasurementQualityId(1), ReadingAtUtc, 0.05m);

        Assert.Equal(0.05m, reading.DoseValue);
        Assert.False(reading.IsFinal);
    }

    [Fact]
    public void Create_with_engineering_unit_id_sets_it_with_no_enforced_fk_at_the_domain_layer()
    {
        var reading = PersonDoseReading.Create(
            new PersonDoseReadingId(1), new PersonDosimeterAssignmentId(1), new DoseTypeId(1), 2,
            new MeasurementQualityId(1), ReadingAtUtc, 0.05m, isFinal: true);

        Assert.Equal(2, reading.EngineeringUnitId);
        Assert.True(reading.IsFinal);
    }
}
