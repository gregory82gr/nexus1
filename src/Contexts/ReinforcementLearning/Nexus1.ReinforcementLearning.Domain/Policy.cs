using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// The readable extraction from a QTable — "the 35x5 policy should have
/// one entry per state" (atlas C.11.2, C.11.5.2 query 1's own subject).
/// Full audit shape is NOT modeled in Domain — EF shadow properties only.
/// </summary>
public sealed class Policy : Entity<PolicyId>, IAggregateRoot
{
    private Policy(
        PolicyId id, QTableId qTableId, PolicyStatusId policyStatusId, string code, string name,
        DateTime extractedAtUtc, int entryCount)
        : base(id)
    {
        QTableId = qTableId;
        PolicyStatusId = policyStatusId;
        Code = code;
        Name = name;
        ExtractedAtUtc = extractedAtUtc;
        EntryCount = entryCount;
    }

    public QTableId QTableId { get; }

    public PolicyStatusId PolicyStatusId { get; }

    public string Code { get; }

    public string Name { get; }

    public DateTime ExtractedAtUtc { get; }

    public int EntryCount { get; }

    public static Policy Create(
        PolicyId id, QTableId qTableId, PolicyStatusId policyStatusId, string code, string name,
        DateTime extractedAtUtc, int entryCount)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Policy code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Policy name must not be empty.", nameof(name));
        }

        return new Policy(id, qTableId, policyStatusId, code, name, extractedAtUtc, entryCount);
    }
}
