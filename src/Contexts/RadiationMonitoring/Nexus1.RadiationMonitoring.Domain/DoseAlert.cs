using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// A computed threshold-breach verdict record (ADR-024), structurally the
/// closest analogue in this sector to Robotics.MissionReadinessAssessment —
/// evidence, not a command that changes anything by itself. DoseLimitId and
/// AlertStatusId are real internal FKs and NOT NULL. PersonDoseReadingId is
/// a real internal FK, nullable. AcknowledgedByUserId is passport-only —
/// Security.ApplicationUser, a different physical database. No audit
/// columns beyond row-insertion bookkeeping.
/// </summary>
public sealed class DoseAlert : Entity<DoseAlertId>, IAggregateRoot
{
    private DoseAlert(
        DoseAlertId id, DoseLimitId doseLimitId, PersonDoseReadingId? personDoseReadingId,
        AlertStatusId alertStatusId, int? acknowledgedByUserId, DateTime alertAtUtc, DateTime? acknowledgedAtUtc,
        string message)
        : base(id)
    {
        DoseLimitId = doseLimitId;
        PersonDoseReadingId = personDoseReadingId;
        AlertStatusId = alertStatusId;
        AcknowledgedByUserId = acknowledgedByUserId;
        AlertAtUtc = alertAtUtc;
        AcknowledgedAtUtc = acknowledgedAtUtc;
        Message = message;
    }

    public DoseLimitId DoseLimitId { get; }

    public PersonDoseReadingId? PersonDoseReadingId { get; }

    public AlertStatusId AlertStatusId { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-024).</summary>
    public int? AcknowledgedByUserId { get; }

    public DateTime AlertAtUtc { get; }

    public DateTime? AcknowledgedAtUtc { get; }

    public string Message { get; }

    public static DoseAlert Create(
        DoseAlertId id, DoseLimitId doseLimitId, AlertStatusId alertStatusId, DateTime alertAtUtc, string message,
        PersonDoseReadingId? personDoseReadingId = null, int? acknowledgedByUserId = null,
        DateTime? acknowledgedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("DoseAlert message must not be empty.", nameof(message));
        }

        return new DoseAlert(
            id, doseLimitId, personDoseReadingId, alertStatusId, acknowledgedByUserId, alertAtUtc,
            acknowledgedAtUtc, message);
    }
}
