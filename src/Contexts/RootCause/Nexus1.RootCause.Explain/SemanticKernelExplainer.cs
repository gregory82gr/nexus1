using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.Explain;

/// <summary>
/// The constrained Semantic Kernel explain pipeline of Listing 9.4 (ADR-033),
/// the fifth and final stage of the One-Truth Pipeline. A kernel over the local
/// Ollama model with NO planner and NO tool-calling
/// (<see cref="FunctionChoiceBehavior"/>.None()), so the model has nothing to
/// call and cannot begin to act. A single fixed system prompt (H1 closed-world
/// contract) plus the Unified Context (the engine's origin and the retrieved
/// passages) is sent at temperature 0 (H6); the reply is parsed into the H5
/// schema. It never decides the origin -- the deterministic engine already did --
/// it only explains it, and only from the supplied context. A refusal or an
/// unparseable reply becomes an abstention (H8), validated afterwards by H3/H4.
/// </summary>
public sealed class SemanticKernelExplainer : IExplainer
{
    private readonly Kernel _kernel;
    private readonly OllamaPromptExecutionSettings _settings;

    public SemanticKernelExplainer(OllamaOptions options)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOllamaChatCompletion(options.ChatModelId, options.Endpoint);
        _kernel = builder.Build();

        _settings = new OllamaPromptExecutionSettings
        {
            Temperature = 0f,        // H6: determinism (the pinned Modelfile also fixes seed/top_p)
            TopP = 1.0f,
            // No tools are registered, so the model has nothing to call:
            FunctionChoiceBehavior = FunctionChoiceBehavior.None(),
        };
    }

    public async Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken)
    {
        var chat = _kernel.GetRequiredService<IChatCompletionService>();

        // No system message here: the H1 closed-world contract and the H5 schema
        // are baked into the pinned nexus-dslm Modelfile's SYSTEM block, so the
        // contract travels with the model. This call supplies only the Unified
        // Context as the user message.
        var history = new ChatHistory();
        history.AddUserMessage(BuildUnifiedContext(ctx, originTag, passages));

        var reply = await chat.GetChatMessageContentAsync(history, _settings, _kernel, cancellationToken);

        return StructuredAnswerParser.Parse(reply.Content);
    }

    private static string BuildUnifiedContext(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages)
    {
        var sb = new StringBuilder();
        sb.Append("INCIDENT: ").Append(ctx.IncidentId).Append(" (unit ").Append(ctx.UnitId).AppendLine(")");
        sb.Append("ORIGIN (decided by the deterministic engine): ").AppendLine(originTag);
        sb.AppendLine("CONTEXT (retrieved passages -- cite chunkId):");
        foreach (var p in passages)
        {
            sb.Append("[chunkId=").Append(p.ChunkId).Append("] (source: ").Append(p.SourceLabel).Append(") ").AppendLine(p.Body);
        }

        return sb.ToString();
    }
}
