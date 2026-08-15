# Evidence: Reporting fan-out subscriber (§5 step 8, third and last of three)

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16.

Scope and design decisions — including the reduced-vs-full-book projection
fork — are recorded in `docs/adr/ADR-012-reporting-fanout-subscriber.md`.
Unlike Audit and Compliance, Reporting needed a new prerequisite contract
(`RootCauseCaseOpenedV1`), a wildcard queue binding, and a genuinely new
reducer shape (two event types feeding one projection row, with an
`AnalysisId`-keyed pending buffer for out-of-order arrival). This report is
the real end-to-end proof of all of it against the live broker: the
in-order chain, the fan-out counters, and a deliberately out-of-order
delivery that exercises the pending buffer.

## Automated regression: 128/128 passing (was 115 before this step)

```
Nexus1.ReactorFleet.UnitTests           12/12 passed
Nexus1.RootCause.UnitTests               22/22 passed
Nexus1.AlarmManagement.UnitTests        16/16 passed
Nexus1.Audit.UnitTests                   3/3  passed
Nexus1.Compliance.UnitTests              2/2  passed
Nexus1.Reporting.UnitTests               4/4  passed  (new — both reducers:
                                                        ApplyOpened,
                                                        ApplyVerdictIssued,
                                                        double-apply guard,
                                                        empty-verdict guard)
Nexus1.AlarmManagement.ComponentTests   15/15 passed
Nexus1.RootCause.ComponentTests         17/17 passed  (unchanged — the
                                                        CaseOpened prerequisite
                                                        publish from
                                                        AlarmFloodMessageHandler
                                                        was covered by fixing
                                                        4 pre-existing tests
                                                        that assumed a single
                                                        outbox row per close)
Nexus1.Audit.ComponentTests              9/9  passed
Nexus1.Compliance.ComponentTests         9/9  passed
Nexus1.Reporting.ComponentTests          9/9  passed  (new — in-order,
                                                        out-of-order buffer,
                                                        dedup on both event
                                                        types, retry/poison,
                                                        straight-to-poison for
                                                        an unsupported event
                                                        type)
Nexus1.ReactorFleet.ComponentTests       3/3  passed
Nexus1.ArchitectureTests                 7/7  passed  (no special-casing
                                                        needed for the new
                                                        Reporting context)
```

## Prerequisite: RootCauseCaseOpenedV1, published from RootCause's existing outbox

Added to `Nexus1.Contracts.RootCause` and wired into **two** publish sites,
both using the same outbox mechanism (no second outbox built):

- `OpenAnalysisCommandHandler` — the general-purpose open path.
- `AlarmFloodMessageHandler` — the auto-open path used when RootCause's own
  consumer opens an analysis directly from a flood event (ADR-008 inlines
  `RootCauseAnalysis.Open()` here for transaction atomicity rather than
  calling the command handler, so it needed its own publish call; caught
  while building this step, not assumed away — the real production auto-open
  path would otherwise never have published `CaseOpened` at all).

## Topology — wildcard binding, confirmed before any message was sent

```
GET /api/exchanges/%2F/nexus.events/bindings/source
  -> destination=reporting.integration-events.v1, routing_key=root-cause.#
     (alongside the pre-existing audit/compliance/rootcause exact-key bindings)

GET /api/queues/%2F/reporting.integration-events.v1
  type=quorum, durable=true, consumers=1,
  policy=nexus-live-queue-safety-reporting.integration-events.v1

GET /api/policies/%2F/nexus-live-queue-safety-reporting.integration-events.v1
  dead-letter-exchange=nexus.dead,
  dead-letter-routing-key=reporting.integration-events.v1.dead,
  dead-letter-strategy=at-least-once, delivery-limit=8
```

`NexusTopology.DeclareQuorumQueue` needed no code change — `QueueBind`
already accepted any topic pattern; `root-cause.#` is the only wildcard
binding in the topology so far (ch.35, ADR-012).

## Proof 1 — in-order chain: the real flood-to-verdict path, both reducers firing

A throwaway harness (not committed) drove the full chain through real
command handlers against the live databases the running hosts (`Nexus1.
ModularRuntime` now composing ReactorFleet+AlarmManagement+Audit+
Compliance+Reporting, `Nexus1.RootCause.Host`) were already watching — both
confirmed `Healthy` on `/health/ready` beforehand. AlarmManagement and
RootCause commands were each run through their own `ServiceProvider`
(mirrors the real host-process separation — see "Owned" below for why that
mattered):

