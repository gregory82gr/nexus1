using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// The top of the training-pipeline spine, anchoring one or more
/// TrainingRun rows to a real ReactorFleet.Unit (atlas C.11.2). OwnerUserId
/// is passport-only (Security.ApplicationUser lives in a different
/// physical database, SecurityDb, ADR-026). Full audit shape is NOT
/// modeled in Domain — EF shadow properties only.
/// </summary>
public sealed class Experiment : Entity<ExperimentId>, IAggregateRoot
{
    private Experiment(ExperimentId id, int unitId, string code, string name, string? objective, int? ownerUserId)
        : base(id)
    {
        UnitId = unitId;
        Code = code;
        Name = name;
        Objective = objective;
        OwnerUserId = ownerUserId;
    }

    /// <summary>ReactorFleet.Unit real FK (ADR-026).</summary>
    public int UnitId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Objective { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-026).</summary>
    public int? OwnerUserId { get; }

    public static Experiment Create(
        ExperimentId id, int unitId, string code, string name, string? objective = null, int? ownerUserId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Experiment code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Experiment name must not be empty.", nameof(name));
        }

        return new Experiment(id, unitId, code, name, objective, ownerUserId);
    }
}
