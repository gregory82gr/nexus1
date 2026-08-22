using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class DosimeterTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_null()
    {
        var dosimeter = Dosimeter.Create(new DosimeterId(1), new DosimeterTypeId(1), new DosimeterStatusId(1), "DOS-001");

        Assert.Equal("DOS-001", dosimeter.Code);
        Assert.Null(dosimeter.SerialNumber);
        Assert.Null(dosimeter.CalibrationDueAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Dosimeter.Create(new DosimeterId(1), new DosimeterTypeId(1), new DosimeterStatusId(1), code));
    }

    [Fact]
    public void Create_with_serial_number_and_calibration_due_date_sets_both()
    {
        var calibrationDueAtUtc = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var dosimeter = Dosimeter.Create(
            new DosimeterId(1), new DosimeterTypeId(1), new DosimeterStatusId(1), "DOS-001",
            serialNumber: "SN-500", calibrationDueAtUtc: calibrationDueAtUtc);

        Assert.Equal("SN-500", dosimeter.SerialNumber);
        Assert.Equal(calibrationDueAtUtc, dosimeter.CalibrationDueAtUtc);
    }
}
