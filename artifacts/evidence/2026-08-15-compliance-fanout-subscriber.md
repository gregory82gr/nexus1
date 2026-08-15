# Evidence: Compliance fan-out subscriber (§5 step 8, second of three)

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16.

Scope and design decisions — including the deliberate structural fork from
Audit's shape — are recorded in
`docs/adr/ADR-011-compliance-fanout-subscriber.md`. No changes were needed
to RootCause's producer side or the shared retry/inbox machinery; this
report is the real proof that a single publish reaches Audit and Compliance
independently, via two separate bindings from the same topic-exchange
routing key.

## Automated regression: 115/115 passing (was 104 before this step)

```
Nexus1.ReactorFleet.UnitTests           12/12 passed
Nexus1.RootCause.UnitTests               22/22 passed
Nexus1.AlarmManagement.UnitTests        16/16 passed
Nexus1.Audit.UnitTests                   3/3  passed
Nexus1.Compliance.UnitTests              2/2  passed  (new)
Nexus1.AlarmManagement.ComponentTests   15/15 passed
Nexus1.RootCause.ComponentTests         17/17 passed
Nexus1.Audit.ComponentTests              9/9  passed
Nexus1.Compliance.ComponentTests         9/9  passed  (new — includes the
                                                        mutation-allowed test
                                                        contrasting with
                                                        Audit's append-only
                                                        lock)
Nexus1.ReactorFleet.ComponentTests       3/3  passed
Nexus1.ArchitectureTests                 7/7  passed  (no special-casing
                                                        needed for the new
                                                        Compliance context)
```

## Topology — two independent bindings, confirmed before any message was sent

```
GET /api/exchanges/%2F/nexus.events/bindings/source
  -> destination=audit.root-cause-verdicts.v1,       routing_key=root-cause.root-cause-verdict-issued.v1
  -> destination=compliance.root-cause-verdicts.v1,  routing_key=root-cause.root-cause-verdict-issued.v1

GET /api/queues/%2F/compliance.root-cause-verdicts.v1
  type=quorum, durable=true,
  policy=nexus-live-queue-safety-compliance.root-cause-verdicts.v1

GET /api/policies/%2F/nexus-live-queue-safety-compliance.root-cause-verdicts.v1
  dead-letter-exchange=nexus.dead,
  dead-letter-routing-key=compliance.root-cause-verdicts.v1.dead,
  delivery-limit=8
```

Exactly ch.34's "independent subscribers require independent queues" design
(Executable Asset 34-U) — two distinct queues bound to the same routing key,
not a shared queue.

## Trigger: the same real chain, no changes needed to RootCause's producer side

A throwaway harness (not committed) drove the full chain through real
command handlers, exactly as in the Audit proof, against both live hosts
(`Nexus1.ModularRuntime` now composing ReactorFleet+AlarmManagement+Audit+
Compliance, `Nexus1.RootCause.Host`) — both confirmed `Healthy` beforehand:

```
Seeded AlarmDefinition + 3 AlarmEvents for UnitId=9005.
DetectFloodCommand result: IsSuccess=True, AlarmFloodId=639223980819212180
RootCauseAnalysis opened by the live consumer: AnalysisId=639223980837418031, OpenedBy=system:alarm-flood-consumer
AddHypothesisCommand result: IsSuccess=True, HypothesisId=2015364638
AddEvidenceCommand result: IsSuccess=True
CloseAnalysisCommand result: IsSuccess=True, AnalysisId=639223980837418031
```

RootCause's existing outbox published the verdict once — no code changed on
the producer side for this step:

```sql
SELECT MessageId, StoredAtUtc, ProcessedAtUtc FROM messaging.OutboxMessage ORDER BY StoredAtUtc DESC;
```

```
MessageId       E35B01F8-F211-4B18-9109-CFA379E3615B
StoredAtUtc     2026-08-15 13:41:25.8828639
ProcessedAtUtc  2026-08-15 13:41:26.4168370
```

## The fan-out proof: the same MessageId, two independent databases

```sql
SELECT ConsumerName, MessageId, ReceivedAtUtc FROM messaging.InboxReceipt;   -- AuditDb
```
```
ConsumerName    audit.root-cause-verdicts.v1
MessageId       E35B01F8-F211-4B18-9109-CFA379E3615B
ReceivedAtUtc   2026-08-15 13:41:26.5726775
```

```sql
SELECT ConsumerName, MessageId, ReceivedAtUtc FROM messaging.InboxReceipt;   -- ComplianceDb
```
```
ConsumerName    compliance.root-cause-verdicts.v1
MessageId       E35B01F8-F211-4B18-9109-CFA379E3615B
ReceivedAtUtc   2026-08-15 13:41:26.5654891
```

The **identical** `MessageId` appears in both databases, received 8ms apart
— genuinely independent consumption, not one subscriber relaying to the
other. Each recorded its own domain content:

```sql
SELECT AuditEvidenceId, SourceAnalysisId FROM Audit.AuditEvidenceRecord WHERE SourceAnalysisId=639223980837418031;
-- AuditEvidenceId=375D0128-..., SourceAnalysisId=639223980837418031

SELECT ComplianceReviewId, SourceAnalysisId, Verdict, State FROM Compliance.ComplianceReview;
-- ComplianceReviewId=9B15D636-..., SourceAnalysisId=639223980837418031,
-- Verdict='SENSOR_CALIBRATION_DRIFT', State='Pending'
```

Audit recorded an immutable evidentiary copy (with the full envelope
digest); Compliance opened a `Pending` review carrying just the verdict
summary needed to correlate it — exactly the shapes ADR-011 records, now
proven against a real delivery rather than only a component test.

## Broker-side evidence — real counters on both queues

```
GET /api/queues/%2F/audit.root-cause-verdicts.v1
  message_stats: ack=2 (cumulative: 1 from the earlier Audit-only proof, +1 here)
  messages=0

GET /api/queues/%2F/compliance.root-cause-verdicts.v1
  message_stats: ack=1 (Compliance's first-ever delivery)
  messages=0

GET /api/exchanges/%2F/nexus.events
  message_stats: publish_in=5 (cumulative: 3 before this run, +2 here —
                                the flood-detected event and the
                                verdict-issued event)
```

One publish (`publish_in` +1 for the verdict), two acknowledged deliveries
(`ack` +1 on each queue) — the broker's own accounting confirms real
fan-out, not two isolated single-consumer runs.

Both host processes were stopped and the MSBuild/Roslyn build servers shut
down after evidence capture; the throwaway harness was deleted.

## Owned

- Same as the Audit report: the retry/DLQ *mechanism* itself is not
  re-proven against the real broker here — it's the identical mechanism
  already proved twice (phase (b) for RootCause, this step's own component
  suite for Compliance). What needed a *real* proof was the fan-out
  behavior itself (two independent queues actually receiving independent
  copies of the same message), covered above.
- `ComplianceDb` was left in place after this run, same reasoning as every
  prior database in this project (destructive drop correctly gated by the
  auto-mode classifier; harmless local dev state, recreatable via
  `dotnet ef database update`).
