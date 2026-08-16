using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class AcquisitionConnectionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_without_poll_interval_succeeds()
    {
        var connection = AcquisitionConnection.Create(
            new AcquisitionConnectionId(1), new DataAcquisitionNodeId(1), new ChannelStatusId(1), "CONN-1", "OPC-UA", NowUtc);

        Assert.Null(connection.PollIntervalMs);
        Assert.True(connection.IsReadOnly);
    }

    [Fact]
    public void Create_with_positive_poll_interval_succeeds()
    {
        var connection = AcquisitionConnection.Create(
            new AcquisitionConnectionId(1), new DataAcquisitionNodeId(1), new ChannelStatusId(1), "CONN-1", "OPC-UA", NowUtc,
            pollIntervalMs: 500);

        Assert.Equal(500, connection.PollIntervalMs);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_with_non_positive_poll_interval_throws(int pollIntervalMs)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AcquisitionConnection.Create(
            new AcquisitionConnectionId(1), new DataAcquisitionNodeId(1), new ChannelStatusId(1), "CONN-1", "OPC-UA", NowUtc,
            pollIntervalMs: pollIntervalMs));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_protocol_throws(string protocol)
    {
        Assert.Throws<ArgumentException>(() => AcquisitionConnection.Create(
            new AcquisitionConnectionId(1), new DataAcquisitionNodeId(1), new ChannelStatusId(1), "CONN-1", protocol, NowUtc));
    }
}
