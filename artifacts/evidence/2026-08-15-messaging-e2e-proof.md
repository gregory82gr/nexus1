# Evidence: Messaging backbone phase (a) — real end-to-end proof (§5 step 7)

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16 (native, portable
install — see `docs/runbooks/local-rabbitmq.md`).

This is the proof requested for phase (a): "with both hosts running against
the real broker and real databases, actually publish a flood-detected event
from AlarmManagement and confirm RootCause consumes it and reacts — inspect
the RabbitMQ management UI/API to show the message actually flowed, not just
that both sides compiled against the same contract type."

## Setup

- Fresh migrations applied to all three databases (`dotnet ef database
  update`): ReactorFleet/AlarmManagement (sharing `AlarmManagementDb`, now
  including `messaging.OutboxMessage`), RootCause (`RootCauseDb`, now
  including `messaging.InboxReceipt`).
- `Nexus1.ModularRuntime` started on `http://localhost:5101` (composes
  ReactorFleet + AlarmManagement, runs `OutboxPublisherBackgroundService`).
- `Nexus1.RootCause.Host` started on `http://localhost:5102` (composes
  RootCause, runs `AlarmFloodConsumerBackgroundService`).
- Both `/health/ready` returned `Healthy` before triggering anything.

## Topology, confirmed via the management API before any message was sent

```
GET /api/exchanges/%2F/nexus.events
  type=topic, durable=true

GET /api/queues/%2F/rootcause.alarm-events.v1
  arguments={"x-queue-type":"quorum"}, durable=true

GET /api/exchanges/%2F/nexus.events/bindings/source
  source=nexus.events, destination=rootcause.alarm-events.v1,
  routing_key=alarm-management.alarm-flood-detected.v1
```

Matches ADR-008's adopted convention exactly (`<producer>.<event-name>.v<major>`
routing key, shared topic exchange, durable quorum queue).

## Trigger: a real command execution against the live database

A throwaway console harness (outside the repo, not committed — resolves
`DetectFloodCommandHandler` via the same public DI registration extensions
`AddAlarmManagementApplication`/`AddAlarmManagementInfrastructure` real
callers use) seeded one `AlarmDefinition` and three `AlarmEvent`s for a
distinctive `UnitId=9001` directly into the **same physical
`AlarmManagementDb`** the live `ModularRuntime` host was already polling,
then called `DetectFloodCommandHandler.Handle(...)`:

```
Seeded AlarmDefinition + 3 AlarmEvents for UnitId=9001.
DetectFloodCommand result: IsSuccess=True, AlarmFloodId=639223865633744300
```

This is a real command handler execution against a real database that the
live host is watching — not a call into either host's process.

## Outbox: the row was picked up and published by the live host

```sql
SELECT MessageId, EventType, RoutingKey, StoredAtUtc, ProcessedAtUtc
FROM messaging.OutboxMessage ORDER BY StoredAtUtc DESC;
```

```
MessageId       8617F2F1-AEB4-4DD2-AE1C-CCBF9D88E66E
EventType       nexus1.alarm-management.alarm-flood-detected.v1
RoutingKey      alarm-management.alarm-flood-detected.v1
StoredAtUtc     2026-08-15 10:29:24.1654897
ProcessedAtUtc  2026-08-15 10:29:24.8122549
```

`ProcessedAtUtc` non-null, ~0.65s after `StoredAtUtc` — the live
`OutboxPublisherBackgroundService` (250ms poll interval) picked the row up
and published it, exactly as designed.

## Broker: real publish/deliver/ack counters, not just topology

```
GET /api/exchanges/%2F/nexus.events
  message_stats: publish_in=1, publish_out=1

GET /api/queues/%2F/rootcause.alarm-events.v1
  message_stats: publish=1, deliver=1, deliver_get=1, ack=1
  messages=0 (fully drained), consumers=1 (active)
```

One message went in, one came out, one was delivered to the live consumer,
one was acknowledged. This is the broker's own accounting, not a description
of expected behavior.

## Inbox: the live RootCause consumer actually received it

```sql
SELECT ConsumerName, MessageId, Producer, EventType, ReceivedAtUtc, CompletedAtUtc
FROM messaging.InboxReceipt ORDER BY ReceivedAtUtc DESC;
```

