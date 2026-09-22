using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.Explain;

/// <summary>
/// Connection settings for the local, loopback-only Ollama runtime (ADR-033).
/// The chat model is the pinned <c>nexus-dslm</c> Modelfile build; the embedding
/// model is <c>nomic-embed-text</c>. The endpoint is loopback by default (H7 /
/// air-gap): the served model is never reached over a network.
/// </summary>
public sealed record OllamaOptions
{
    public Uri Endpoint { get; init; } = new("http://127.0.0.1:11434");

    public string ChatModelId { get; init; } = "nexus-dslm";

    public string EmbeddingModelId { get; init; } = "nomic-embed-text";

    /// <summary>
    /// Timeout for the chat-model call (ADR-033 addendum). Default 150s -- above the
    /// observed 93-110s cold qwen2.5:3b CPU inference, and below the BFF's 180s outer
    /// bound (ADR-035) so the Host times out first and returns its own clean 503
    /// rather than the BFF cutting off a Host still working. The SK Ollama connector's
    /// default HttpClient uses 100s, which cold inference collides with; this replaces
    /// it. Does not apply to embeddings (nomic calls are sub-second).
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(150);
}

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Wires the served-model half: the Ollama-backed embedder and the Semantic
    /// Kernel explainer, replacing the deterministic side's NoOp defaults. Call
    /// AFTER AddRootCauseInfrastructure so these replacements win. This is the
    /// only place a served model is composed into the runtime (ADR-033).
    /// </summary>
    public static IServiceCollection AddRootCauseExplain(this IServiceCollection services, OllamaOptions? options = null)
    {
        services.AddSingleton(options ?? new OllamaOptions());
        services.AddSingleton<HttpClient>();

        services.Replace(ServiceDescriptor.Singleton<IEmbedder, OllamaEmbedder>());
        services.Replace(ServiceDescriptor.Singleton<IExplainer, SemanticKernelExplainer>());

        return services;
    }
}