```
Seeded 3 AlarmEvents for UnitId=9201.
DetectFloodCommand result: IsSuccess=True, AlarmFloodId=639224028003214559
RootCauseAnalysis opened by the live consumer: AnalysisId=639224028038165230
AddHypothesisCommand result: IsSuccess=True, HypothesisId=1983632797
AddEvidenceCommand result: IsSuccess=True
CloseAnalysisCommand result: IsSuccess=True, AnalysisId=639224028038165230
```

Reporting's projection, queried directly against `ReportingDb` after the
live `ReportingConsumerBackgroundService` (inside `ModularRuntime`) applied
both reducers:

```sql
SELECT RootCauseCaseSummaryId, UnitId, AlarmFloodId, Status, Verdict, OpenedAtUtc, VerdictIssuedAtUtc
FROM Reporting.RootCauseCaseSummary WHERE UnitId = 9201;
```
```
RootCauseCaseSummaryId  639224028038165230   <- matches AnalysisId
UnitId                  9201
AlarmFloodId             639224028003214559   <- matches AlarmFloodCommand's own id
Status                  VerdictIssued
Verdict                 Loose fitting confirmed as cause.
OpenedAtUtc              2026-08-15 15:00:03.8171687
VerdictIssuedAtUtc       2026-08-15 15:00:08.3174329
```

One row, created by `ApplyOpened` and advanced in place by
`ApplyVerdictIssued` — the "delayed truth" projection the book describes,
proven against a real delivery rather than only a component test.

## Proof 2 — deliberately out-of-order: the pending buffer, against the real broker

Published directly to the broker via the same `IBrokerPublisher`/
`MessageEnvelopeFactory` production code the outbox relay uses, in
deliberately reversed order for a synthetic `AnalysisId` that had never
existed in any database:

```
Published VerdictIssued FIRST for synthetic AnalysisId=639224028646395697, MessageId=d00b2d86-6c8f-4c1e-b54e-594bcdb46328.
Confirmed PendingVerdict row holds AnalysisId=639224028646395697 (case-opened row does not exist yet).
Published CaseOpened SECOND for the same AnalysisId=639224028646395697, MessageId=57fda141-7f4a-426d-9b75-77d424d5e0b5.
Reporting projection after resolution: AnalysisId=639224028646395697, Status=VerdictIssued, Verdict=Sensor drift confirmed (out-of-order proof).
Confirmed the PendingVerdict row was consumed once CaseOpened arrived.
```

Between the two publishes, `Reporting.PendingVerdict` genuinely held the row
(verified via a fresh connection) and `Reporting.RootCauseCaseSummary` had
no row yet for that `AnalysisId` — not inferred, checked directly:

```sql
SELECT RootCauseCaseSummaryId, UnitId, AlarmFloodId, Status, Verdict, OpenedAtUtc, VerdictIssuedAtUtc
FROM Reporting.RootCauseCaseSummary WHERE UnitId = 9202;
```
```
RootCauseCaseSummaryId  639224028646395697
UnitId                  9202
AlarmFloodId             555000111
Status                  VerdictIssued
Verdict                 Sensor drift confirmed (out-of-order proof).
OpenedAtUtc              2026-08-15 15:00:59.6395724
VerdictIssuedAtUtc       2026-08-15 15:01:04.6395768
```

`Reporting.PendingVerdict` has 0 rows after both proofs — the buffer emptied
itself once the case-opened row resolved it, exactly as ADR-012 scoped.

## InboxReceipt — both event types recorded by the one wildcard-bound consumer

```sql
SELECT ConsumerName, MessageId, EventType, ReceivedAtUtc FROM messaging.InboxReceipt ORDER BY ReceivedAtUtc;
```
```
reporting.integration-events.v1  D1DAA1E3-...  nexus1.root-cause.root-cause-case-opened.v1     2026-08-15 15:00:06.35
reporting.integration-events.v1  479A4657-...  nexus1.root-cause.root-cause-verdict-issued.v1  2026-08-15 15:00:08.71
reporting.integration-events.v1  D00B2D86-...  nexus1.root-cause.root-cause-verdict-issued.v1  2026-08-15 15:01:05.14
reporting.integration-events.v1  57FDA141-...  nexus1.root-cause.root-cause-case-opened.v1     2026-08-15 15:01:10.03
```

