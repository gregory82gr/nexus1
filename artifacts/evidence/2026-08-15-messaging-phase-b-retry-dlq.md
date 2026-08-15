# Evidence: Messaging backbone phase (b) — retry, backoff, dead-letter hardening (§5 step 7)

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16.

Scope and design decisions are recorded in `docs/adr/ADR-009-messaging-backbone-phase-b.md`.
This report is the real-failure proof phase (b) requires: a simulated poison
message actually landing in the broker's dead-letter queue, and a simulated
broker outage actually leaving outbox rows unpublished until the broker
recovers — broker/DB state as evidence, not test output alone (though the
automated suite is included too).

## Automated regression: 89/89 passing (was 71 before this phase)

```
Nexus1.ReactorFleet.UnitTests           12/12 passed
Nexus1.RootCause.UnitTests               22/22 passed  (was 9; +13: RetryBudget/RetryBackoff)
Nexus1.AlarmManagement.UnitTests        16/16 passed
Nexus1.AlarmManagement.ComponentTests   15/15 passed
Nexus1.RootCause.ComponentTests         14/14 passed  (was 9; +5: retry ticket/poison + RetryDispatcher)
Nexus1.ReactorFleet.ComponentTests       3/3  passed
Nexus1.ArchitectureTests                 7/7  passed
```

## Setup

Both hosts started against the real broker and real (already-migrated,
RetryTicket/PoisonMessage tables included) databases:
`Nexus1.ModularRuntime` on `:5101`, `Nexus1.RootCause.Host` on `:5102`, both
confirmed `Healthy` on `/health/ready` before either scenario.

At `RootCause.Host` startup, `RabbitMqDeadLetterPolicyProvisioner` applied
the DLX policy and `NexusTopology.DeclareDeadQueue` declared the dead queue.
Confirmed via management API before any failure was simulated:

```
GET /api/policies/%2F/nexus-live-queue-safety-rootcause.alarm-events.v1
  pattern: ^rootcause\.alarm-events\.v1$
  definition: dead-letter-exchange=nexus.dead,
              dead-letter-routing-key=rootcause.alarm-events.v1.dead,
              dead-letter-strategy=at-least-once,
              overflow=reject-publish, delivery-limit=8

GET /api/exchanges/%2F/nexus.dead        -> type=topic, durable=true
GET /api/queues/%2F/rootcause.alarm-events.v1.dead
  -> arguments={"x-queue-type":"quorum"}, durable=true
GET /api/exchanges/%2F/nexus.dead/bindings/source
  -> source=nexus.dead, destination=rootcause.alarm-events.v1.dead,
     routing_key=rootcause.alarm-events.v1.dead
```

Matches ADR-009's adopted policy exactly, including the dead-letter-routing-key
correction to the book's own topology model (see ADR-009's "Owned" note).

## Scenario 1: poison message actually reaches the broker's dead-letter queue

A message with a deliberately unparseable body (not valid JSON) was published
directly to `nexus.events` with routing key
`alarm-management.alarm-flood-detected.v1` and a real AMQP `MessageId`, using
a throwaway harness (not committed — publishes via `RabbitMQ.Client` directly,
outside the repo).

The live `RootCause.Host` consumer picked it up and retried with real,
persisted, escalating backoff — not simulated:

```sql
SELECT Attempt, FailureCode, DueAtUtc, PublishedAtUtc, CreatedAtUtc
FROM messaging.RetryTicket WHERE MessageId='e7666ab4-...' ORDER BY Attempt;
```

```
Attempt  FailureCode           CreatedAtUtc          DueAtUtc              PublishedAtUtc
1        JsonReaderException   11:28:22.834           11:28:24.556          11:28:24.786
2        JsonReaderException   11:28:24.796           11:28:27.769          11:28:27.925
3        JsonReaderException   11:28:27.928           11:28:33.890          11:28:33.934
4        JsonReaderException   11:28:33.938           11:28:48.337          11:28:48.436
5        JsonReaderException   11:28:48.429           11:29:01.941          11:29:02.120
```

