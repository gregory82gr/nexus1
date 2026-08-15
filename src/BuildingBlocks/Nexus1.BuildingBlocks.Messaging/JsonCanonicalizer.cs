using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// RFC 8785 (JSON Canonicalization Scheme) subset, scoped to the JSON
/// shapes this project's message payloads actually produce — strings,
/// integers, longs, booleans, nested objects/arrays. Does not implement
/// RFC 8785's ECMAScript-compatible double-formatting rules (§3.2.2.3):
/// no payload in this project uses floating-point numbers, so that
/// correctness-hardest area of the spec is out of scope by construction
/// (ADR-008 — two available community RFC 8785 packages were rejected as
/// too low-adoption/unverified for a routine every message depends on).
///
/// Implements: object keys sorted by UTF-16 code unit (§3.2.3), no
/// insignificant whitespace, standard JSON string escaping.
/// </summary>
public static class JsonCanonicalizer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Canonicalize(object value)
    {
        var node = JsonSerializer.SerializeToNode(value, value.GetType(), SerializerOptions);
        var canonical = CanonicalizeNode(node);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
        {
            if (canonical is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                canonical.WriteTo(writer);
            }
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static JsonNode? CanonicalizeNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                var sorted = new JsonObject();
                foreach (var key in obj.Select(kv => kv.Key).OrderBy(k => k, StringComparer.Ordinal))
                {
                    sorted[key] = CanonicalizeNode(obj[key]?.DeepClone());
                }

                return sorted;

            case JsonArray array:
                var newArray = new JsonArray();
                foreach (var item in array)
                {
                    newArray.Add(CanonicalizeNode(item?.DeepClone()));
                }

                return newArray;

            default:
                return node?.DeepClone();
        }
    }
}
