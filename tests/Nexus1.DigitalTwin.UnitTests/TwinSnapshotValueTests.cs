using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class TwinSnapshotValueTests
{
    [Fact]
    public void Create_with_numeric_value_succeeds()
    {
        var value = TwinSnapshotValue.Create(new TwinSnapshotValueId(1), new TwinSnapshotId(1), new TwinVariableId(1), numericValue: 87.5);
        Assert.Equal(87.5, value.NumericValue);
    }

    [Fact]
    public void Create_with_text_value_succeeds()
    {
        var value = TwinSnapshotValue.Create(new TwinSnapshotValueId(1), new TwinSnapshotId(1), new TwinVariableId(1), textValue: "NOMINAL");
        Assert.Equal("NOMINAL", value.TextValue);
    }

    [Fact]
    public void Create_with_json_value_succeeds()
    {
        var value = TwinSnapshotValue.Create(new TwinSnapshotValueId(1), new TwinSnapshotId(1), new TwinVariableId(1), jsonValue: "{}");
        Assert.Equal("{}", value.JsonValue);
    }

    [Fact]
    public void Create_with_no_value_throws()
    {
        Assert.Throws<ArgumentException>(() => TwinSnapshotValue.Create(new TwinSnapshotValueId(1), new TwinSnapshotId(1), new TwinVariableId(1)));
    }
}
