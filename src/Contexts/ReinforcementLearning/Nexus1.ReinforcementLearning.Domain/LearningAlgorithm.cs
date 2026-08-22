using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>Classifies TrainingRun's learning algorithm, e.g. Q-learning (atlas C.11.2). ModifiedAtUtc/RowVersion are EF-only shadow properties, not Domain-modeled (ADR-026).</summary>
public sealed class LearningAlgorithm : Entity<LearningAlgorithmId>, IAggregateRoot
{
    private LearningAlgorithm(LearningAlgorithmId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public int DisplayOrder { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public static LearningAlgorithm Create(
        LearningAlgorithmId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("LearningAlgorithm code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("LearningAlgorithm name must not be empty.", nameof(name));
        }

        return new LearningAlgorithm(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
