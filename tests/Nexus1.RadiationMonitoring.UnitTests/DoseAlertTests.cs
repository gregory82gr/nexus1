using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class DoseAlertTests
{
    private static readonly DateTime AlertAtUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_null()
    {
        var alert = DoseAlert.Create(
            new DoseAlertId(1), new DoseLimitId(1), new AlertStatusId(1), AlertAtUtc, "Annual dose limit exceeded");

        Assert.Equal("Annual dose limit exceeded", alert.Message);
        Assert.Null(alert.PersonDoseReadingId);
        Assert.Null(alert.AcknowledgedByUserId);
        Assert.Null(alert.AcknowledgedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_message_throws(string message)
    {
        Assert.Throws<ArgumentException>(() => DoseAlert.Create(
            new DoseAlertId(1), new DoseLimitId(1), new AlertStatusId(1), AlertAtUtc, message));
    }

    [Fact]
    public void Create_with_passport_only_acknowledged_by_user_id_sets_it_with_no_enforced_fk()
    {
        var acknowledgedAtUtc = AlertAtUtc.AddHours(1);

        var alert = DoseAlert.Create(
            new DoseAlertId(1), new DoseLimitId(1), new AlertStatusId(2), AlertAtUtc, "Annual dose limit exceeded",
            personDoseReadingId: new PersonDoseReadingId(1), acknowledgedByUserId: 7,
            acknowledgedAtUtc: acknowledgedAtUtc);

        Assert.Equal(new PersonDoseReadingId(1), alert.PersonDoseReadingId);
        Assert.Equal(7, alert.AcknowledgedByUserId);
        Assert.Equal(acknowledgedAtUtc, alert.AcknowledgedAtUtc);
    }
}
