using System.Text.Json;
using System.Text.Json.Serialization;
using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.Explain;

/// <summary>
/// The H5 structured-output schema the served model must fill (ADR-033) -- a
/// fixed set of named slots, never free prose. Confidence is carried as a string
/// flagged illustrative, never a fabricated probability. The model may abstain
/// first-class (H8) by setting <see cref="Abstain"/>.
/// </summary>
public sealed class StructuredAnswer
{
    [JsonPropertyName("cause")]
    public string? Cause { get; set; }

    [JsonPropertyName("entities")]
    public List<string> Entities { get; set; } = [];

    [JsonPropertyName("claims")]
    public List<StructuredClaim> Claims { get; set; } = [];

    [JsonPropertyName("confidence")]
    public string? Confidence { get; set; }

    [JsonPropertyName("abstain")]
    public bool Abstain { get; set; }

    [JsonPropertyName("abstain_reason")]
    public string? AbstainReason { get; set; }
}

public sealed class StructuredClaim
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("chunkId")]
    public long ChunkId { get; set; }
}

/// <summary>
/// Parses the model's raw output into the H5 schema and maps it to an
/// <see cref="ExplainOutcome"/> (ADR-033). Pure and side-effect-free, so it is
/// unit-tested directly. A parse failure, an explicit abstention, or a missing
/// cause is a real abstention (H8) -- never a salvage attempt that guesses at
/// what the model "meant".
/// </summary>
public static class StructuredAnswerParser
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static ExplainOutcome Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return ExplainOutcome.Abstain("model returned empty output");
        }

        var json = ExtractJsonObject(raw);
        if (json is null)
        {
            return ExplainOutcome.Abstain("model output was not the H5 JSON schema");
        }

        StructuredAnswer? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<StructuredAnswer>(json, Options);
        }
        catch (JsonException)
        {
            return ExplainOutcome.Abstain("model output failed to parse as the H5 JSON schema");
        }

        if (parsed is null)
        {
            return ExplainOutcome.Abstain("model output parsed to null");
        }

        if (parsed.Abstain)
        {
            return ExplainOutcome.Abstain(parsed.AbstainReason is { Length: > 0 } r ? r : "model abstained");
        }

        if (string.IsNullOrWhiteSpace(parsed.Cause))
        {
            return ExplainOutcome.Abstain("model named no cause");
        }

        var claims = parsed.Claims
            .Where(c => !string.IsNullOrWhiteSpace(c.Text))
            .Select(c => new Claim(c.Text!, c.ChunkId))
            .ToList();

        var draft = new DraftAnswer(parsed.Cause!, parsed.Entities, claims);
        return ExplainOutcome.Answer(draft);
    }

    /// <summary>Extracts the first balanced top-level JSON object, tolerating code fences or surrounding prose.</summary>
    private static string? ExtractJsonObject(string raw)
    {
        var start = raw.IndexOf('{');
        if (start < 0)
        {
            return null;
        }

        var depth = 0;
        var inString = false;
        var escaped = false;
        for (var i = start; i < raw.Length; i++)
        {
            var ch = raw[i];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (ch == '\\')
                {
                    escaped = true;
                }
                else if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            switch (ch)
            {
                case '"':
                    inString = true;
                    break;
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return raw[start..(i + 1)];
                    }

                    break;
            }
        }

        return null;
    }
}
