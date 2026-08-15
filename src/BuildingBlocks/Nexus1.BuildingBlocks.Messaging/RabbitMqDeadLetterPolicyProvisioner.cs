using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// From_Services_To_Runtime Executable Asset 25-C, adopted exactly for
/// dead-letter-exchange/strategy/overflow/delivery-limit (ADR-009):
/// dead-lettering for quorum queues is operational policy, not a queue
/// x-argument — "operators can change them without deleting and recreating
/// the live queue" (ch.25.5). RabbitMQ.Client (AMQP) has no policy API, so
/// this goes through the management HTTP API instead — no new package, just
/// the BCL HttpClient.
///
/// One deliberate departure from the book's literal code: the book's own
/// NexusTopology model computes a per-subscription DeadRoute (e.g.
/// "rootcause.alarm-events.v1.dead") and uses it only to bind the dead
/// queue to nexus.dead — it never sets dead-letter-routing-key anywhere
/// (not as a queue argument, not in the Asset 25-C policy). Without that
/// override, RabbitMQ dead-letters a message using its ORIGINAL routing key
/// (e.g. "alarm-management.alarm-flood-detected.v1"), which does not match
/// the dead queue's binding key — the message would be unroutable in
/// nexus.dead and silently dropped. This looks like a gap in the book's own
/// code rather than an intentional simplification, so this implementation
/// adds dead-letter-routing-key to the policy, set to the queue's own dead
/// route. Because that key is inherently per-queue, one policy is applied
/// per live queue rather than one policy pattern-matching several (ADR-009:
/// revisit if/when the pattern needs to cover audit/compliance/reporting
/// subscriptions too, step 8).
/// </summary>
public sealed class RabbitMqDeadLetterPolicyProvisioner(RabbitMqOptions options)
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    public async Task EnsureAsync(string policyName, string liveQueueName, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://{options.HostName}:{options.ManagementPort}/"),
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.UserName}:{options.Password}")));

        var body = new Dictionary<string, object>
        {
            ["pattern"] = $"^{Regex.Escape(liveQueueName)}$",
            ["definition"] = new Dictionary<string, object>
            {
                ["dead-letter-exchange"] = NexusTopology.DeadExchange,
                ["dead-letter-routing-key"] = $"{liveQueueName}.dead",
                ["dead-letter-strategy"] = "at-least-once",
                ["overflow"] = "reject-publish",
                ["delivery-limit"] = 8,
            },
            ["priority"] = 50,
            ["apply-to"] = "quorum_queues",
        };

        var vhost = Uri.EscapeDataString(options.VirtualHost);
        var json = JsonSerializer.Serialize(body, SerializerOptions);
        using var response = await httpClient.PutAsync(
            $"api/policies/{vhost}/{Uri.EscapeDataString(policyName)}",
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
