using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RadiationZoneType_Create_with_valid_fields_succeeds()
    {
        var type = RadiationZoneType.Create(new RadiationZoneTypeId(1), "CONTROLLED", "Controlled Area", NowUtc);
        Assert.Equal("CONTROLLED", type.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RadiationZoneType_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => RadiationZoneType.Create(new RadiationZoneTypeId(1), code, "Controlled Area", NowUtc));
    }

    [Fact]
    public void RadiationZoneStatus_Create_with_valid_fields_succeeds()
    {
        var status = RadiationZoneStatus.Create(new RadiationZoneStatusId(1), "ACTIVE", "Active", NowUtc);
        Assert.Equal("ACTIVE", status.Code);
    }

    [Fact]
    public void RadiationAreaClassification_Create_with_valid_fields_succeeds()
    {
        var classification = RadiationAreaClassification.Create(new RadiationAreaClassificationId(1), "HIGH", "High Radiation Area", NowUtc);
        Assert.Equal("HIGH", classification.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RadiationAreaClassification_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => RadiationAreaClassification.Create(new RadiationAreaClassificationId(1), "HIGH", name, NowUtc));
    }

    [Fact]
    public void MonitorType_Create_with_valid_fields_succeeds()
    {
        var type = MonitorType.Create(new MonitorTypeId(1), "AREA_GAMMA", "Area Gamma Monitor", NowUtc);
        Assert.Equal("AREA_GAMMA", type.Code);
    }

    [Fact]
    public void MonitorStatus_Create_with_valid_fields_succeeds()
    {
        var status = MonitorStatus.Create(new MonitorStatusId(1), "IN_SERVICE", "In Service", NowUtc);
        Assert.Equal("IN_SERVICE", status.Code);
    }

    [Fact]
    public void MeasurementType_Create_with_valid_fields_succeeds()
    {
        var type = MeasurementType.Create(new MeasurementTypeId(1), "DOSE_RATE", "Dose Rate", NowUtc);
        Assert.Equal("DOSE_RATE", type.Code);
    }

    [Fact]
    public void MeasurementQuality_Create_with_valid_fields_succeeds()
    {
        var quality = MeasurementQuality.Create(new MeasurementQualityId(1), "VALID", "Valid", NowUtc);
        Assert.Equal("VALID", quality.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MeasurementQuality_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => MeasurementQuality.Create(new MeasurementQualityId(1), code, "Valid", NowUtc));
    }

    [Fact]
    public void DoseType_Create_with_valid_fields_succeeds()
    {
        var type = DoseType.Create(new DoseTypeId(1), "EFFECTIVE", "Effective Dose", NowUtc);
        Assert.Equal("EFFECTIVE", type.Code);
    }

    [Fact]
    public void DosimeterType_Create_with_valid_fields_succeeds()
    {
        var type = DosimeterType.Create(new DosimeterTypeId(1), "TLD", "Thermoluminescent Dosimeter", NowUtc);
        Assert.Equal("TLD", type.Code);
    }

    [Fact]
    public void DosimeterStatus_Create_with_valid_fields_succeeds()
    {
        var status = DosimeterStatus.Create(new DosimeterStatusId(1), "ISSUED", "Issued", NowUtc);
        Assert.Equal("ISSUED", status.Code);
    }

    [Fact]
    public void LimitType_Create_with_valid_fields_succeeds()
    {
        var type = LimitType.Create(new LimitTypeId(1), "ANNUAL", "Annual Limit", NowUtc);
        Assert.Equal("ANNUAL", type.Code);
    }

    [Fact]
    public void AlertStatus_Create_with_valid_fields_succeeds()
    {
        var status = AlertStatus.Create(new AlertStatusId(1), "OPEN", "Open", NowUtc);
        Assert.Equal("OPEN", status.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AlertStatus_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => AlertStatus.Create(new AlertStatusId(1), "OPEN", name, NowUtc));
    }
}
