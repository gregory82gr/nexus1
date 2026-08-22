using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// A personal dosimetry device (ADR-024). DosimeterTypeId/DosimeterStatusId
/// are real internal FKs and NOT NULL. Full audit shape is NOT modeled in
/// Domain at all — EF shadow properties only, same treatment as
/// RadiationZone/RadiationMonitor.
/// </summary>
public sealed class Dosimeter : Entity<DosimeterId>, IAggregateRoot
{
    private Dosimeter(
        DosimeterId id, DosimeterTypeId dosimeterTypeId, DosimeterStatusId dosimeterStatusId, string code,
        string? serialNumber, DateTime? calibrationDueAtUtc)
        : base(id)
    {
        DosimeterTypeId = dosimeterTypeId;
        DosimeterStatusId = dosimeterStatusId;
        Code = code;
        SerialNumber = serialNumber;
        CalibrationDueAtUtc = calibrationDueAtUtc;
    }

    public DosimeterTypeId DosimeterTypeId { get; }

    public DosimeterStatusId DosimeterStatusId { get; }

    public string Code { get; }

    public string? SerialNumber { get; }

    public DateTime? CalibrationDueAtUtc { get; }

    public static Dosimeter Create(
        DosimeterId id, DosimeterTypeId dosimeterTypeId, DosimeterStatusId dosimeterStatusId, string code,
        string? serialNumber = null, DateTime? calibrationDueAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Dosimeter code must not be empty.", nameof(code));
        }

        return new Dosimeter(id, dosimeterTypeId, dosimeterStatusId, code, serialNumber, calibrationDueAtUtc);
    }
}
