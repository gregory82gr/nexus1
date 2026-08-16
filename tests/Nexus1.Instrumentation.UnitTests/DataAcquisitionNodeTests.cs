using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class DataAcquisitionNodeTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var node = DataAcquisitionNode.Create(
            new DataAcquisitionNodeId(1), unitId: 1, new ChannelStatusId(1), "NODE-1", "Node One", NowUtc);

        Assert.Equal(1, node.UnitId);
        Assert.Equal("NODE-1", node.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => DataAcquisitionNode.Create(
            new DataAcquisitionNodeId(1), unitId: 1, new ChannelStatusId(1), code, "Node One", NowUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => DataAcquisitionNode.Create(
            new DataAcquisitionNodeId(1), unitId: 1, new ChannelStatusId(1), "NODE-1", name, NowUtc));
    }
}
