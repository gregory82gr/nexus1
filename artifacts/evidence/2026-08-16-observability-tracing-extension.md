# Evidence: Observability tracing extension (Ch.51) — AlarmManagement, Audit, Compliance, Reporting

Date: 2026-08-16
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16,
`otelcol-contrib` v0.158.0 (portable, `docs/runbooks/local-otel-collector.md`).

Scope: ADR-013 step 5 — extending RootCause's proof-context instrumentation
(`artifacts/evidence/2026-08-15-observability-tracing-foundation.md`) to the
remaining four contexts, then proving the shared plumbing holds under a
genuine multi-context fan-out, not just isolated per-context checks.

## Automated regression: 151/151 passing (was 140/140 before this step)

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.BuildingBlocks.Observability.UnitTests      9/9  passed
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   20/20 passed
Nexus1.AlarmManagement.ComponentTests             19/19 passed  (was 15;
                                                    +4 TracingTests)
Nexus1.Audit.ComponentTests                       11/11 passed  (was 9;
                                                    +2 TracingTests)
Nexus1.Compliance.ComponentTests                  11/11 passed  (was 9;
                                                    +2 TracingTests)
Nexus1.Reporting.ComponentTests                   12/12 passed  (was 9;
                                                    +3 TracingTests)
Nexus1.ArchitectureTests                           7/7  passed
```

`Nexus1.DistributedSlice.EndToEndTests` carries the tracked empty `.csproj`
only (0 tests) — same pattern as the RootCause step; its `.csproj` now also
references Audit/Compliance/Reporting Infrastructure for the next session
that needs the fan-out harness shape again.

## Per-context instrumentation

**AlarmManagement** (`DetectFloodCommandHandler`, `DefineAlarmCommandHandler`,
`EvaluateReadingCommandHandler`, `AcknowledgeAlarmCommandHandler`) — INTERNAL
owner spans on all four command handlers; `RetryDispatcher` background-work
span; `OutboxMessage`/`EfOutboxWriter`/`OutboxRelay` carry the same
`ProducerTraceSnapshot` plumbing as RootCause (migration
`20260816043459_AddOutboxMessageTraceSnapshot`, applied to
`AlarmManagementDb`).

**Audit** (`AuditVerdictMessageHandler`, `AuditConsumerBackgroundService`,
`RetryDispatcher`) — CONSUMER span + carrier extraction in the background
service; nested INTERNAL owner span (`audit evidence record`) in the
handler, outcome `COMMITTED`/`DUPLICATE_MATCH` on the two-key dedup branches.
No outbox (Audit is consumer-only).

**Compliance** — same shape as Audit exactly (`compliance review open`,
`ComplianceConsumerBackgroundService`, `RetryDispatcher`), consumer-only.

**Reporting** — same CONSUMER-span pattern in
`ReportingConsumerBackgroundService`, but with *two* nested INTERNAL owner
spans (`reporting apply case-opened`, `reporting apply verdict-issued`) since
`ReportingProjectionMessageHandler` has two reducers over one wildcard-bound
queue. The verdict reducer now surfaces a third outcome code beyond
`COMMITTED`/`DUPLICATE_MATCH`: `ABSTAINED`, for the out-of-order path where
the verdict arrives before its case-opened event and gets buffered into
`PendingVerdict` rather than applied — matching Ch.51's STATUS MATRIX
vocabulary (51-R) rather than inventing a fifth status. Consumer-only, no
outbox.

### Correction: PRODUCER is shared, CONSUMER is not

The step-5 plan assumed "the shared AMQP carrier and PRODUCER/CONSUMER spans
already cover all contexts via the shared publisher/consumer path." Verified
against the actual code before extending, per this project's verification
convention: **only the PRODUCER side is genuinely shared** — every context's
outbox dispatch goes through the one `RabbitMqBrokerPublisher` in
`Nexus1.BuildingBlocks.Messaging`, so PRODUCER spans and carrier injection
needed zero new code. **CONSUMER-span wrapping is duplicated, not shared** —
each context owns its own `XxxConsumerBackgroundService.cs` (a thin
hand-written hosting loop around `AsyncEventingBasicConsumer`), so the
CONSUMER span + carrier-extraction block had to be added independently to
Audit's, Compliance's, and Reporting's own files. This is exactly the
"generalizes with zero changes" trap this project has already been burned by
twice (`DraftGate`/`BundleCheckRunner`, EVD-01..04 reuse) — caught here
before being asserted as fact in the evidence report, not after.

## Fan-out campaign scope decision: one shared campaign, not four

User's call, with reasoning recorded here as asked. Audit, Compliance, and
Reporting all bind to the same `RootCauseVerdictIssuedV1` publish (Reporting
via the `root-cause.#` wildcard, Audit/Compliance via the exact routing key)
— one publish, one AMQP fanout, three independent deliveries. A single
campaign that publishes **one** verdict and observes all three consumers
proves something four isolated campaigns cannot: that one PRODUCER span's
injected carrier is extracted *consistently* by three independently
hand-written consumer implementations, all landing as children of the same
parent under the same traceId — not merely that each, checked alone, can
extract *some* carrier. Four separate campaigns would each only prove "my
own consumer works in isolation," which the per-context component tests
(`TracingTests.cs`, in-process `ActivityListener` capture) already prove
faster and more cheaply than a real-collector round trip. The real-collector
campaign's distinguishing value is specifically the cross-context structural
proof, so it was spent on the fan-out shape. AlarmManagement got its own
dedicated campaign regardless (explicitly required), since it is the
producer-outbox side of a *different* hop (AlarmManagement -> RootCause),
not part of the verdict fan-out.

