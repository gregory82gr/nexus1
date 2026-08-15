using Nexus1.Audit.Domain;

namespace Nexus1.Audit.Infrastructure.Persistence;

public sealed class AuditMutationRejectedException(IReadOnlyCollection<AuditEvidenceId> ids)
    : Exception($"Audit evidence is append-only; rejected attempted change to: {string.Join(", ", ids)}")
{
    public IReadOnlyCollection<AuditEvidenceId> Ids { get; } = ids;
}
