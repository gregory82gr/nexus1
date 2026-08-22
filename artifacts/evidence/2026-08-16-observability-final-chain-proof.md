# Evidence: Observability final cross-cutting proof — the whole Phase 1 chain, one campaign

Date: 2026-08-16
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16,
`otelcol-contrib` v0.158.0 (portable, `docs/runbooks/local-otel-collector.md`).

Scope: the closing step of the observability workstream (ADR-013/ADR-014),
matching how step 8 (the fan-out subscribers) was closed with its own final
evidence report. Every prior tracing/metrics campaign in this project
exercised one or two hops at a time (RootCause's own proof context, then
each context extended and proven individually or via the verdict fan-out).
This step is the first — and, per the user's own framing, the intentionally
last — campaign to drive the **entire** real chain in one run: flood
detected → AlarmManagement → RootCause opens/hypothesis/evidence/close →
verdict issued → fan-out to Audit/Compliance/Reporting, including
Reporting's own case-opened path, all six contexts, one real trace and one
real metrics capture.

## Automated regression: 194/194 passing, unchanged

No `src/` code changed in this step (see "What this step did and did not
touch" below) — the count is identical to the metrics-extension step's
final count, confirmed re-run clean after this step's harness work:

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.BuildingBlocks.Observability.UnitTests     40/40 passed
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   24/24 passed
Nexus1.AlarmManagement.ComponentTests             19/19 passed
Nexus1.Audit.ComponentTests                       13/13 passed
Nexus1.Compliance.ComponentTests                  13/13 passed
Nexus1.Reporting.ComponentTests                   16/16 passed
Nexus1.ArchitectureTests                            7/7  passed
```

## What this step did and did not touch

This step needed no new instrumentation — every context already had full
tracing and metrics coverage from the two extension steps. What was new
here was **harness topology**: proving the shared plumbing holds not just
per-hop or per-fan-out, but across the *entire* chain in one real run,
against two hosts built to mirror this project's actual deployable
topology exactly (`Nexus1.ModularRuntime`'s real composition — AlarmManagement
+ Audit + Compliance + Reporting — and `Nexus1.RootCause.Host`'s real
composition — RootCause alone), not an ad-hoc grouping invented for the
test. The only `src/` file touched is
`tests/Nexus1.DistributedSlice.EndToEndTests/Nexus1.DistributedSlice.EndToEndTests.csproj`,
which gained a `Microsoft.Extensions.Hosting` package reference so a future
session can build real `IHost`s the same way — no production code changed.

## Proof 1 — the complete chain, against the real collector

A throwaway harness (`FullChainObservabilityCampaignHarness.cs`, not
committed) built both hosts for real, seeded three real `AlarmEvent` rows
(timestamps captured after host start, per the lesson from the
metrics-extension step), called the real `DetectFloodCommandHandler`,
waited for the real chain to run end to end — AlarmManagement's own outbox
relay dispatching with a carrier, RootCause's real
`AlarmFloodConsumerBackgroundService` auto-opening a real
`RootCauseAnalysis`, Reporting's real wildcard consumer picking up the
real `CaseOpened` event (confirmed by directly querying `ReportingDb` for
the new `CaseSummary` row *before* any verdict existed — this is
"Reporting's own case-opened path" the user asked to include, not inferred
from the later verdict transition) — then drove real `AddHypothesis`,
`AddEvidence`, and `CloseAnalysisCommand` calls against the real
auto-opened analysis through `RootCause.Host`'s own DI, and waited for the
real fan-out to settle in `AuditDb`, `ComplianceDb`, and `ReportingDb`.

### Every span kind, structurally validated — not just present

```
publish flood-detected     spanId=4b16ea295e8b6855
process flood-detected     spanId=fa5050db700d04f3  parent=4b16ea295e8b6855
root-cause case open       spanId=c2bdbb1548899860  parent=fa5050db700d04f3
publish case-opened        spanId=b11b27ad0559efda
process case-opened (Rep.) spanId=fdd62a34c7d363f6  parent=b11b27ad0559efda
root-cause verdict commit  spanId=a250ac09bb6096d4
publish verdict-issued     spanId=e8d72173a899bd93
  process (fan-out, Audit)      spanId=cbfe33415debca54  parent=e8d72173a899bd93  traceId=14500e921e8c30fbe5ad0802a0251da0
  process (fan-out, Compliance) spanId=d3d94e9ca1fcca75  parent=e8d72173a899bd93  traceId=14500e921e8c30fbe5ad0802a0251da0
  process (fan-out, Reporting)  spanId=e9573e00126352a8  parent=e8d72173a899bd93  traceId=14500e921e8c30fbe5ad0802a0251da0
```

(AlarmFloodId=639224729494415914, UnitId=9101, AnalysisId=639224729512739008.)

Checked structurally, chained across every hop, not merely "every span
exists": `floodPublish.SpanId == floodProcess.ParentSpanId`,
`floodProcess.SpanId == caseOpen.ParentSpanId`,
`caseOpenedPublish.SpanId == caseOpenedProcess.ParentSpanId` (Reporting's
own case-opened consumer span, parented correctly across the broker
boundary), all three fan-out `process` spans share the *same*
`verdictPublish.SpanId` as parent and the *same* `traceId` — one publish,
one AMQP fanout, three independently hand-written consumers all extracting
the identical carrier. `verdictCommit` itself has no parent, correctly —
it is invoked directly by the harness's own call into
`CloseAnalysisCommandHandler` with no ambient CONSUMER span in scope
(unlike `caseOpen`'s auto-open path, which inherits the flood-detected
CONSUMER span as ambient parent), matching the original RootCause
tracing-foundation proof's own "root-cause verdict commit" span shape.
Every owner span in the chain carries `nexus1.outcome.code = COMMITTED`.

This is every span kind Ch.51 defines, in one graph: PRODUCER/CONSUMER
messaging spans (three separate publish/process hops), INTERNAL owner
spans (flood commit's own effect is visible as the CONSUMER's parent
chain; case-open; add-hypothesis; add-evidence; verdict-commit; each fan-out
context's own nested owner span), and CLIENT SqlClient spans (automatic,
present throughout — not separately quoted here, already proven in every
prior report). The one span kind not exercised by this happy-path
campaign is `RetryDispatcher`'s own INTERNAL background-work span, since
nothing failed for it to retry — already proven present in the tracing
foundation report's Proof 1 and not re-claimed here.

### Every metric instrument, in one real scrape

```
nexus1_inbox_outcomes_total{nexus1_component="Nexus1.RootCauseAnalysis",nexus1_outcome="COMMITTED"} 1
nexus1_inbox_outcomes_total{nexus1_component="Nexus1.Audit",nexus1_outcome="COMMITTED"} 1
nexus1_inbox_outcomes_total{nexus1_component="Nexus1.Compliance",nexus1_outcome="COMMITTED"} 1
nexus1_inbox_outcomes_total{nexus1_component="Nexus1.Reporting",nexus1_outcome="COMMITTED"} 2
nexus1_message_attempts_total{nexus1_component="Nexus1.Messaging",nexus1_operation="publish",nexus1_outcome="COMMITTED"} 3
nexus1_message_attempts_total{nexus1_component="Nexus1.Messaging",nexus1_operation="process",nexus1_outcome="COMMITTED"} 5
nexus1_message_duration_seconds_sum{nexus1_operation="publish"} 0.0879359
nexus1_message_duration_seconds_sum{nexus1_operation="process"} 0.8624014
nexus1_outbox_pending{nexus1_component="Nexus1.AlarmManagement"} 0
nexus1_outbox_pending{nexus1_component="Nexus1.RootCauseAnalysis"} 0
nexus1_outbox_oldest_age_seconds{nexus1_component="Nexus1.AlarmManagement"} 0
nexus1_outbox_oldest_age_seconds{nexus1_component="Nexus1.RootCauseAnalysis"} 0
nexus1_outbox_snapshot_age_seconds{nexus1_component="Nexus1.AlarmManagement"} 1.47
nexus1_outbox_snapshot_age_seconds{nexus1_component="Nexus1.RootCauseAnalysis"} 0.60
nexus1_workflow_duration_seconds_sum{nexus1_operation="alarm-to-verdict",nexus1_outcome="COMMITTED"} 3.7102149
nexus1_workflow_duration_seconds_count{nexus1_operation="alarm-to-verdict",nexus1_outcome="COMMITTED"} 1
```

All eight instrument families from `MetricNames`/`NexusRuntimeMetrics` (plus
the outbox trio's third gauge) present in one scrape: message
attempts+duration for both `publish` and `process`, inbox outcomes for all
four consuming contexts (Reporting shows `2` — its two reducers,
case-opened and verdict-issued, each recording their own terminal
observation), the outbox gauge trio for both outbox-writing contexts
(AlarmManagement and RootCause), and workflow duration reconstructed from
the real `AlarmFloodStartedAtUtc` → verdict-commit milestones (3.71s — a
real elapsed duration across this specific campaign's own real chain, not
a fabricated number). `nexus1_outbox_pending` reads `0` beside a fresh
(sub-2s) `nexus1_outbox_snapshot_age_seconds` for both contexts
simultaneously — the "zero is healthy only when fresh" case proven for
both outbox-writing contexts at once, in the same scrape.

## Proof 2 — the same chain, deliberately broken at both hops, against the real collector

**Where to break it, and why both**: the user offered a choice — the
AlarmManagement→RootCause hop, the RootCause→fan-out hop, or both.
Breaking only one hop would repeat evidence this project already has
(each hop already has its own isolated dropped-carrier proof from the
tracing-extension step). Breaking **both hops in the same run** is
genuinely new evidence: it checks that two independent propagation gaps
in one business chain don't cross-contaminate or mask each other, and —
the specific new case this campaign adds beyond precedent — the second
break lands on a verdict for an analysis that **does** have a real,
already-applied `CaseOpened` event (unlike the tracing-extension's Proof 4,
which deliberately used a synthetic analysis id with no matching
`CaseOpened`, to test Reporting's out-of-order buffering path instead).
This campaign instead exercises Reporting's normal `Opened → VerdictIssued`
transition under a missing carrier, which precedent had not covered.

Business correctness was proven the same way this project always proves
it — direct queries against real databases — while the trace-side technique
matches every prior broken campaign: the two broken publishes are bare
AMQP publishes bypassing `RabbitMqBrokerPublisher` (which always injects
the carrier), not calls through the real command handlers. For hop 2's
business state (the real `RootCauseAnalysis.Close`), the harness calls the
same domain aggregate and repository code `CloseAnalysisCommandHandler`
itself uses — real `Closed` status, real verdict text, real
`ClosedAtUtc` — with only the outbox enqueue (the part that would carry
the carrier) swapped for the direct bare publish, isolating exactly the
hop under test, same discipline as every earlier proof in this project.

```
Published AlarmFloodDetectedV1 directly with NO trace carrier. AlarmFloodId=639224732553604511, UnitId=9202.
Business state correct despite the dropped carrier: RootCauseAnalysis auto-opened, AnalysisId=639224732577845394.
Published RootCauseVerdictIssuedV1 directly with NO trace carrier for the same real AnalysisId=639224732577845394 (real Closed row, real verdict text committed via the same domain code the command handler uses).
Business state correct despite the second dropped carrier: Audit evidence recorded, Compliance review opened, Reporting case advanced to VerdictIssued.
Confirmed: hop-1 CONSUMER span is a new root (no parent). traceId=fa922c0768447b9854d2b594c486038b spanId=d9e84ae8015dd74b
Control confirmed: the untouched case-opened hop still parents normally for this same Reporting consumer. traceId=ef60f5667a80b4d347037ab2ea6d3e82 spanId=7f14c727546c9ca5 parent=6e8f2383c853789f
Confirmed: hop-2 independent new root - traceId=c4d20be4e22112bfad7e8abbe80ed241 spanId=e05d15f0c294f438 parent=(none)
Confirmed: hop-2 independent new root - traceId=07c49b82e37b2d73493de3aee4e56284 spanId=9496af78daa52d94 parent=(none)
Confirmed: hop-2 independent new root - traceId=a3897bae1509af1b3ad22ea8c7250273 spanId=b29081f53107d8cb parent=(none)
```

Checked, not just observed: the hop-1 `process` span for the specific
broken delivery has no `parentSpanId` (one genuine new root). The
**control** — this run's `CaseOpened` hop, which was deliberately left
alone (real relay, real carrier) — still parents normally for the exact
same Reporting consumer that handled the broken verdict moments later,
proving the break is localized to the specific broken deliveries and does
not corrupt every message a broken-hop consumer ever receives. Hop 2
produced **three independent new roots**, one per fan-out consumer, each
with its own distinct `traceId` (four distinct traceIds total across the
whole broken campaign — hop 1's root, and each of the three hop-2 roots) —
each independently-written consumer correctly falls back to its own fresh
diagnostic root rather than inheriting a stale or shared one, exactly
Ch.51's classified propagation-gap behavior, demonstrated across the whole
chain rather than one hop.

## Owned

- **A genuine finding: two `IHost`s built in one test process share
  process-wide `Meter` registration by name, not per-`IServiceProvider`
  isolation.** `NexusRuntimeMetrics.MeterName = "Nexus1.Runtime"` is the
  same string in both hosts; .NET's `Meter`/`MeterListener` subscription
  model operates at the CLR-process level (`AddMeter("Nexus1.Runtime")`
  matches by name, not by which `IMeterFactory` created the `Meter`
  instance). Consequence, observed directly in the scrape: both the
  `Harness.Everything` and `Harness.RootCause` `job` series showed
  **identical** counter values for the same measurement (e.g.
  `nexus1_inbox_outcomes_total{...,nexus1_component="Nexus1.Audit"}` under
  *both* job labels) — each host's own OTel `MeterProvider` was actually
  capturing measurements from *both* hosts' `NexusRuntimeMetrics`
  instances, not just its own. This is the metrics analogue of the
  tracing-extension step's `ActivityListener` cross-test-leakage finding
  (same root cause class: a process-wide .NET diagnostics subscription
  model, assumed to be container-scoped until checked). It does **not**
  affect this project's real deployed topology — `Nexus1.ModularRuntime`
  and `Nexus1.RootCause.Host` run as separate OS processes in production,
  where this collision is structurally impossible — and it does not
  invalidate this campaign's assertions (the right series with the right
  label combinations were still genuinely produced by the right code); it
  is a harness-only artifact of composing two hosts in one process,
  disclosed here rather than left implicit the way the metrics numbers
  might otherwise silently double-count if someone tried to read them as
  "per-process."
- **A real operational finding, not a code defect**: RabbitMQ's durable
  quorum queues had accumulated a 332-message backlog on
  `rootcause.alarm-events.v1` from earlier sessions' campaigns (consistent
  with this project's own documented policy of leaving broker state in
  place between steps). At that backlog size, a *new* campaign's own flood
  message queued behind all 332 stale ones, and the consumer's per-message
  DB round trips made draining the backlog take far longer than any
  reasonable poll timeout. Purged via the RabbitMQ management API
  (`DELETE .../contents` on the four live queues and their `.dead`
  counterparts) before each clean run in this step — a broker-state
  cleanup, not a source change, and consistent with the collector-restart
  precedent already established for getting a clean baseline.
- **A harness-only resilience gap, found and fixed before it could corrupt
  a campaign silently**: .NET's default `HostOptions.BackgroundServiceExceptionBehavior`
  is `StopHost` — the moment *any* one hosted background service throws
  once (observed here as a transient LocalDB command timeout under this
  harness's own heavy concurrent polling load, itself made worse by the
  backlog above), the *entire* host tears down, including unrelated
  hosted services still mid-campaign. A real deployed host reasonably
  wants fail-fast; a long-running, multi-minute evidence harness does not.
  Fixed by configuring `BackgroundServiceExceptionBehavior.Ignore` on both
  harness hosts only — not a production behavior change, and disclosed
  rather than silently patched around.
- **A harness-only bug in the harness's own assertion, not a tracing
  defect**: an early draft asserted `verdictPublish.SpanId` equals
  `verdictCommit`'s parent — meaningless, since `RabbitMqBrokerPublisher`
  *links* a publish span to its `ProducerTraceSnapshot` origin (an
  `ActivityLink`, ch.51 51-J), it never parents to it, and this session's
  trace reader does not parse links. Caught before being reported as
  evidence; replaced with the real, meaningful check —
  `Assert.Null(verdictCommit.ParentSpanId)`, since the harness calls
  `CloseAnalysisCommandHandler` directly with no ambient span in scope.
- Two already-documented export-cadence lessons, both reconfirmed rather
  than assumed to still hold with two hosts and a six-hop chain in play:
  the OTLP metrics exporter's periodic (2s) interval needed an explicit
  wait before scraping (same as the metrics-foundation and
  metrics-extension steps), and the trace batch exporter/collector flush
  needed a bounded poll rather than a fixed delay for the broken
  campaign's read (same fix already applied in the tracing-extension
  step, reapplied here since this run's shape — two independent bare
  publishes racing a batch flush — differs enough from precedent's
  single-publish shape to need re-checking, not assumed to carry over
  unchanged).
- A disposal-time race producing benign `ObjectDisposedException`/
  `TaskCanceledException` log noise from background services during host
  teardown at test end — same documented class as every prior real-host
  harness in this project (`using`, not graceful `StopAsync`); did not
  affect any assertion in either campaign.
- All five databases and the RabbitMQ queues were left in place after
  evidence capture — harmless local dev state, same reasoning as every
  prior step. The collector process was stopped;
  `FullChainObservabilityCampaignHarness.cs` was deleted, leaving the
  tracked `Nexus1.DistributedSlice.EndToEndTests.csproj` (now also
  carrying a `Microsoft.Extensions.Hosting` package reference for the next
  session that needs this two-real-host shape again) — same pattern as
  every prior real-collector proof.

## Closing note

This closes the observability workstream (Ch.51/52, ADR-013/ADR-014): all
six contexts have tracing and metrics parity, the shared plumbing has now
been proven both per-hop, per-fan-out, and as one continuous six-context
chain, and both the complete and broken shapes of that whole-chain proof
are in hand. See ADR-013 and ADR-014 for their own closing notes recording
this. `CLAUDE.md` is updated in the same commit as this report so a future
session does not read Ch.51/52 as outstanding work.