## Proof 1 — AlarmManagement complete trace, against the real collector

A throwaway harness (`AlarmManagementTracingHarness.cs`, not committed) drove
the same flood -> auto-open hop RootCause's original proof exercised, but
now with AlarmManagement's own instrumentation live — the point was to prove
the *new* span and its link, not re-prove propagation that already had
evidence.

```
alarm flood commit    spanId=4ab9741615219e63  outcome=COMMITTED
publish flood-detected spanId=7aa2e256d16d0fe6  (linked to 4ab9741615219e63, not parented)
process flood-detected spanId=1d8fc9c753172bb6  parent=7aa2e256d16d0fe6
root-cause case open   spanId=9caf0204a93b8440  parent=1d8fc9c753172bb6
```

Checked structurally: the PRODUCER span for `alarm-flood-detected.v1` now
carries an `ActivityLink` back to AlarmManagement's own `alarm flood commit`
INTERNAL span — absent in the original RootCause proof, where AlarmManagement
had no owner span yet to link to. `process`'s parent is the PRODUCER span's
id; RootCause's `root-cause case open` parents to `process` — propagation
across the broker boundary holds with the new instrumentation in the chain.

## Proof 2 — AlarmManagement broken trace (dropped carrier), against the real collector

Same technique as RootCause's original Proof 2 (direct bare-channel publish,
no `traceparent`/`tracestate`), re-run because AlarmManagement's own
outbox-writing path now has `ProducerTraceSnapshot` machinery in play and the
guarantee needed re-checking with that code present, not assumed to still
hold unchanged:

```
Published AlarmFloodDetectedV1 directly with NO trace carrier. MessageId=97b66914-8a46-40c4-a0fa-f701abaa526e, AlarmFloodId=639224556934241091.
Business state correct despite the dropped carrier: RootCauseAnalysis auto-opened, AnalysisId=639224556956641355.
Confirmed: CONSUMER span for the dropped-carrier delivery has no parent (new root). spanId=42706d6d7537c3d2, parent=(none)
```

