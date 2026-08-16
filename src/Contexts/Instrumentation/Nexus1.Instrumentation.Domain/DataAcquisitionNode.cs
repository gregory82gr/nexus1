using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>
/// Logical acquisition node, gateway, simulator feed or historian bridge for
/// one unit (atlas C.5.4.3). UnitId is a plain int, not a shared
/// ReactorFleet.Domain.UnitId type — Domain never references another
/// context's Domain project (dependency law); the real SQL FOREIGN KEY to
/// ReactorFleet.Unit is configured at the Infrastructure layer only
/// (ADR-019).
/// </summary>
public sealed class DataAcquisitionNode : Entity<DataAcquisitionNodeId>, IAggregateRoot
{
    private DataAcquisitionNode(
        DataAcquisitionNodeId id, int unitId, ChannelStatusId channelStatusId, string code, string name,
        string? hostName, string? networkZone, string? description, DateTime createdAtUtc)
        : base(id)
    {
        UnitId = unitId;
        ChannelStatusId = channelStatusId;
        Code = code;
        Name = name;
        HostName = hostName;
        NetworkZone = networkZone;
        Description = description;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>ReactorFleet.Unit real FK (ADR-019) — plain int at the Domain layer.</summary>
    public int UnitId { get; }

    public ChannelStatusId ChannelStatusId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? HostName { get; }

    public string? NetworkZone { get; }

    public string? Description { get; }

    public DateTime CreatedAtUtc { get; }

    public static DataAcquisitionNode Create(
        DataAcquisitionNodeId id, int unitId, ChannelStatusId channelStatusId, string code, string name,
        DateTime createdAtUtc, string? hostName = null, string? networkZone = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("DataAcquisitionNode code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DataAcquisitionNode name must not be empty.", nameof(name));
        }

        return new DataAcquisitionNode(id, unitId, channelStatusId, code, name, hostName, networkZone, description, createdAtUtc);
    }
}
