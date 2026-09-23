using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.Explain;

/// <summary>
/// The real embedder (ADR-033): nomic-embed-text served by the local, loopback-
/// only Ollama. Uses Ollama's stable HTTP embeddings API directly rather than the
/// experimental Semantic Kernel embedding abstraction, which churns across alpha
/// releases; this is still "via Ollama / nomic-embed-text", the same served
/// runtime as the chat model. Replaces the NoOp embedder, lighting up
/// <see cref="IRetriever"/>'s C#-cosine semantic path.
/// </summary>
public sealed class OllamaEmbedder(HttpClient httpClient, OllamaOptions options) : IEmbedder
{
    public async Task<IReadOnlyList<double>?> EmbedAsync(string text, EmbedKind kind, CancellationToken cancellationToken)
    {
        // nomic-embed-text is asymmetric: it expects a task prefix so stored
        // documents and search queries land in comparable regions of the space.
        var prefixed = kind == EmbedKind.Query ? "search_query: " + text : "search_document: " + text;

        using var response = await httpClient.PostAsJsonAsync(
            new Uri(options.Endpoint, "/api/embed"),
            new EmbedRequest(options.EmbeddingModelId, prefixed),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Ollama /api/embed returned no body.");

        if (body.Embeddings is not { Count: > 0 } || body.Embeddings[0] is not { Length: > 0 } vector)
        {
            throw new InvalidOperationException($"Ollama /api/embed returned no embedding for model '{options.EmbeddingModelId}'.");
        }

        return Array.ConvertAll(vector, static v => (double)v);
    }

    private sealed record EmbedRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] string Input);

    private sealed record EmbedResponse(
        [property: JsonPropertyName("embeddings")] IReadOnlyList<float[]>? Embeddings);
}