Business processing (real auto-open of a `RootCauseAnalysis` row) succeeded
independent of the missing carrier; the CONSUMER span for that specific
delivery (matched by `nexus1.message.id`) has no `parentSpanId` — a genuine
new diagnostic root, TB-07's THR-043-017 holding with AlarmManagement's new
code in the path, not just RootCause's.

## Proof 3 — Verdict fan-out complete trace, against the real collector

A second throwaway harness (`VerdictFanOutTracingHarness.cs`, not committed)
opened a real `RootCauseAnalysis`, added a hypothesis and evidence, then
closed it — driving `CloseAnalysisCommandHandler`'s real INTERNAL span,
outbox write with `ProducerTraceSnapshot`, and the real
`RabbitMqBrokerPublisher` PRODUCER span — while a composed Audit+Compliance+
Reporting host consumed the fanned-out delivery with all three consumers'
real (independently written) CONSUMER-span code:

```
root-cause verdict commit  spanId=223a932295b50332  outcome=COMMITTED
publish verdict-issued     spanId=7284804094b12435  (linked to 223a932295b50332, not parented)
  process (Audit)          spanId=83e6beb70a7adc98  parent=7284804094b12435
    audit evidence record   spanId=fccdc95bc0c6d464  parent=83e6beb70a7adc98  outcome=COMMITTED
  process (Compliance)     spanId=2a0c1155ba921263  parent=7284804094b12435
    compliance review open  spanId=617d204589beb6fc  parent=2a0c1155ba921263  outcome=COMMITTED
  process (Reporting)      spanId=92bfba2f5fe09d65  parent=7284804094b12435
    reporting apply verdict spanId=64b25678be3016f4  parent=92bfba2f5fe09d65  outcome=COMMITTED
```

Checked structurally, not just by presence: all three `process` spans share
the *same* `parentSpanId` (`7284804094b12435`, the one PRODUCER span) and the
*same* `traceId` — one publish, one AMQP topic-exchange fanout, three
independently-implemented consumers all correctly extracting the identical
W3C carrier. Each context's own nested owner span parents correctly to its
own `process` span, and all three report `COMMITTED` — real rows exist in
`AuditDb`, `ComplianceDb`, and `ReportingDb` for the same `AnalysisId`,
verified by direct query before the collector shutdown.

## Proof 4 — Verdict fan-out broken trace (dropped carrier), against the real collector

A `RootCauseVerdictIssuedV1` for a synthetic `AnalysisId` (no matching
`RootCauseCaseOpenedV1` was ever published) was published directly via a bare
AMQP channel, no carrier, exercising both TB-07's dropped-carrier guarantee
*and* Reporting's out-of-order buffering path in the same run:

```
Published RootCauseVerdictIssuedV1 directly with NO trace carrier. MessageId=ed5e30d4-f93c-4668-8e9b-131d3c0bf1c3, AnalysisId=639224561227116294.
Business state correct despite the dropped carrier: Audit evidence recorded, Compliance review opened, Reporting buffered the out-of-order verdict.
Confirmed: independent new root - traceId=96d2dd5750c5c72ecb5c90610e35011c spanId=9397108912cd50c2 parent=(none)
Confirmed: independent new root - traceId=b940e1413113ca7bd760c809eb4f9b06 spanId=cafabd0ad53bb953 parent=(none)
Confirmed: independent new root - traceId=66a980c8cb619850b0b66d1769c8d8bf spanId=8e03a9625aa9b1ab parent=(none)
```

