# ADR-014: Observability metrics foundation — instrument catalogue, label vocabulary, collector pipeline

## Status

Accepted.

## Context

ADR-013 built the tracing half of Ch.51/52 (Part IX) and explicitly deferred
metrics: "Metrics instrumentation (Ch.52) is explicitly deferred to a later
step." This ADR covers that step. Ch.52 ("Metrics, Service Objectives, and
Dashboards") is a large chapter — instrument catalogue, bounded label
vocabulary, cardinality budgets, outbox/inbox/queue-age/projection-lag/
workflow-duration measures, Prometheus recording rules, SLI/educational-
objective machinery, and a provisioned Grafana dashboard with golden-image
tests. The user scoped this step down explicitly before any code was
written, narrower than the full chapter:

- In scope: message attempts/duration, outbox pending/oldest-age (per-context
  snapshot readers, "duplication until proven" like the retry/poison
  readers), inbox outcomes, workflow duration (computed from durable
  milestones, no ambient stopwatch).
- Explicitly out of scope for this step: `nexus1.edge.requests` (no BFF
  exists — ADR-007 deferred it, so there is no real HTTP edge seam to
  count), `nexus1.projection.lag` (RootCause, the proof context, owns no
  projection — Reporting does, deferred to when this extends there),
  Prometheus recording rules, the educational-objective evaluator, and the
  provisioned Grafana dashboard. Those remain named residuals of Ch.52 this
  project has not built, not silently declared "specified."
- Explicit label-vocabulary discipline: `nexus1.operation`, `nexus1.outcome`,
  `nexus1.component`, `error.type` — reusing the bounded-attribute pattern
  already built for tracing (`SafeTags`/`ErrorClassifier`), not duplicating
  it.
- Observable gauges must read a synchronously-cached snapshot, never do I/O
  in the collection callback (ch.52 52-M), with staleness as a first-class,
  separately-exported state.
- Same complete/broken real-collector campaign discipline as tracing, plus
  Ch.52's own "educational objective" framing: any numeric target is a
  scoped campaign target, never a claimed production SLO.

## Decision

### Vocabulary: reused from tracing, not reinvented

`MetricVocabulary.Outcomes` = `COMMITTED`/`REJECTED`/`ABSTAINED`/
`DUPLICATE_MATCH`/`FAILED` — the same five values every owner span already
tags via `nexus1.outcome.code` (plus `FAILED`, ch.52 52-Q's distinct
"operational failure" dimension), not the book's own illustrative
`succeeded`/`rejected`/`abstained`/`failed` set. `MetricVocabulary.Components`
= `NexusActivitySources.All` exactly — the same seven names already reviewed
for trace sources. `error.type` is `ErrorClassifier`'s five-value bucket
(`timeout`/`dependency_unavailable`/`contract_invalid`/`shutdown_cancelled`/
`unclassified`) — **this classifier is now shared by both signals**:
`SafeError.Record` (trace `error.type`) was changed to call it instead of
the raw `exception.GetType().Name`, which is technically unbounded across
every exception type any dependency could throw. `MetricVocabulary.Operations`
= `publish`/`process`/`retry-dispatch`/`alarm-to-verdict` — the first three
are the messaging-lifecycle stages this project's shared publisher/consumer
path actually has (reduced from Ch.52-K's create/send/receive/process/settle
to what exists); the fourth names the one end-to-end workflow this project
measures rather than introducing a fifth tag key (`nexus1.workflow`) for a
single-element domain.

### Admission gate: reject, don't fabricate

`MetricLabelPolicy.TryFor(operation, outcome, component, out labels)`
returns false for any out-of-vocabulary value; callers then increment
`NexusRuntimeMetrics.TelemetryRejected` (one bounded, always-safe counter)
instead of recording the real instrument — ch.52 52-F's admission gate
("rejected... never admitted as a new series") implemented literally: no
placeholder "unknown" label value is invented, because that would itself
need to be a reviewed vocabulary member. `error.type` never needs runtime
validation since it is always machine-classified (`ErrorClassifier`), never
caller-supplied text.

