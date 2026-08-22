using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>
/// Protocol address, register, topic, JSON path or synthetic point before it
/// becomes a signal (atlas C.5.4.3). No IsDeleted/audit columns — the atlas
/// DDL genuinely gives this table none, unlike DataAcquisitionNode/
/// AcquisitionConnection/Signal (verified against the atlas, ADR-019).
/// </summary>
public sealed class AcquisitionPoint : Entity<AcquisitionPointId>, IAggregateRoot
{
    private AcquisitionPoint(
        AcquisitionPointId id, AcquisitionConnectionId acquisitionConnectionId, string code, string rawAddress,
        string? rawDataType, decimal? scaleFactor, decimal? offsetValue, DateTime createdAtUtc)
        : base(id)
    {
        AcquisitionConnectionId = acquisitionConnectionId;
        Code = code;
        RawAddress = rawAddress;
        RawDataType = rawDataType;
        ScaleFactor = scaleFactor;
        OffsetValue = offsetValue;
        CreatedAtUtc = createdAtUtc;
    }

    public AcquisitionConnectionId AcquisitionConnectionId { get; }

    public string Code { get; }

    public string RawAddress { get; }

    public string? RawDataType { get; }

    public decimal? ScaleFactor { get; }

    public decimal? OffsetValue { get; }

    public DateTime CreatedAtUtc { get; }

    public static AcquisitionPoint Create(
        AcquisitionPointId id, AcquisitionConnectionId acquisitionConnectionId, string code, string rawAddress,
        DateTime createdAtUtc, string? rawDataType = null, decimal? scaleFactor = null, decimal? offsetValue = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("AcquisitionPoint code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(rawAddress))
        {
            throw new ArgumentException("AcquisitionPoint RawAddress must not be empty.", nameof(rawAddress));
        }

        return new AcquisitionPoint(id, acquisitionConnectionId, code, rawAddress, rawDataType, scaleFactor, offsetValue, createdAtUtc);
    }
}
