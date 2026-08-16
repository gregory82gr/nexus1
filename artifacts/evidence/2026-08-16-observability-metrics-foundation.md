# Evidence: Observability metrics foundation (Ch.52) — RootCause proof context

Date: 2026-08-16
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16,
`otelcol-contrib` v0.158.0 (portable, `docs/runbooks/local-otel-collector.md`).

Scope and design decisions are recorded in
`docs/adr/ADR-014-observability-metrics-foundation.md`. This report is the
real proof: the shared metrics catalogue built and unit-tested in isolation,
`ServiceDefaults.AddNexusObservability` extended with `.WithMetrics(...)`,
the collector's Prometheus pipeline wired, RootCause fully instrumented as
the proof context (message attempts/duration, outbox pending/oldest-age/
snapshot-age, inbox outcomes, workflow duration), and both a **complete**
metrics campaign and a **deliberately broken** metrics campaign run against
the live broker + live hosts + a real `otelcol-contrib` Prometheus endpoint.

## Automated regression: 186/186 passing (was 151/151 before this step)

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.BuildingBlocks.Observability.UnitTests     40/40 passed  (was 9;
                                                    +31: ErrorClassifier,
                                                    MetricLabels/
                                                    MetricLabelPolicy,
                                                    MetricCardinality,
                                                    NexusRuntimeMetrics,
                                                    OutboxMetricState)
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   24/24 passed  (was 20;
                                                    +4 MetricsTests — inbox
                                                    outcomes, workflow
                                                    duration present/absent)
