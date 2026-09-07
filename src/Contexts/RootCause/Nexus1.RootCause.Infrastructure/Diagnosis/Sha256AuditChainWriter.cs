using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The audit seal of Listing 9.5 / H10 (ADR-032): every conclusion (or
/// abstention) is appended as Hash = SHA-256(PrevHash + Payload), lower-case
/// hex, chaining from the previous head. The first entry chains from a fixed
/// genesis hash, so any later tampering breaks verification from that point on.
/// The write is a single transaction that reads the current head and appends the
/// next link; the returned hash is the new head.
/// </summary>
public sealed class Sha256AuditChainWriter(RootCauseDbContext db, IDateTimeProvider clock) : IAuditChainWriter
{
    /// <summary>The chain's genesis PrevHash -- 64 zero hex digits, so the first real entry has a defined predecessor.</summary>
    public const string GenesisHash = "0000000000000000000000000000000000000000000000000000000000000000";

    public async Task<string> AppendAsync(string payload, CancellationToken cancellationToken)
    {
        var prevHash = await db.AuditEntries
            .OrderByDescending(a => a.Seq)
            .Select(a => a.Hash)
            .FirstOrDefaultAsync(cancellationToken) ?? GenesisHash;

        var hash = ComputeHash(prevHash, payload);

        db.AuditEntries.Add(new AuditEntry
        {
            TimestampUtc = clock.UtcNow,
            Payload = payload,
            PrevHash = prevHash,
            Hash = hash,
        });
        await db.SaveChangesAsync(cancellationToken);

        return hash;
    }

    /// <summary>Hash = SHA-256(PrevHash + Payload), lower-case hex (Listing 9.5).</summary>
    public static string ComputeHash(string prevHash, string payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(prevHash + payload))).ToLowerInvariant();
}
