using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// Anchors training to a real ReactorFleet.Unit and optionally a
/// DigitalTwin.TwinModel — "you train against a fast point-kinetics
/// surrogate, never the real plant" (ADR-026, From_Trial_to_Policy Ch.3).
/// UnitId/TwinModelId are plain ints, not shared types from another
/// context's Domain — Domain never references transport, persistence, or
/// another context's Domain (dependency law). They get real SQL FOREIGN
/// KEYs at the Infrastructure layer via the ReactorFleetUnitReference and
/// DigitalTwinTwinModelReference shadow-entity technique.
///
/// Full audit shape (CreatedAtUtc/CreatedBy/ModifiedAtUtc/ModifiedBy/
/// IsDeleted/RowVersion) is NOT modeled in Domain — EF shadow properties
/// only, same restraint as every prior sector's lookup/substantive tables
/// carrying that shape. Real invariant: TimeStepSeconds must be greater
/// than zero (CK_ReinforcementLearning_EnvironmentModel_TimeStepSeconds).
/// </summary>
public sealed class EnvironmentModel : Entity<EnvironmentModelId>, IAggregateRoot
{
    private EnvironmentModel(
        EnvironmentModelId id, EnvironmentModelTypeId environmentModelTypeId, int unitId, int? twinModelId,
        string code, string name, string? description, string versionLabel, decimal timeStepSeconds,
        bool isDeterministic, int? randomSeed)
        : base(id)
    {
        EnvironmentModelTypeId = environmentModelTypeId;
        UnitId = unitId;
        TwinModelId = twinModelId;
        Code = code;
        Name = name;
        Description = description;
        VersionLabel = versionLabel;
        TimeStepSeconds = timeStepSeconds;
        IsDeterministic = isDeterministic;
        RandomSeed = randomSeed;
    }

    public EnvironmentModelTypeId EnvironmentModelTypeId { get; }

    /// <summary>ReactorFleet.Unit real FK (ADR-026).</summary>
    public int UnitId { get; }

    /// <summary>DigitalTwin.TwinModel real FK, nullable (ADR-026).</summary>
    public int? TwinModelId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public string VersionLabel { get; }

    public decimal TimeStepSeconds { get; }

    public bool IsDeterministic { get; }

    public int? RandomSeed { get; }

    public static EnvironmentModel Create(
        EnvironmentModelId id, EnvironmentModelTypeId environmentModelTypeId, int unitId, string code, string name,
        string versionLabel, decimal timeStepSeconds, int? twinModelId = null, string? description = null,
        bool isDeterministic = true, int? randomSeed = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EnvironmentModel code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EnvironmentModel name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(versionLabel))
        {
            throw new ArgumentException("EnvironmentModel versionLabel must not be empty.", nameof(versionLabel));
        }

        if (timeStepSeconds <= 0)
        {
            throw new ArgumentException(
                "TimeStepSeconds must be greater than zero (CK_ReinforcementLearning_EnvironmentModel_TimeStepSeconds).",
                nameof(timeStepSeconds));
        }

        return new EnvironmentModel(
            id, environmentModelTypeId, unitId, twinModelId, code, name, description, versionLabel, timeStepSeconds,
            isDeterministic, randomSeed);
    }
}