Nexus1.Audit.ComponentTests                       11/11 passed
Nexus1.Compliance.ComponentTests                  11/11 passed
Nexus1.Reporting.ComponentTests                   12/12 passed
Nexus1.AlarmManagement.ComponentTests             19/19 passed
Nexus1.ArchitectureTests                           7/7  passed
```

## Shared metrics catalogue — built and proven in isolation first

`MetricNames` (closed instrument-name catalogue, scoped to this step:
message attempts/duration, outbox pending/oldest-age/snapshot-age, inbox
outcomes, workflow duration — no `edge.requests`, no `projection.lag`),
`MetricVocabulary` (`Operations`/`Outcomes`/`Components`/`ErrorTypes`,
deliberately reusing tracing's already-reviewed outcome vocabulary and
`NexusActivitySources.All` rather than inventing a second set),
`MetricLabels`/`MetricLabelPolicy` (bounded `TagList` projection + the
admission gate — an out-of-vocabulary value is never admitted as a new
series, redirected to `TelemetryRejected` instead), `MetricCardinality`
(the `Product()` fail-closed budget check, real worksheet: 4 operations ×
5 outcomes × 7 components = 140, asserted `<= 256`), `NexusRuntimeMetrics`
(the DI-singleton instrument owner), `OutboxMetricSnapshot`/
`IOutboxMetricSnapshotReader`/`OutboxMetricState` (the shared, multi-
component cached-gauge holder — pending/oldest-age/**snapshot-age**, the
third gauge added beyond the book's own two-gauge example specifically so
staleness is a first-class, separately-exported state). `ErrorClassifier`
is new and now shared with tracing: `SafeError.Record` was changed to call
it instead of the raw (technically unbounded) `exception.GetType().Name`.
40 unit tests, zero host or broker involved, before any wiring happened.

## ServiceDefaults.AddNexusObservability — `.WithMetrics(...)` alongside `.WithTracing(...)`

Registers `NexusRuntimeMetrics`/`OutboxMetricState` as singletons, adds the
`Nexus1.Runtime` meter, configures explicit histogram bucket boundaries for
`message.duration` and `workflow.duration`, and wires the OTLP exporter —
with one deliberate departure from the trace-signal defaults: the periodic
metrics export interval is set to 2s instead of the SDK's 60s default (see
"Owned" below — this was found, not assumed, by running the actual
campaign).

## Collector — Prometheus pipeline, a different evidence shape than traces

`otelcol-contrib`'s config gained a `prometheus` exporter (`:9464`) and a
`metrics` pipeline, alongside the unchanged `file/traces` trace pipeline.
Confirmed reachable (`GET /metrics` → 200) before any application code ran.
Documented in `docs/runbooks/local-otel-collector.md`, including that
metrics evidence is a scrape at a chosen instant (cumulative counters since
collector start), not an accumulating corpus the way `traces.json` is.

## RootCause instrumentation — every measurement kind this step defines

- **Message attempts/duration** (`publish`/`process` operations): shared
  `RabbitMqBrokerPublisher` records every outbox dispatch's "publish"
  attempt+duration regardless of which context triggered it (genuinely
  shared code, like the PRODUCER span). RootCause's own
  `AlarmFloodConsumerBackgroundService` records its "process" attempt+
  duration (per-context duplicated code, like the CONSUMER span pattern
  already established for tracing).
- **Inbox outcomes**: `AlarmFloodMessageHandler` records one terminal
  observation per admission decision across all four branches — first-seen
  (`COMMITTED`), fast-path replay (`DUPLICATE_MATCH`), concurrent-write
  resolution (`DUPLICATE_MATCH` or `ABSTAINED` depending on which delivery
  actually won), and processing failure (`FAILED`, with a classified
  `error.type`).
- **Workflow duration**: `CloseAnalysisCommandHandler` records
  `alarm-to-verdict` duration from the new `AlarmFloodStartedAtUtc` durable
  milestone through the verdict-commit timestamp, in the same transaction
  as the Closed-status commit — recorded only when the milestone is
  present (never fabricated for manually-opened analyses).
- **Outbox pending/oldest-age/snapshot-age**: `RootCauseOutboxMetricSnapshotReader`
  (one EF query, count + oldest `StoredAtUtc` together, per ch.52 52-G's
  "one owner read" rule) + `OutboxMetricRefreshBackgroundService` (2s
  cadence, 5s per-read timeout, last-good retained on failure) publish into
  the shared `OutboxMetricState` under component `RootCause`.

### Schema decision: `AlarmFloodStartedAtUtc`

Verified against the actual code before writing any metrics code (this
project's verification convention): `RootCauseAnalysis` had no durable
timestamp for "flood detected," only `OpenedAtUtc` (RootCause's own
processing time). Raised to the user explicitly rather than silently
narrowing the measured span or silently changing the domain model — user
chose adding `AlarmFloodStartedAtUtc` (nullable `DateTime`, populated only
on the real auto-open path, migration
`20260816063656_AddRootCauseAnalysisAlarmFloodStartedAtUtc`) over measuring
only case-opened→verdict. Full reasoning in ADR-014.

## Proof 1 — complete metrics campaign, against the real collector

A throwaway harness (`RootCauseMetricsCampaignHarness.cs`, not committed)
drove the same flood → auto-open → hypothesis → evidence → verdict chain
the tracing campaigns used, against real `AlarmManagementDb`/`RootCauseDb`,
the live broker, and a live `otelcol-contrib` collector restarted fresh
beforehand. Real scrape of `http://localhost:9464/metrics`, all seven
declared instruments present with real values (job labels distinguish the
two harness hosts):

```
nexus1_message_attempts_total{...,nexus1_operation="publish",nexus1_outcome="COMMITTED",...} 3
nexus1_message_attempts_total{...,nexus1_operation="process",nexus1_outcome="COMMITTED",...} 1
nexus1_message_duration_seconds_sum{...,nexus1_operation="publish",...}  0.0916622
nexus1_inbox_outcomes_total{...,nexus1_component="Nexus1.RootCauseAnalysis",nexus1_outcome="COMMITTED",...} 1
nexus1_outbox_pending{...,nexus1_component="Nexus1.RootCauseAnalysis",...} 0
nexus1_outbox_oldest_age_seconds{...} 0
nexus1_outbox_snapshot_age_seconds{...,job="nexus1/Harness.RootCause",...} 1.003879
nexus1_workflow_duration_seconds_count{...,nexus1_operation="alarm-to-verdict",nexus1_outcome="COMMITTED",...} 1
nexus1_workflow_duration_seconds_sum{...} 2.1384616
```

Checked, not just observed to exist: `nexus1_outbox_pending` reads `0`
*beside* a fresh `nexus1_outbox_snapshot_age_seconds` (~1s) — the steady-
state "zero is healthy only when fresh" case (ch.52 52-M) actually
demonstrated, not merely asserted possible. `nexus1_workflow_duration_seconds_sum`
(2.14s) is a real end-to-end duration reconstructed from the durable
`AlarmFloodStartedAtUtc`→`ClosedAtUtc` milestones, not an ambient stopwatch.