### Cardinality budget: computed, not assumed

`MetricCardinality.Product(...)` mirrors 52-C's `Product()` check exactly.
Real worksheet: `message.attempts`/`message.duration`'s series count is
Operations(4) × Outcomes(5) × Components(7) = 140, asserted `<= 256`
(`MetricCardinality.MessageMetricsBudget`) — a round number above the
computed product rather than pinned to it, so one more reviewed component
does not immediately require a budget change.

### Outbox gauges: three, not two — staleness is first-class

Ch.52 52-H's worked example shows two gauges (`pending`, `oldest_age`) with
no separate staleness signal. The user's explicit requirement ("a freshness
timestamp beside any 'zero' reading") needed a third: `nexus1.outbox.
snapshot_age` (seconds since the cached snapshot was last refreshed,
distinct from `oldest_age`, which is the age of the oldest *row*, not the
age of the *observation*). `OutboxMetricState` is one shared, multi-
component cached-gauge holder (`Nexus1.BuildingBlocks.Observability`) —
genuinely shared plumbing, like `RabbitMqBrokerPublisher`, since it is one
instrument registration serving every context, not one per context. A
component with no published snapshot yet emits no measurement at all
(absence, not a fabricated zero) — matching ch.52's zero-data discipline.
The **reader** (`IOutboxMetricSnapshotReader`/`RootCauseOutboxMetricSnapshotReader`)
and the **refresh worker** (`OutboxMetricRefreshBackgroundService`) are
per-context, "duplication until proven," exactly as asked — AlarmManagement's
own outbox will need its own copies when tracing/metrics extend there, not
a shared abstraction over two different `DbContext` types.

### Workflow duration needed a small schema addition — flagged, not silently added

Ch.52 52-T's milestone table ("alarm accepted / message committed / case
opened / verdict committed") assumes each milestone's timestamp is already
durable. Verified against the actual code before writing any metrics code
(this project's verification convention): `RootCauseAnalysis` retained only
`OpenedAtUtc` (RootCause's own processing time) and `ClosedAtUtc` — the
flood's true `StartedAtUtc` ("flood detected") flowed through
`AlarmFloodMessageHandler` in memory but was never persisted. Raised to the
user explicitly rather than silently narrowing scope (measure only
case-opened→verdict) or silently adding schema (a domain-model change).
**Decision: add `AlarmFloodStartedAtUtc` (nullable `DateTime`) to
`RootCauseAnalysis`**, populated only on the real auto-open production path
(`AlarmFloodMessageHandler`, which already has the flood's payload in
scope) — the manual `OpenAnalysisCommand` path leaves it `null` (no
cross-context read back to `AlarmManagementDb` exists to backfill it, and
none was added). `CloseAnalysisCommandHandler` records
`nexus1.workflow.duration` only when this value is present — never
fabricated from `OpenedAtUtc` as a stand-in. Migration:
`20260816063656_AddRootCauseAnalysisAlarmFloodStartedAtUtc`, applied to
`RootCauseDb`.

### Collector: Prometheus exporter, not a file corpus — metrics need a different evidence shape than traces

Ch.52's own reference collector profile (52-S) exports metrics to a
scrapable `prometheus` endpoint (`:9464`), not a retained JSON file the way
traces use `file/traces`. Kept both: traces keep `file/traces` unchanged;
metrics get a new `metrics` pipeline exporting to `prometheus`. Evidence for
a metrics campaign is therefore a scrape (`curl`/`Invoke-WebRequest
http://localhost:9464/metrics`, Prometheus text-exposition format read back)
at a chosen instant, not an accumulating corpus — documented in
`docs/runbooks/local-otel-collector.md` alongside the existing trace-corpus
behavior notes.

### New dependency: `Microsoft.Extensions.Diagnostics.Abstractions`

