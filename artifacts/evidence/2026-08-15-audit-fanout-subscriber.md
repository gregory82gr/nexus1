# Evidence: Audit fan-out subscriber (§5 step 8, first of three)

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16.

Scope and design decisions are recorded in
`docs/adr/ADR-010-audit-fanout-subscriber.md`. This report is the real
end-to-end proof: `RootCauseVerdictIssuedV1` actually published by RootCause
(via a producer-side outbox that did not exist before this step) and
actually consumed by Audit, over the real broker.

## Automated regression: 104/104 passing (was 101 before this step)

```
Nexus1.ReactorFleet.UnitTests           12/12 passed
Nexus1.RootCause.UnitTests               22/22 passed
Nexus1.AlarmManagement.UnitTests        16/16 passed
Nexus1.Audit.UnitTests                   3/3  passed  (new)
Nexus1.AlarmManagement.ComponentTests   15/15 passed
Nexus1.RootCause.ComponentTests         17/17 passed  (was 14; +3: RootCause's own OutboxRelayTests)
Nexus1.Audit.ComponentTests              9/9  passed  (new)
Nexus1.ReactorFleet.ComponentTests       3/3  passed
Nexus1.ArchitectureTests                 7/7  passed  (no special-casing needed for the new Audit context)
```

## Topology and DLX policy — confirmed before any message was sent

```
GET /api/queues/%2F/audit.root-cause-verdicts.v1
  type=quorum, durable=true,
  policy=nexus-live-queue-safety-audit.root-cause-verdicts.v1

GET /api/policies/%2F/nexus-live-queue-safety-audit.root-cause-verdicts.v1
  pattern: ^audit\.root-cause-verdicts\.v1$
  definition: dead-letter-exchange=nexus.dead,
              dead-letter-routing-key=audit.root-cause-verdicts.v1.dead,
              dead-letter-strategy=at-least-once,
              overflow=reject-publish, delivery-limit=8

GET /api/exchanges/%2F/nexus.events/bindings/source
  -> destination=audit.root-cause-verdicts.v1, destination_type=queue,
     routing_key=root-cause.root-cause-verdict-issued.v1
```

Matches ch.34's frozen topology (Executable Asset 34-U) and ADR-009's
policy-provisioning pattern, reused unmodified for a second queue.

## Trigger: the full real chain, not a synthetic shortcut

A throwaway harness (not committed) drove the entire chain through real
command handlers against the live databases the running hosts (`Nexus1.
ModularRuntime` composing ReactorFleet+AlarmManagement+Audit, `Nexus1.
RootCause.Host`) were already watching — both confirmed `Healthy` on
`/health/ready` beforehand:

```
Seeded AlarmDefinition + 3 AlarmEvents for UnitId=9004.
DetectFloodCommand result: IsSuccess=True, AlarmFloodId=639223958878295524
RootCauseAnalysis opened by the live consumer: AnalysisId=639223958897100060, OpenedBy=system:alarm-flood-consumer
AddHypothesisCommand result: IsSuccess=True, HypothesisId=1549382898
AddEvidenceCommand result: IsSuccess=True
CloseAnalysisCommand result: IsSuccess=True, AnalysisId=639223958897100060
```

Every step ran through the real Application-layer command handlers — the
same `DetectFloodCommandHandler`/`AddHypothesisCommandHandler`/
`AddEvidenceCommandHandler`/`CloseAnalysisCommandHandler` a real caller would
use — not a hand-crafted database row.

## RootCause's new producer-side outbox actually published

```sql
SELECT MessageId, RoutingKey, StoredAtUtc, ProcessedAtUtc FROM messaging.OutboxMessage;
```

```
MessageId       38156434-CD6E-4F74-B387-BBF1E142A316
RoutingKey      root-cause.root-cause-verdict-issued.v1
StoredAtUtc     2026-08-15 13:04:51.3019393
ProcessedAtUtc  2026-08-15 13:04:51.6595003   <- published ~0.36s later
```

## Audit consumed it — matching MessageId, matching identity

```sql
SELECT ConsumerName, MessageId, ReceivedAtUtc FROM messaging.InboxReceipt;   -- AuditDb
```

```
ConsumerName    audit.root-cause-verdicts.v1
MessageId       38156434-CD6E-4F74-B387-BBF1E142A316   <- same MessageId as the outbox row
ReceivedAtUtc   2026-08-15 13:04:51.7646253
```

```sql
SELECT AuditEvidenceId, SourceMessageId, SourceAnalysisId, EventType, RecordedAtUtc FROM Audit.AuditEvidenceRecord;
```

```
SourceMessageId    38156434-CD6E-4F74-B387-BBF1E142A316   <- matches
SourceAnalysisId   639223958897100060                     <- matches CloseAnalysisCommand's own AnalysisId
EventType          nexus1.root-cause.root-cause-verdict-issued.v1
```

## Broker-side evidence — real counters, not inferred behavior

```
GET /api/queues/%2F/audit.root-cause-verdicts.v1
  message_stats: ack=1
  messages=0, consumers=1

GET /api/exchanges/%2F/nexus.events
  message_stats: publish_in=3
```

(`publish_in=3` is cumulative since the broker's last restart during phase
(b)'s outage proof: 1 recovery-flow message from that proof, plus this run's
2 publishes — the flood-detected event routed to RootCause's queue and the
verdict-issued event routed to Audit's queue. Consistent with what was
actually run, not an isolated count.)

`ack=1, messages=0` on Audit's queue: the one message published was
delivered and acknowledged, fully drained — the live `AuditConsumerBackgroundService`
actually processed it, not a description of expected behavior.

Both host processes were stopped and the MSBuild/Roslyn build servers shut
down after evidence capture; the throwaway harness was deleted.

## Owned

- The retry/DLQ *mechanism* itself (backoff timing, poison quarantine,
  broker DLX routing) is not re-proven against the real broker in this
  report — it's the identical mechanism phase (b) already proved for
  RootCause, reused verbatim for Audit (ADR-010), and is exercised here by
  the component test suite (`Exhausting_the_retry_budget_...`,
  `A_dispatch_failure_leaves_the_ticket_unpublished_...`) instead. What
  needed a *real* end-to-end proof was the new wiring — RootCause actually
  publishing for the first time, and Audit's own two-key dedup oracle and
  append-only enforcement actually running against a real delivery — both
  covered above and by the component suite respectively.
- `AuditDb`/`RootCauseDb`/`AlarmManagementDb` were left in place after this
  run (same reasoning as phase (a)/(b): dropping databases is a destructive
  action the auto-mode classifier correctly gates, and these are harmless
  local dev state, recreatable via `dotnet ef database update`).
