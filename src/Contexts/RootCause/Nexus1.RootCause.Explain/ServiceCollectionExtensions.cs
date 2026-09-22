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
