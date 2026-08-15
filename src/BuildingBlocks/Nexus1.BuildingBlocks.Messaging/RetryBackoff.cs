using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// From_Services_To_Runtime Executable Assets 29-K/29-L, adopted exactly
/// (ADR-009). Delay is computed once from (PolicyId, MessageId, Attempt) via
/// a SHA-256 hash, not process-local Random — the same inputs always produce
/// the same due time, so a restarted dispatcher recomputes identically
/// rather than needing to persist the jittered value separately.
/// </summary>
public static class RetryBackoff
{
    public static TimeSpan ExponentialCap(RetryPolicy policy, int nextAttempt)
    {
        if (nextAttempt < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(nextAttempt), nextAttempt, "Attempt must be at least 1.");
        }

        var ticks = policy.InitialDelay.Ticks;
        for (var step = 1; step < nextAttempt; step++)
        {
            if (ticks >= policy.MaxDelay.Ticks / 2)
            {
                return policy.MaxDelay;
            }

            ticks *= 2;
        }

        return TimeSpan.FromTicks(Math.Min(ticks, policy.MaxDelay.Ticks));
    }

    public static TimeSpan EqualJitter(RetryPolicy policy, Guid messageId, int nextAttempt)
    {
        var cap = ExponentialCap(policy, nextAttempt);
        var floorPercent = 100 - policy.EqualJitterPercent;
        var floorTicks = cap.Ticks * floorPercent / 100;
        var spreadTicks = cap.Ticks - floorTicks;

        var input = Encoding.UTF8.GetBytes($"{policy.PolicyId}\n{messageId:D}\n{nextAttempt}");
        var hash = SHA256.HashData(input);
        var sample = BinaryPrimitives.ReadUInt64BigEndian(hash);
        var offset = spreadTicks == 0 ? 0 : (long)(sample % (ulong)(spreadTicks + 1));

        return TimeSpan.FromTicks(floorTicks + offset);
    }
}
