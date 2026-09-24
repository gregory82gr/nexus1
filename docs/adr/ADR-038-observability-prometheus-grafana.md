# ADR-038: Diagnosis observability — Prometheus + Grafana (Appendix I)

## Status

Accepted. Builds on ADR-013/ADR-014 (the existing OpenTelemetry tracing + metrics
plumbing, Ch.51/52) and ADR-007 (the deferred BFF/edge concern).

## Context

`From Flood to Cause` Appendix I ("Observability Bootstrap") instruments **the RAG
system, not the plant**: how long a diagnosis took, whether retrieval found grounding,
how often the engine abstained, how often the validator rejected. It specifies four
instruments on a meter `Nexus1.Diagnostics` — `nexus.abstentions` (counter),
`nexus.validator_rejections` (counter), `nexus.diagnosis_ms` (histogram),
`nexus.retrieval_recall` (histogram) — and an OpenTelemetry → Prometheus → Grafana
wiring (`AddPrometheusExporter()`, `MapPrometheusScrapingEndpoint()` → `/metrics`).

State of this repo before this slice:
- `AddNexusObservability()` (ADR-013/014) wires tracing + one meter, `Nexus1.Runtime`,
  whose instruments are **messaging/workflow-only** (`nexus1.message.*`,
  `nexus1.inbox.*`, `nexus1.outbox.*`, `nexus1.workflow.duration`,
  `nexus1.telemetry.rejected`). Export is **OTLP-only** (to a collector at :4317).
- The **diagnosis/RAG pipeline** (`FixedIncidentDiagnosisRunner` and its seams —
  walker, corroborator, retriever, validator, explainer) has **no metrics and no
  tracing spans**. It is a different surface from the instrumented messaging backbone.
- **Prometheus/Grafana packages are absent**; there is no `/metrics` endpoint anywhere.

So Appendix I's four metrics are all genuinely new instrumentation, and Prometheus
export is a second, orthogonal gap.

## Decision

Instrument the diagnosis pipeline with a new meter and export it to a local,
dev-only Prometheus + Grafana, scoped to RootCause.Host.

### 1. New `Nexus1.Diagnostics` meter, four instruments

A new `NexusDiagnosticsMetrics` (in `Nexus1.BuildingBlocks.Observability`), meter name
`Nexus1.Diagnostics` (Appendix I's own meter name), with four instruments named per
this repo's `nexus1.*` catalogue convention (the book's `nexus.*` names are noted in a
comment), added to `MetricNames`:

- `nexus1.diagnosis.abstentions` — `Counter<long>` (Appendix I `nexus.abstentions`)
- `nexus1.diagnosis.validator_rejections` — `Counter<long>` (`nexus.validator_rejections`)
- `nexus1.diagnosis.duration` — `Histogram<double>`, unit **ms** (`nexus.diagnosis_ms`)
- `nexus1.diagnosis.grounding_hit` — `Histogram<double>` recording **1.0/0.0** per run
  that reached retrieval (Appendix I's `nexus.retrieval_recall` **slot**, deliberately
  renamed — see §4).

**Untagged, per Appendix I.** The book's instruments carry no dimensions, and this
repo already runs a strict reviewed metric-label vocabulary + admission gate (ch.52
52-F, `MetricLabelPolicy`). Rather than expand that vocabulary, the diagnosis
instruments are emitted **untagged**: aggregate abstention/rejection counts, the
duration distribution (which shows the bimodal cold/warm split without any tag), and
the grounding-hit fraction are exactly Appendix I's "four numbers whose quiet drift is
the first sign." A per-incident (0418 vs 0419) breakdown would require extending the
reviewed vocabulary and is **deferred** as a separate concern.

### 2. Emitted from `FixedIncidentDiagnosisRunner`

