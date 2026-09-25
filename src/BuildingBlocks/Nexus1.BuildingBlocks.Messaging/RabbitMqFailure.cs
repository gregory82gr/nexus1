using System.Net;
using RabbitMQ.Client.Exceptions;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// Classifies broker failures for logging (ADR-044): a credential problem will not resolve by
/// retrying, so it gets its own message instead of reading like a transient outage.
/// </summary>
public static class RabbitMqFailure
{
    public const string AuthenticationFailureMessage =
        "RabbitMQ rejected the configured credentials (RabbitMq:UserName / RabbitMq:Password) -- retrying, but this will not resolve without a configuration change.";

    /// <summary>True for an AMQP authentication failure anywhere in the exception chain, or a 401/403 from the management API.</summary>
    public static bool IsAuthenticationFailure(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is AuthenticationFailureException
                || (current is HttpRequestException http && http.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden))
            {
                return true;
            }

            if (current is AggregateException aggregate && aggregate.InnerExceptions.Any(IsAuthenticationFailure))
            {
                return true;
            }
        }

        return false;
    }
}