`NexusRuntimeMetrics`/`OutboxMetricState` take `IMeterFactory` in their
constructors — the book's own Executable Asset 52-A pattern. This interface
ships in `Microsoft.Extensions.Diagnostics.Abstractions`, not already
referenced by `Nexus1.BuildingBlocks.Observability` (a plain classlib, no
ASP.NET Core `FrameworkReference`). Added as a small, first-party,
low-risk package — `Host.CreateApplicationBuilder()`/`WebApplicationBuilder`
already register a default `IMeterFactory`, so no additional DI wiring
beyond the package reference itself was needed.

### Proof context: RootCause first, then extend (same as tracing)

RootCause exercises every measurement kind this step defines: `publish`/
`process` message attempts+duration (shared `RabbitMqBrokerPublisher` +
its own `AlarmFloodConsumerBackgroundService`), inbox outcomes
(`AlarmFloodMessageHandler`, all four branches: first-seen, replay-duplicate,
ambiguous-concurrent, processing-failed), outbox pending/oldest-age/
snapshot-age (its own outbox), and workflow duration
(`CloseAnalysisCommandHandler`, present and absent cases). Proven with one
complete and one deliberately-broken real-collector campaign before
extending the same shared plumbing to AlarmManagement, Audit, Compliance,
and Reporting.

## Consequences

- `Nexus1.BuildingBlocks.Observability` gains a metrics half alongside its
  tracing half: `MetricNames`, `MetricVocabulary`, `MetricLabels`/
  `MetricLabelPolicy`, `MetricCardinality`, `NexusRuntimeMetrics`,
  `OutboxMetricSnapshot`/`IOutboxMetricSnapshotReader`, `OutboxMetricState`.
  `ErrorClassifier` is new and shared with tracing's `SafeError`.
- `RootCauseAnalysis` gains one nullable column
  (`AlarmFloodStartedAtUtc`) — the first metrics-driven domain-model change
  in this project; future contexts extending workflow-duration measurement
  may need similar small additions, decided per-context as they come up,
  not assumed to generalize automatically.
- `ServiceDefaults.AddNexusObservability` now registers `NexusRuntimeMetrics`
  and `OutboxMetricState` as singletons and wires `.WithMetrics(...)`
  alongside `.WithTracing(...)`.
- The collector config (`%LOCALAPPDATA%\otelcol-contrib\config.yaml`) gains
  a `prometheus` exporter and a `metrics` pipeline; `docs/runbooks/
  local-otel-collector.md` documents the different evidence shape.
- `nexus1.edge.requests` and `nexus1.projection.lag` remain unbuilt —
  explicit residuals, not silently declared done.

## Rejected alternatives

- **Measure workflow duration from `OpenedAtUtc` instead of adding
  `AlarmFloodStartedAtUtc`.** Rejected: this would silently narrow "alarm
  accepted → flood detected → case opened → verdict issued" down to
  "case opened → verdict issued," dropping the two earliest milestones the
  user explicitly named, without saying so.
- **Give every context's outbox gauge its own three-instrument registration.**
  Rejected: the instrument definitions (name/unit/type) are identical across
  contexts, only the reader's query differs — matching this project's
  PRODUCER-shared/CONSUMER-duplicated precedent from ADR-013's tracing
  extension (`artifacts/evidence/2026-08-16-observability-tracing-extension.md`),
  the state holder is shared, the reader is duplicated.
- **Reuse `exception.GetType().Name` for metric `error.type`, add a separate
  bounded classifier only for metrics.** Rejected as exactly the kind of
  duplicated-not-reused instrumentation the user's checkpoint message warned
  against — `SafeError` was changed to use the same `ErrorClassifier`
  instead.

## Evidence required

- `Nexus1.BuildingBlocks.Observability.UnitTests` (instrument registration,
  admission-gate accept/reject, cardinality-budget assertion, cached-gauge
  staleness/multi-component behavior) passing in isolation, no host or
  broker involved.
- RootCause `MetricsTests.cs` (in-process `MeterListener` capture) proving
  inbox-outcome branches and the workflow-duration present/absent cases
  against real LocalDB.
- A complete metrics campaign against the real collector's Prometheus
  endpoint, and a deliberately-broken campaign proving the admission gate
  rejects an out-of-vocabulary label rather than admitting a new series —
  both documented in `artifacts/evidence/`.
