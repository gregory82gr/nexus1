namespace Nexus1.ReinforcementLearning.Application;

/// <summary>
/// No "current/active policy" concept exists anywhere in the domain model
/// (Policy carries no IsCurrent flag, PolicyStatus is a generic lookup with
/// no enforced single-active-row rule) — this finder's own definition of
/// "active" is a judgment call, not a recovered fact: the most recently
/// extracted Policy whose source QTable is IsFinal (the one real,
/// documented invariant this domain does support — atlas C.11.5.2 query 2's
/// own "a final Q-table should contain 175 state-action values"). Added for
/// this slice; nothing upstream relied on there being a single answer to
/// "which policy is current" before now.
/// </summary>
public interface IActivePolicyFinder
{
    Task<int?> GetActivePolicyIdAsync(CancellationToken cancellationToken);
}