All three `process` spans for this message (`nexus1.message.id` matched)
have **three distinct traceIds**, not one — each of the three independently
hand-written consumers correctly falls back to its own new diagnostic root
rather than inheriting a stale or shared one, and no `parentSpanId` on any of
them. Business state committed correctly in all three databases regardless
(Audit evidence row, Compliance review row, and — the interesting third
case — Reporting's `PendingVerdict` buffer row, since no case-opened event
exists for this synthetic id; buffering, not committing, *is* Reporting's
correct business outcome here, per ADR-012's out-of-order design, and the
missing carrier didn't change that decision).

## Broker-side evidence — real counters, cumulative across this project's history

```
GET /api/exchanges/%2F/nexus.events
  publish_in=40, publish_out=58

GET /api/queues/%2F/rootcause.alarm-events.v1        ack=15, deliver=15, messages=0
GET /api/queues/%2F/audit.root-cause-verdicts.v1      ack=9,  deliver=9,  messages=0
GET /api/queues/%2F/compliance.root-cause-verdicts.v1 ack=9,  deliver=9,  messages=0
GET /api/queues/%2F/reporting.integration-events.v1   ack=25, deliver=25, messages=0
```

Every queue fully drained after all four campaigns — no stuck or
unacknowledged deliveries left behind.

## Owned

- **A real test-isolation defect, not a tracing-correctness bug, found and
  fixed during this step's verification pass.** Running the newly-added
  `TracingTests.cs` for AlarmManagement, Compliance, and Reporting inside the
  full `dotnet test` run produced non-deterministic failures — extra spans
  matching `Assert.Single`'s predicate that didn't belong to that test.
  Root cause: `CaptureSpans`' `ActivityListener` is registered process-wide
  and filters only by `ActivitySource` name; xUnit's default cross-class
  parallelism lets a *different* test class in the same assembly (e.g.
  `ComplianceVerdictMessageHandlerTests`, which exercises the same handler
  and therefore emits spans on the same source) run concurrently and leak
  its spans into the listener. Not a flaky assertion to patch around —
  `[assembly: CollectionBehavior(DisableTestParallelization = true)]` added
  to all five `*.ComponentTests` projects that use this pattern (RootCause,
  AlarmManagement, Audit, Compliance, Reporting), verified deterministic
  across 3 repeated runs per project (15/15 green) before trusting any of
  this step's component-test evidence.
- Three harness-only bugs, not product bugs, all caught before being
  reported as evidence:
  1. OTLP JSON encodes span `kind` as a numeric enum
     (`SPAN_KIND_INTERNAL=1`, ..., `CONSUMER=5`), not a string — the
     original `TraceCorpusReader` (built fresh for this step, RootCause's
     reader was never committed) called `.GetString()` on it and crashed.
  2. The OTLP batch exporter can flush the same completed span twice (a
     periodic timer flush racing the explicit `ForceFlush` a clean host
     shutdown triggers) — same traceId+spanId written to the file exporter
     twice. `TraceCorpusReader` now dedupes by `(traceId, spanId)` before
     returning; without this, `Assert.Single` assertions failed on
     genuinely-duplicated (not duplicated-business-operation) spans.
  3. A single fixed post-`StopAsync()` delay raced the OTLP exporter flush
     under multi-host/multi-consumer scenarios — reliable for the two-host
     AlarmManagement campaign at 4s, insufficient for the three-consumer
     fan-out broken-trace campaign (2/3 spans present at the 4s mark, all
     3 present moments later). Fixed by polling the corpus for the expected
     span count with a bounded retry loop instead of guessing a delay long
     enough. None of these touched any file under `src/`.
- `RootCauseDb`, `AuditDb`, `ComplianceDb`, `AlarmManagementDb`, `ReportingDb`
  were left in place — harmless local dev state, same reasoning as every
  prior step.
- Both throwaway harnesses and the collector process were stopped after
  evidence capture; `AlarmManagementTracingHarness.cs`,
  `VerdictFanOutTracingHarness.cs`, and `TraceCorpusReader.cs` were deleted,
  leaving only the tracked `Nexus1.DistributedSlice.EndToEndTests.csproj`
  (now with Audit/Compliance/Reporting Infrastructure references added) —
  same pattern as the RootCause step.

## Scope explicitly not covered by this step

Metrics (Ch.52) remain deferred to step 6 per ADR-013. All six contexts now
have tracing parity (owner spans, PRODUCER/CONSUMER spans, carrier
propagation); none has a metrics pipeline yet.