## Proof 2 — deliberately broken metrics campaign (out-of-vocabulary label), against the real collector

Matching ch.52 52-AA's `UnsafeMetricFault` intent, adapted to this
project's actual defense layer: rather than bypassing the Meter API
directly (this project's admission gate lives in application code, not
only at the SDK view layer), the harness called
`MetricLabelPolicy.TryFor("not-a-reviewed-operation", "COMMITTED",
NexusActivitySources.RootCause, out _)` on a real host's real
`NexusRuntimeMetrics` singleton:

```
TelemetryRejected before=0 after=1
Business state correct despite the rejected metric: RootCauseAnalysis auto-opened, AnalysisId=639224609552868836.
Confirmed: no series exists for the unreviewed operation value.
```

Both halves checked independently: **(a)** `nexus1_telemetry_rejected_total`
increased by exactly one after the deliberately out-of-vocabulary call, and
**(b)** the string `"not-a-reviewed-operation"` never appears anywhere in
the post-fault scrape — the gate genuinely never admitted a new series, it
did not merely log a warning. Alongside the injected fault, a real
`AlarmFloodDetectedV1` delivery through the same host still auto-opened a
`RootCauseAnalysis` row correctly — TB-07's "compromised or missing
telemetry may impair detection, but it must not manufacture or block a
business outcome," already proven for tracing's dropped-carrier case,
extended here to metrics' cardinality-admission case.

## Owned

- **A real timing gotcha, not a product bug, found running the first
  complete-campaign attempt**: the OTLP metrics SDK's `PeriodicExportingMetricReader`
  defaults to a 60-second export interval (unlike traces' ~5s batch flush).
  The harness's first scrape (5s after the business stimulus) read an
  empty body, while a manual `curl` moments later showed the export had
  genuinely happened — confirming the pipeline worked, just not yet at
  scrape time. Fixed by setting `ExportIntervalMilliseconds = 2000` in
  `AddNexusObservability`'s metrics registration — an explicit "evidence
  profile" decision (same rationale as `AlwaysOnSampler` for tracing),
  documented inline and in ADR-014, not a production-latency claim.
- Two AddOtlpExporter overload-resolution misses while writing the fix
  (guessed a two-Action-parameter overload that does not exist; the actual
  API takes one `Action<OtlpExporterOptions, MetricReaderOptions>`) —
  caught by the compiler immediately, no runtime impact.
- The broken-campaign harness logged (not failed on) `ObjectDisposedException`/
  `TaskCanceledException` from `RetryDispatcherBackgroundService`/
  `OutboxPublisherBackgroundService` during host teardown — the harness
  disposes the host synchronously (`using`, not `await ... StopAsync()`) at
  test end, which does not gracefully drain in-flight background-service
  loop iterations before the DI container is torn down. Harness-only,
  matches the same shutdown-ordering class of issue already documented in
  the tracing evidence reports; did not affect any assertion.
- `AlarmManagementDb`/`RootCauseDb` were left in place — harmless local dev
  state, same reasoning as every prior step.
- Both harness hosts and the collector process were stopped after evidence
  capture; `RootCauseMetricsCampaignHarness.cs` was deleted, leaving only
  the tracked empty `Nexus1.DistributedSlice.EndToEndTests.csproj` — same
  pattern as every prior real-collector proof.

## Scope explicitly not covered by this step

Per the user's explicit scope for this step: `nexus1.edge.requests` (no BFF
exists, ADR-007) and `nexus1.projection.lag` (RootCause owns no
projection) were not built. Prometheus recording rules, the educational-
objective evaluator (`EducationalObjective`/`ObjectiveVerdict`), and a
provisioned Grafana dashboard — all part of Ch.52's full scope — were not
built; this step is the instrument/vocabulary/collector foundation and
RootCause's own proof only. AlarmManagement, Audit, Compliance, and
Reporting do not yet have their own message-attempt/inbox-outcome/
outbox-gauge instrumentation — the shared plumbing (`RabbitMqBrokerPublisher`'s
"publish" recording, the admission gate, `OutboxMetricState`) already
benefits every context that publishes through it, but the per-context
"process"/inbox/outbox wiring is RootCause-only until the next steps
extend it context by context, per the confirmed plan.
