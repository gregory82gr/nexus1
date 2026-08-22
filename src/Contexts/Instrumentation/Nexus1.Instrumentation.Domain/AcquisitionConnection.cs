using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>Named protocol connection or local feed into an acquisition node (atlas C.5.4.3).</summary>
public sealed class AcquisitionConnection : Entity<AcquisitionConnectionId>, IAggregateRoot
{
    private AcquisitionConnection(
        AcquisitionConnectionId id, DataAcquisitionNodeId dataAcquisitionNodeId, ChannelStatusId channelStatusId,
        string code, string protocol, string? endpoint, int? pollIntervalMs, bool isReadOnly, DateTime createdAtUtc)
        : base(id)
    {
        DataAcquisitionNodeId = dataAcquisitionNodeId;
        ChannelStatusId = channelStatusId;
        Code = code;
        Protocol = protocol;
        Endpoint = endpoint;
        PollIntervalMs = pollIntervalMs;
        IsReadOnly = isReadOnly;
        CreatedAtUtc = createdAtUtc;
    }

    public DataAcquisitionNodeId DataAcquisitionNodeId { get; }

    public ChannelStatusId ChannelStatusId { get; }

    public string Code { get; }

    public string Protocol { get; }

    public string? Endpoint { get; }

    public int? PollIntervalMs { get; }

    public bool IsReadOnly { get; }

    public DateTime CreatedAtUtc { get; }

    public static AcquisitionConnection Create(
        AcquisitionConnectionId id, DataAcquisitionNodeId dataAcquisitionNodeId, ChannelStatusId channelStatusId,
        string code, string protocol, DateTime createdAtUtc, string? endpoint = null, int? pollIntervalMs = null,
        bool isReadOnly = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("AcquisitionConnection code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(protocol))
        {
            throw new ArgumentException("AcquisitionConnection protocol must not be empty.", nameof(protocol));
        }

        if (pollIntervalMs is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pollIntervalMs), pollIntervalMs, "PollIntervalMs must be greater than zero when set.");
        }

        return new AcquisitionConnection(id, dataAcquisitionNodeId, channelStatusId, code, protocol, endpoint, pollIntervalMs, isReadOnly, createdAtUtc);
    }
}