Same consumer name for both event types (one queue, two reducers) — matches
the wildcard-binding design; four receipts for four publishes, no drops, no
duplicates.

## Broker-side evidence — real counters reconcile against what was actually published

```
GET /api/queues/%2F/reporting.integration-events.v1
  message_stats: publish=4, ack=4, deliver=4   (both proofs' 4 messages, fully drained)

GET /api/queues/%2F/audit.root-cause-verdicts.v1
  message_stats: ack=4   (cumulative: 2 from Audit's/Compliance's own proofs
                           + 2 new — Audit is bound to the exact verdict-issued
                           key, so it also received both of this step's
                           VerdictIssued publishes independently)

GET /api/queues/%2F/compliance.root-cause-verdicts.v1
  message_stats: ack=3   (cumulative: 1 from Compliance's own proof + 2 new,
                           same reasoning)

GET /api/exchanges/%2F/nexus.events
  message_stats: publish_in=10 (was 5 before this step: +1 AlarmFloodDetectedV1,
                                 +1 CaseOpened in-order, +1 VerdictIssued in-order,
                                 +1 VerdictIssued out-of-order direct publish,
                                 +1 CaseOpened out-of-order direct publish = +5)
                 publish_out=15 (was 6: AlarmFloodDetectedV1 fans to 1 queue,
                                 each CaseOpened fans to 1 queue (Reporting only —
                                 Audit/Compliance aren't bound to that key), each
                                 VerdictIssued fans to 3 queues (Audit+Compliance+
                                 Reporting) = 1+1+3+1+3 = 9 new -> 6+9=15)
```

Every one of these numbers is exactly what the two proofs above published —
the broker's own accounting confirms real three-way fan-out on the
verdict-issued key and real wildcard delivery on the case-opened key, not
an isolated single-consumer run.

## Owned

- Building the harness surfaced a harness-only bug, not a product bug:
  combining `AddAlarmManagementInfrastructure` and `AddRootCauseInfrastructure`
  registrations on one shared `ServiceCollection` collides on the
  `IUnitOfWork`/`IOutboxWriter` interfaces both contexts register against —
  the last registration wins for singular resolution, so the first attempt
  silently routed `DetectFloodCommandHandler`'s `SaveChangesAsync`/`Enqueue`
  calls to RootCause's `DbContext` instead of AlarmManagement's (caught via
  a debug query showing the flood row never landing, despite `Result.
  IsSuccess=True`). Fixed by giving each context its own `ServiceProvider`
  in the harness, which also more faithfully mirrors the real topology
  (AlarmManagement and RootCause commands run in separate host processes in
  production; they were never meant to share one container). No product
  code was at fault or changed for this.
- Same as the Audit/Compliance reports: the retry/DLQ *mechanism* itself is
  not re-proven against the real broker here — it's the identical mechanism
  already proved for RootCause/Audit/Compliance, plus this step's own
  component suite (`Exhausting_the_retry_budget_...`,
  `An_unsupported_event_type_is_quarantined_immediately_...`). What needed a
  *real* proof was Reporting's own new shape: the wildcard binding actually
  routing both event types to one queue, the two reducers actually building
  one row from two independent deliveries, and the pending buffer actually
  holding and later resolving a genuinely out-of-order delivery — all three
  covered above.
- `ReportingDb` was left in place after this run, same reasoning as every
  prior database in this project (destructive drop correctly gated by the
  auto-mode classifier; harmless local dev state, recreatable via
  `dotnet ef database update`).
- Both host processes and the MSBuild/Roslyn build servers were stopped
  after evidence capture; the throwaway harness (`tests/Nexus1.
  DistributedSlice.EndToEndTests/ReportingEndToEndHarness.cs`) was deleted,
  leaving only the tracked empty `.csproj` — same scratch-project pattern
  used for the Audit and Compliance proofs.

## Step 8 of §5 closed

All three fan-out subscribers — Audit (append-only, single event type),
Compliance (mutable review, single event type, second binding to the same
key), Reporting (wildcard binding, two event types, out-of-order buffer) —
are built, tested, and proven against the real broker. This closes out
build-order step 8 of §5 entirely.