Five real, escalating, jittered delays (~1.7s, ~2.9s, ~6.0s, ~14.4s, ~13.5s)
— matching `RetryBackoff.ExponentialCap`'s 2s/4s/8s/15s/15s caps minus equal
jitter, not a fixed or naive interval. After the 6th delivery (5 retries
exhausted), the message was quarantined:

```sql
SELECT TerminalReason, RetryAttempts, QuarantinedAtUtc
FROM messaging.PoisonMessage WHERE MessageId='e7666ab4-...';
```

```
TerminalReason: attempt-budget-exhausted
RetryAttempts: 5
QuarantinedAtUtc: 11:29:02.125
```

**Broker-side evidence — the message is physically in the dead queue**, not
just recorded in a database:

```
GET /api/queues/%2F/rootcause.alarm-events.v1.dead
  messages: 1
  messages_ready: 1
  message_bytes: 322
  message_bytes_persistent: 322
```

Live queue's own counters over the whole 6-delivery cycle (cumulative with
phase (a)'s earlier 1 message): `publish: 7, deliver: 7, ack: 6` — exactly
one of the seven deliveries was *not* acked, matching the single
`nack(requeue: false)` that sent the poisoned message to `nexus.dead`.

## Scenario 2: broker outage — outbox rows survive, then recover

RabbitMQ was killed outright (`taskkill` on its OS process) while both hosts
kept running. A new flood (`UnitId=9002`) was then seeded and detected via a
throwaway harness (not committed) — a real `DetectFloodCommandHandler`
execution against the live `AlarmManagementDb`, writing the `AlarmFlood` and
outbox row in one transaction as usual.

Both hosts stayed `Healthy` on `/health/ready` throughout — a broker outage
does not take a host down:

```
GET :5101/health/ready -> Healthy (200)
GET :5102/health/ready -> Healthy (200)
```

The live host's log shows the real connection failure, caught by
`OutboxRelay`'s existing broad catch, exactly as designed:

```
warn: Nexus1.AlarmManagement.Infrastructure.Messaging.OutboxRelay[0]
      Failed to publish outbox message ae5ffa98-...; left unprocessed for redelivery.
      RabbitMQ.Client.Exceptions.AlreadyClosedException: Already closed: ...
      An existing connection was forcibly closed by the remote host.
```

```sql
SELECT MessageId, StoredAtUtc, ProcessedAtUtc FROM messaging.OutboxMessage
WHERE MessageId='ae5ffa98-...';
```

```
StoredAtUtc: 11:34:11.251   ProcessedAtUtc: NULL   <- while broker was down
```

RabbitMQ was then restarted (fresh process, same data directory). The
already-running host's `RabbitMqConnectionManager` — configured with
`AutomaticRecoveryEnabled=true` — reconnected on its own; no host restart was
performed or needed. Once reconnected, the same row was picked up by the next
poll and published:

```sql
SELECT ProcessedAtUtc FROM messaging.OutboxMessage WHERE MessageId='ae5ffa98-...';
```

```
ProcessedAtUtc: 11:35:21.512   <- non-null once the broker recovered
```

And the message actually flowed through to RootCause — a real reaction, not
an inferred one:

```sql
SELECT UnitId, AlarmFloodId, Status, OpenedBy FROM RootCause.RootCauseAnalysis
WHERE UnitId=9002;
```

```
UnitId: 9002   AlarmFloodId: 639223904505651318   Status: Open
OpenedBy: system:alarm-flood-consumer
```

Both processes were stopped and the MSBuild/Roslyn build servers shut down
after evidence capture; both throwaway harnesses (outside the repo) were
deleted.

## Owned

- The `elapsed-budget-exhausted` reason was never observed in this session —
  both scenarios exhaust via `attempt-budget-exhausted` first, since 5
  retries even at worst-case backoff sum to well under the 15-minute
  book-given elapsed budget by design (ADR-009). Elapsed-budget exhaustion is
  covered by a unit test (`RetryBudgetTests`) with a controlled clock, not by
  a real end-to-end run — waiting 15 real minutes for that specific path
  wasn't judged worth the evidence gained over the deterministic unit test.
- A real `DateTime`-Kind bug was caught and fixed during this phase's own
  component-test development (`RetryBudget` taking `DateTime` instead of the
  book's `DateTimeOffset`) — recorded in ADR-009's "Owned" section, not
  repeated here.
