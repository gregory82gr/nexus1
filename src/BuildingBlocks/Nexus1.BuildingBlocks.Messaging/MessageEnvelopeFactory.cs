using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// Envelope shape frozen by From_Services_To_Runtime ch.32-34; two
/// separate hashes serve different purposes (ADR-008): EnvelopeSha256
/// verifies storage/transport integrity of the exact bytes about to be
/// published (the relay checks this before every attempt, per the book);
/// ContentFingerprint is the payload's own content-addressable identity —
/// a JCS-canonicalized hash of the business data alone, independent of
/// envelope metadata like MessageId.
/// </summary>
public sealed record EnvelopeContent(byte[] EnvelopeBytes, byte[] EnvelopeSha256, string ContentFingerprint);

public static class MessageEnvelopeFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static EnvelopeContent Build(
        Guid messageId,
        string eventType,
        int schemaVersion,
        DateTimeOffset occurredAtUtc,
        string producer,
        Guid correlationId,
        Guid? causationId,
        object payload)
    {
        var canonicalPayloadJson = JsonCanonicalizer.Canonicalize(payload);
        var payloadHash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayloadJson));
        var contentFingerprint = "sha256:" + Convert.ToHexString(payloadHash).ToLowerInvariant();

        var envelope = new
        {
            envelopeVersion = 1,
            messageId,
            eventType,
            schemaVersion,
            occurredAtUtc,
            producer,
            correlationId,
            causationId,
            contentType = "application/json",
            fingerprintAlgorithm = "sha-256+jcs",
            payload = JsonNode.Parse(canonicalPayloadJson),
            contentFingerprint,
        };

        var envelopeJson = JsonSerializer.Serialize(envelope, SerializerOptions);
        var envelopeBytes = Encoding.UTF8.GetBytes(envelopeJson);
        var envelopeSha256 = SHA256.HashData(envelopeBytes);

        return new EnvelopeContent(envelopeBytes, envelopeSha256, contentFingerprint);
    }
}