The runner already observes verdict-vs-abstain, the H3/H4 validation outcome, and can
time `RunAsync`:
- `duration` recorded (ms) for **every** run, at the single `FinishAsync` exit.
- `abstentions` incremented whenever a run abstains (any gate).
- `validator_rejections` incremented specifically when H3/H4 validation fails (a
  subset of abstentions — both counters move, matching Appendix I's separate counters).
- `grounding_hit` recorded (1.0 if retrieval returned ≥1 passage above the H2 floor,
  else 0.0) **only for runs that reached retrieval** — a run that abstained earlier
  (no origin, telemetry) never measured retrieval, so recording 0 would conflate "found
  nothing" with "never ran." Absence of a sample is honest.

### 3. Prometheus exporter alongside OTLP; endpoint on RootCause.Host only

`AddPrometheusExporter()` is added **additively** to the shared `AddNexusObservability`
metrics pipeline (the existing `Nexus1.Runtime` metrics become scrapeable too, a free
side benefit), and `AddMeter("Nexus1.Diagnostics")` is registered there. The existing
OTLP exporter stays (tracing + the ch.52 campaigns are unchanged). Only
**RootCause.Host** calls `MapPrometheusScrapingEndpoint()` → `/metrics`;
ModularRuntime and the BFF are untouched.

**Scope: RootCause.Host only.** All four metrics originate there (the runner and the
in-process `SemanticKernelExplainer` both run in RootCause.Host; `Nexus1.RootCause.Explain`
is a library, not a separate host). The **BFF is deliberately not wired** — it has no
observability today (ADR-030), and BFF proxy latency / 502-vs-503 edge counts are the
`nexus1.edge.requests` concern **explicitly deferred to ADR-007**, a separate future
slice, not Appendix I's four metrics.

### 4. `grounding_hit` is a proxy, never "recall"

Appendix I's own honest boundary: `retrieval_recall` is only meaningful against a
labelled set (H9 / Appendix J's eval harness), which **does not exist** in this repo.
Emitting a metric called "recall" without that set would claim a measurement this
project cannot make. So the `nexus.retrieval_recall` slot is filled by
`nexus1.diagnosis.grounding_hit` — an honestly-named **proxy**: did retrieval return ≥1
passage above the H2 relevance floor (1/0). It is never called or described as "recall"
in code, comments, or the dashboard. True recall is **deferred** to a future
Appendix-J slice that builds the labelled golden set.

### 5. Dev-only Prometheus + Grafana (portable, like RabbitMQ/Ollama)

Prometheus and Grafana run as **portable binaries** started as background processes —
no Docker, no admin/service install — matching exactly how RabbitMQ and Ollama are run
on this box (no admin rights for the session). A new runbook
`docs/runbooks/local-prometheus-grafana.md` (sibling to `local-rabbitmq.md`) documents
it. Committed dev config: `docs/observability/prometheus.yml` (job `nexus1-rootcause`,
15s, `/metrics`, `localhost:5102`) and a **provisioned** Grafana dashboard + datasource
under `docs/observability/grafana/`. Building the provisioned dashboard closes a named
residual in CLAUDE.md §5 ("a provisioned Grafana dashboard").

## Consequences

- The diagnosis pipeline's health (abstention rate, validator rejections, latency
  distribution, grounding-hit rate) is observable in Grafana, matching the empirical
  cold ~122 s / warm ~29 s latency split we already measured.
- Existing `Nexus1.Runtime` messaging metrics also become Prometheus-scrapeable.
- Tracing, the OTLP export, and the ch.52 campaigns are unchanged.
- A second system (Prometheus + Grafana) is real operating weight — dev-only, run by
  hand, exactly like the broker and the model.

## Scope boundary

- No change to the diagnosis/graph **route contracts** — metric emission only.
- No alerting/paging (no Alertmanager). The book's "a number you can alert on" is noted,
  not built.
- No Angular console change. No BFF observability wiring. No Appendix-J eval harness
  (so true recall stays deferred).
- Dev-only infrastructure — not a production/Docker deployment concern.

## Rejected alternatives

- **Tagging the diagnosis metrics by incident/outcome** — rejected for this slice;
  would expand the reviewed ch.52 vocabulary. Appendix I's instruments are untagged and
  the aggregate numbers meet the goal.
- **Calling the proxy "recall"** — rejected; there is no labelled set, so it would be a
  claimed measurement the project cannot make (§4).
- **Prometheus scraping the OTLP collector** — rejected; Appendix I's pattern is the
  host exposing `/metrics` directly, which is simpler and needs no collector for this path.
- **Wiring the BFF / edge metrics now** — rejected; deferred to ADR-007.
- **Docker Compose for Prometheus/Grafana** — rejected; inconsistent with how RabbitMQ
  and Ollama are already run here (portable binaries).

## Reversal condition

- Revisit `grounding_hit` → real `retrieval_recall` when Appendix J's labelled golden
  set lands.
- Revisit untagged → per-incident dimensioning if/when the ch.52 vocabulary is extended.
- Revisit portable binaries → Docker if the project adopts Docker for dev dependencies
  (Ch.55).

## Evidence required

- Both incidents (0418, 0419) run live → `diagnosis.duration` populated with real
  cold+warm samples; `/metrics` count == number of runs; values match prior evidence
  (122 s cold / 29 s warm).
- A deliberate abstention over HTTP → `abstentions` increments in `/metrics` and Grafana.
- `validator_rejections` increments via the deterministic fixture-bad-draft path
  (stated as the deterministic path, not a claimed live-model hallucination).
- Grafana screenshot reflecting the same real numbers.
- `grounding_hit` labelled a proxy, never "recall," everywhere.

See `artifacts/evidence/2026-09-24-observability-prometheus-grafana.md`.
