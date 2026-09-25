using System.Net;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Messaging;
using RabbitMQ.Client.Exceptions;

namespace Nexus1.RootCause.UnitTests;

/// <summary>
/// ADR-044: the shared consumer start sequence retries every failure (broker not up yet,
/// management API not answering) instead of throwing out of ExecuteAsync and stopping the host,
/// disposes a half-started channel before retrying, and stops cleanly on host shutdown.
/// Driven through the generic seam with fakes and a zero backoff, so no broker is needed.
/// </summary>
public class ConsumerStartupTests
{
    [Fact]
    public async Task Retries_failed_start_attempts_until_one_succeeds_and_disposes_each_failed_channel()
    {
        var opened = new List<FakeChannel>();
        var starts = 0;
        var logger = new CapturingLogger();

        var channel = await ConsumerStartup.RunWithRetryAsync(
            waitUntilConnected: _ => Task.CompletedTask,
            openChannel: () => { var c = new FakeChannel(); opened.Add(c); return c; },
            consumerName: "test.queue.v1",
            startConsuming: (_, _) => ++starts < 3 ? throw new InvalidOperationException("management API not up yet") : Task.CompletedTask,
            logger,
            backoff: _ => TimeSpan.Zero,
            CancellationToken.None);

        Assert.Equal(3, starts);
        Assert.Same(opened[2], channel);
        Assert.True(opened[0].Disposed);
        Assert.True(opened[1].Disposed);
        Assert.False(opened[2].Disposed); // the consuming channel stays open
        Assert.Equal(2, logger.Entries.Count(e => e.Level == LogLevel.Warning && e.Message.Contains("could not start")));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("started on attempt 3"));
    }

    [Fact]
    public async Task Retries_when_opening_the_channel_itself_fails()
    {
        var opens = 0;

        var channel = await ConsumerStartup.RunWithRetryAsync(
            waitUntilConnected: _ => Task.CompletedTask,
            openChannel: () => ++opens < 2 ? throw new InvalidOperationException("connection not established yet") : new FakeChannel(),
            consumerName: "test.queue.v1",
            startConsuming: (_, _) => Task.CompletedTask,
            new CapturingLogger(),
            backoff: _ => TimeSpan.Zero,
            CancellationToken.None);

        Assert.NotNull(channel);
        Assert.Equal(2, opens);
    }

    [Fact]
    public async Task Stops_cleanly_on_host_shutdown_while_the_broker_never_appears()
    {
        using var shutdown = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var opens = 0;

        var channel = await ConsumerStartup.RunWithRetryAsync<FakeChannel>(
            // Never connects: waits until cancelled, exactly like RabbitMqConnectionManager with no broker.
            waitUntilConnected: ct => Task.Delay(Timeout.Infinite, ct),
            openChannel: () => { opens++; return new FakeChannel(); },
            consumerName: "test.queue.v1",
            startConsuming: (_, _) => Task.CompletedTask,
            new CapturingLogger(),
            backoff: _ => TimeSpan.Zero,
            shutdown.Token);

        Assert.Null(channel); // returns (does not throw) -- the host's own shutdown, not a crash
        Assert.Equal(0, opens);
    }

    [Fact]
    public async Task Stops_cleanly_on_host_shutdown_during_the_retry_backoff()
    {
        using var shutdown = new CancellationTokenSource();
        var attempts = 0;

        var run = ConsumerStartup.RunWithRetryAsync(
            waitUntilConnected: _ => Task.CompletedTask,
            openChannel: () => new FakeChannel(),
            consumerName: "test.queue.v1",
            startConsuming: (_, _) => { attempts++; shutdown.CancelAfter(50); throw new InvalidOperationException("still failing"); },
            new CapturingLogger(),
            backoff: _ => TimeSpan.FromMinutes(5), // would hang the test if cancellation were ignored
            shutdown.Token);

        var completed = await Task.WhenAny(run, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(run, completed);
        Assert.Null(await run);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task Authentication_failures_are_logged_as_a_credential_problem_not_an_outage()
    {
        var starts = 0;
        var logger = new CapturingLogger();

        await ConsumerStartup.RunWithRetryAsync(
            waitUntilConnected: _ => Task.CompletedTask,
            openChannel: () => new FakeChannel(),
            consumerName: "test.queue.v1",
            startConsuming: (_, _) => ++starts == 1
                ? throw new HttpRequestException("management API", null, HttpStatusCode.Unauthorized)
                : Task.CompletedTask,
            logger,
            backoff: _ => TimeSpan.Zero,
            CancellationToken.None);

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.Contains("rejected the configured credentials", entry.Message);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void Failure_classification_finds_an_AMQP_authentication_failure_anywhere_in_the_chain()
    {
        var amqp = new BrokerUnreachableException(new AuthenticationFailureException("ACCESS_REFUSED"));
        var management = new HttpRequestException("policy PUT", null, HttpStatusCode.Forbidden);
        var outage = new BrokerUnreachableException(new System.Net.Sockets.SocketException());

        Assert.True(RabbitMqFailure.IsAuthenticationFailure(amqp));
        Assert.True(RabbitMqFailure.IsAuthenticationFailure(new AggregateException(management)));
        Assert.False(RabbitMqFailure.IsAuthenticationFailure(outage));
        Assert.False(RabbitMqFailure.IsAuthenticationFailure(new HttpRequestException("down", null, HttpStatusCode.ServiceUnavailable)));
    }

    private sealed class FakeChannel : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));
    }
}
