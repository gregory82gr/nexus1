namespace Nexus1.AlarmManagement.Domain;

public sealed record AlarmAcknowledged(AlarmEventId AlarmEventId, UserId AcknowledgedBy, DateTime AcknowledgedAtUtc);
