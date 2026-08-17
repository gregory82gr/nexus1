using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

/// <summary>DegradationRecord's real open/close lifecycle (ADR-021): Create starts IsActive = true / ClosedAtUtc = null; Close sets IsActive = false and ClosedAtUtc.</summary>
public class DegradationRecordTests
{
    private static readonly DateTime DetectedAtUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ClosedAtUtc = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_starts_active_with_no_closed_at()
    {
        var record = DegradationRecord.Create(
            new DegradationRecordId(1), new AssetId(1), new DegradationMechanismId(1), new FindingSeverityId(1),
            DetectedAtUtc, "Localized pitting corrosion observed on the pump casing.");

        Assert.True(record.IsActive);
        Assert.Null(record.ClosedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_description_throws(string description)
    {
        Assert.Throws<ArgumentException>(() => DegradationRecord.Create(
            new DegradationRecordId(1), new AssetId(1), new DegradationMechanismId(1), new FindingSeverityId(1),
            DetectedAtUtc, description));
    }

    [Fact]
    public void Close_sets_inactive_and_records_closed_at()
    {
        var record = DegradationRecord.Create(
            new DegradationRecordId(1), new AssetId(1), new DegradationMechanismId(1), new FindingSeverityId(1),
            DetectedAtUtc, "Localized pitting corrosion observed on the pump casing.");

        record.Close(ClosedAtUtc);

        Assert.False(record.IsActive);
        Assert.Equal(ClosedAtUtc, record.ClosedAtUtc);
    }

    [Fact]
    public void Create_with_optional_asset_component_sets_the_real_internal_fk()
    {
        var record = DegradationRecord.Create(
            new DegradationRecordId(1), new AssetId(1), new DegradationMechanismId(1), new FindingSeverityId(1),
            DetectedAtUtc, "Seal degradation.", assetComponentId: new AssetComponentId(5));

        Assert.Equal(new AssetComponentId(5), record.AssetComponentId);
    }
}
