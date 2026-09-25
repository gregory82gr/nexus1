using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nexus1.RootCause.Explain;

/// <summary>
/// Readiness check for the served-model dependency of the diagnosis route (ADR-044):
/// <c>GET {Endpoint}/api/tags</c> with a short timeout, Healthy only if BOTH configured models
/// (the chat model and the embedding model) are listed.
///
/// REACHABLE IS NOT WARM. <c>/api/tags</c> lists installed models and never loads one, so a
/// Healthy result proves the Ollama server answers and the models are present — NOT that a model
/// is resident in memory. A cold first diagnosis can still take 100-150 s. The Healthy
/// description says so, so nobody reads more into the green than it proves.
/// </summary>
public sealed class OllamaReachabilityHealthCheck(HttpClient http, OllamaOptions options, TimeSpan timeout) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        TagsResponse? tags;
        try
        {
            tags = await http.GetFromJsonAsync<TagsResponse>(new Uri(options.Endpoint, "/api/tags"), cts.Token);
        }
        catch (TaskCanceledException ex)
        {
            return HealthCheckResult.Unhealthy($"Ollama did not answer /api/tags within {timeout.TotalSeconds:0.#}s.", ex);
        }
        catch (HttpRequestException ex)
        {
            // Description is log-only: the /health/ready body is just the aggregate status.
            return HealthCheckResult.Unhealthy($"Ollama unreachable: {ex.Message}", ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return HealthCheckResult.Unhealthy("Ollama answered /api/tags with an unreadable response.", ex);
        }

        var installed = tags?.Models?.Select(m => m.Name ?? string.Empty).ToList() ?? [];
        var missing = new[] { options.ChatModelId, options.EmbeddingModelId }
            .Where(model => !installed.Any(name => IsModel(name, model)))
            .ToList();

        return missing.Count == 0
            ? HealthCheckResult.Healthy(
                $"Ollama reachable; models present: {options.ChatModelId}, {options.EmbeddingModelId}. "
                + "Reachability and model presence only -- NOT a warmth check: /api/tags never loads a model, "
                + "so a cold first diagnosis can still take 100-150 s.")
            : HealthCheckResult.Unhealthy($"Ollama reachable but model(s) not installed: {string.Join(", ", missing)}.");
    }

    // "nexus-dslm" matches "nexus-dslm" or "nexus-dslm:<tag>" -- never a different model sharing a prefix.
    private static bool IsModel(string installedName, string model) =>
        installedName.Equals(model, StringComparison.Ordinal) || installedName.StartsWith(model + ":", StringComparison.Ordinal);

    private sealed record TagsResponse([property: JsonPropertyName("models")] List<TagModel>? Models);

    private sealed record TagModel([property: JsonPropertyName("name")] string? Name);
}

public static class OllamaHealthChecksBuilderExtensions
{
    // A dedicated client for probes: the shared diagnosis HttpClient carries the 150 s inference
    // budget; a readiness probe must answer in seconds. One instance, reused across probes.
    private static readonly HttpClient ProbeClient = new();

    /// <summary>Adds the "ollama" readiness check (2 s timeout). RootCause.Host only — the diagnosis route is what needs it.</summary>
    public static IHealthChecksBuilder AddOllamaReachabilityCheck(this IHealthChecksBuilder builder, OllamaOptions options)
    {
        var timeout = TimeSpan.FromSeconds(2);
        return builder.Add(new HealthCheckRegistration(
            "ollama",
            _ => new OllamaReachabilityHealthCheck(ProbeClient, options, timeout),
            failureStatus: HealthStatus.Unhealthy,
            tags: null,
            // One second of headroom over the check's own timeout, so the check always reports its
            // own reason instead of the framework cancelling it first as an "unhandled exception".
            timeout: timeout + TimeSpan.FromSeconds(1)));
    }
}