```
ConsumerName   rootcause.alarm-events.v1
MessageId      8617F2F1-AEB4-4DD2-AE1C-CCBF9D88E66E   <- same MessageId as the outbox row
Producer       alarm-management
EventType      nexus1.alarm-management.alarm-flood-detected.v1
ReceivedAtUtc  2026-08-15 10:29:26.1910198
```

The `MessageId` matches the outbox row exactly — this receipt was written by
the live `AlarmFloodMessageHandler` consuming the real AMQP delivery, not a
fabricated value.

## RootCause reacted: a real analysis was opened

```sql
SELECT * FROM RootCause.RootCauseAnalysis ORDER BY OpenedAtUtc DESC;
```

```
RootCauseAnalysisId  639223865659513187
UnitId                9001                          <- matches the harness's seeded UnitId
AlarmFloodId          639223865633744300             <- matches DetectFloodCommand's own AlarmFloodId
Status                Open
OpenedBy              system:alarm-flood-consumer    <- the real consumer, not a test double
```

`UnitId` and `AlarmFloodId` match the AlarmManagement-side values exactly,
end to end: command handler -> outbox -> broker -> inbox -> domain reaction.

## Regression check

`dotnet test Nexus1.Runtime.sln` after stopping both hosts (DLL locks
released) — **71/71 passing** (was 65 before this phase; +3 `OutboxRelayTests`,
+3 `AlarmFloodMessageHandlerTests`):

```
Nexus1.ReactorFleet.UnitTests           12/12 passed
Nexus1.RootCause.UnitTests               9/9  passed
Nexus1.AlarmManagement.UnitTests        16/16 passed
Nexus1.AlarmManagement.ComponentTests   15/15 passed
Nexus1.RootCause.ComponentTests          9/9  passed
Nexus1.ReactorFleet.ComponentTests       3/3  passed
Nexus1.ArchitectureTests                 7/7  passed
```

Both host processes were killed and the MSBuild/Roslyn build servers shut
down afterward — this evidence captures what was actually run, not a
standing dev environment.

## Owned

- The triggering harness is intentionally **not** part of the repo: it lives
  in the session scratchpad, references the real Application/Infrastructure
  assemblies only through their public DI extensions (no `InternalsVisibleTo`
  added to production code for a throwaway tool), and was deleted after use.
  It exists only to prove the wiring; it is not a substitute for an owned,
  repo-committed end-to-end test.
- Building the harness against a `net9.0`-targeted throwaway project while
  referencing `net8.0` repo projects surfaced an MSBuild subtlety worth
  recording: a `<PropertyGroup>` in a project's own `.csproj` (e.g.
  `EnforceCodeStyleInBuild=false`) only affects that project's own
  compilation — it does **not** propagate to `ProjectReference`d projects
  when MSBuild builds them as part of the graph. Each referenced project is
  evaluated with its own `Directory.Build.props`/local props independent of
  the referencing project's local overrides. Global properties passed on the
  command line (`dotnet run -p:EnforceCodeStyleInBuild=false ...`) *do*
  propagate across the whole build graph and were used instead — no repo
  source file was touched to work around this (unlike the two earlier
  occurrences of this exact scratch-harness quirk, recorded in
  SESSION-LOG.md, where a repo interface got an explicit `public` modifier).
- `tests/Nexus1.DistributedSlice.EndToEndTests/` (`.csproj` only, zero test
  files, `dotnet test` reports "No test is available") was checked —
  correctly still empty, not a gap. CLAUDE.md §4/§5 names this project in
  the reference tree and explicitly scopes its content to step 9 ("End-to-
  end slice tests + failure experiments... per book Ch. 36"), which comes
  after step 8 (fan-out subscribers) — both still ahead of this session.
  Step 1's own instruction was to scaffold it empty ("empty context/host/
  test projects per §4... before any business code exists"). This phase's
  proof (above) was intentionally done through the scratch harness plus
  direct DB/management-API inspection rather than by populating this
  project early, so that step 9's real automated E2E suite is designed once
  fan-out subscribers exist too, not piecemeal against a moving target.
