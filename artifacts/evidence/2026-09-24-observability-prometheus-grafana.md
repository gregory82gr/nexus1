# Evidence — diagnosis observability: Prometheus + Grafana (Appendix I)

**Date:** 2026-09-24
**ADR:** ADR-038
**Scope:** Instrument the RootCause diagnosis/RAG pipeline with Appendix I's four
metrics on a new `Nexus1.Diagnostics` meter, export them to a dev-only Prometheus +
Grafana, scoped to RootCause.Host. `grounding_hit` is an honestly-named PROXY, never
"recall." No route-contract change, no BFF wiring, no alerting, no Angular change.

## Built

| Piece | Location |
|---|---|
| ADR-038 | docs/adr/ADR-038-observability-prometheus-grafana.md |
| `NexusDiagnosticsMetrics` (meter `Nexus1.Diagnostics`, 4 instruments) | BuildingBlocks.Observability |
| Metric names added | BuildingBlocks.Observability/MetricNames.cs |
| Emission from the runner (duration, abstentions, validator_rejections, grounding_hit) | FixedIncidentDiagnosisRunner.cs |
| Prometheus exporter + `Nexus1.Diagnostics` meter + ms-bucket view (shared wiring) | ServiceDefaults/ServiceCollectionExtensions.cs |
| `MapPrometheusScrapingEndpoint()` (RootCause.Host only) | RootCause.Host/Program.cs |
| `OpenTelemetry.Exporter.Prometheus.AspNetCore` 1.17.0-beta.1 | Directory.Packages.props, ServiceDefaults.csproj |
| Dev config: prometheus.yml + Grafana provisioning + dashboard | docs/observability/ |
| Runbook | docs/runbooks/local-prometheus-grafana.md |
| Deterministic metrics tests (3) | tests/…ComponentTests/DiagnosisMetricsTests.cs |

## The four metrics (Appendix I → this repo)

| Appendix I | This repo (meter `Nexus1.Diagnostics`) | Type |
|---|---|---|
| nexus.abstentions | nexus1.diagnosis.abstentions | Counter |
| nexus.validator_rejections | nexus1.diagnosis.validator_rejections | Counter |
| nexus.diagnosis_ms | nexus1.diagnosis.duration (ms) | Histogram |
| nexus.retrieval_recall | **nexus1.diagnosis.grounding_hit** (PROXY, never recall) | Histogram (1.0/0.0) |

Emitted **untagged**, faithful to Appendix I and to avoid expanding the ch.52 reviewed
metric-label vocabulary. Per-incident breakdown deferred.

## Gates

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8** (dependency law intact).
- `Nexus1.RootCause.UnitTests` → **35/35** (runner-construction updated for the metrics param).
- `Nexus1.RootCause.ComponentTests` (deterministic, `!~Ollama`) → **58/58**, incl. the
  **3 new** `DiagnosisMetricsTests`.

## Deterministic emission proof (component tests, no live model, no Prometheus)

`DiagnosisMetricsTests` uses a MeterListener scoped to the runner's exact instrument
instances (identity match, robust against parallel classes):
- **Verdict run:** duration recorded once, grounding_hit = 1.0, abstentions 0, rejections 0.
- **Abstention run** (corpus deleted): abstentions +1, duration recorded, grounding_hit = 0.0
  (retrieval ran, found nothing).
- **Validator rejection** (fixture bad draft naming an unregistered entity → H3): **both**
  validator_rejections +1 and abstentions +1. This is the **deterministic** path
  (fixture bad draft), NOT a claimed live-model hallucination — the same standing
  boundary as OllamaExplainPipelineTests (a temperature-0 local model cannot be reliably
  forced to hallucinate).

## Live stack evidence (real: BFF → Host → Ollama → DB, + Prometheus + Grafana)

Stack: RabbitMQ, Ollama (nexus-dslm + nomic), RootCause.Host `:5102` (with
`/metrics`), BFF `:5103`, Prometheus 3.14.0 `:9090` (portable), Grafana 10.2.3 `:3000`
(portable, provisioned).

### Runs

- `POST …/EVT-2026-0418/diagnoses` → 200, **FV-104**, **92.1 s cold**.
- `POST …/EVT-2026-0419/diagnoses` → 200, **BKR-2A**, 63.7 s.
- Forced **abstention** over HTTP: deleted the CoreT (ch6) historian row (reversibly —
  captured, deleted, **restored** exactly afterwards), `POST …/0418` → 200,
  `abstained: true`, reason "telemetry corroboration failed: missing historian data for
  component 6" (0.03 s — abstains before the model). Row restored and verified.
- Five further warm 0419 verdict runs (~23–31 s each) for live dashboard movement.

### `/metrics` (raw, cross-checked before trusting the dashboard)

```
nexus1_diagnosis_abstentions_total                     1
nexus1_diagnosis_duration_milliseconds_count           8
nexus1_diagnosis_duration_milliseconds_sum             298754.47   (ms, 8 runs)
nexus1_diagnosis_grounding_hit_count                   7
nexus1_diagnosis_grounding_hit_sum                     7
nexus1_diagnosis_validator_rejections_total            (absent — 0 live; deterministic test proves it)
```

Internally consistent: **duration count 8 = grounding_hit count 7 (verdict runs that
reached retrieval) + 1 pre-retrieval abstention**. grounding_hit 7/7 = **100 %**.
An earlier snapshot after the first 3 runs read count 3, sum 155 387 ms — the bucket
distribution showed one sample ≤100 ms (the abstain) and two in the 60–100 s bucket
(the 92 s cold + 63 s warm verdicts), i.e. the bimodal cold/warm split, matching the
122 s/29 s figures from prior evidence reports.

### Prometheus

Target `nexus1-rootcause` (localhost:5102/metrics) **health "up"**. Prometheus 3.x
defaults to UTF-8 metric names (which preserved the dotted OTel names Grafana 10.x
cannot query); `prometheus.yml` sets `metric_name_validation_scheme: legacy` so
Prometheus stores the conventional `nexus1_diagnosis_*` underscore names. Query via the
Grafana datasource proxy returned `nexus1_diagnosis_duration_milliseconds_count = 8`.

### Grafana dashboard (screenshot)

`artifacts/evidence/screenshots/observability-grafana-dashboard.png` — the provisioned
"NEXUS-1 Diagnosis (Appendix I)" dashboard showing the same real numbers:
**Diagnoses run 5 · Abstentions 1 · Validator rejections 0 · Grounding-hit rate
(proxy — NOT recall) 100 %** (a fresh run set after the dashboard fix below: 2 verdicts
+ 1 forced abstention + 2 warm), plus the latency p50/p95 panel showing the cold
(~2 min) vs warm (~40 s) split and the mean-latency/run-count panel. The grounding
panel title reads **"Grounding-hit rate (proxy — NOT recall)"**; nothing in the code,
comments, config, or dashboard uses the word "recall" for it.

### Dashboard fix (two review findings)

Two panel bugs were found in the first screenshot and fixed, both with the same root
cause: `X or vector(0)` on a **labelled** metric returns two series — the real labelled
series AND a phantom empty-label `0` (their label sets never match), so `or` keeps
both. Symptoms: the "Abstentions" stat showed both `1` and `0`, and the mean-latency
panel's legend had two series named "abstentions" (the real one and the phantom). Fix:
wrap the three affected queries in `sum(... or vector(0))`, which collapses to one
series carrying the correct value (N when present, 0 when the metric is absent).
Verified live via the Grafana datasource proxy: `sum(...abstentions... or vector(0))`
returns a single series value 1, `sum(...validator_rejections... or vector(0))` a
single series value 0. The re-captured screenshot confirms one clean number per stat
panel and a single, correctly-named "abstentions" series in the timeseries legend.

## Scope boundary honoured

- No change to the diagnosis/graph **route contracts** — metric emission only.
- No alerting/paging. No Angular console change. No BFF observability wiring (edge
  metrics deferred to ADR-007). No Appendix-J eval harness (true recall stays deferred).
- Dev-only: Prometheus + Grafana are portable binaries under `%LOCALAPPDATA%`, like
  RabbitMQ/Ollama; not committed, not a production/Docker concern. Only the scrape
  config, provisioning, dashboard, and runbook are committed.

## Honest boundaries / what did NOT run

- `validator_rejections` was proven via the **deterministic** fixture path, not forced
  from the live model (stated above).
- `OllamaExplainPipelineTests` was not re-run this slice; the live HTTP runs exercise the
  real model for both incidents. It compiles against the updated runner constructor.
- Dev stack stopped after evidence (Host, BFF, RabbitMQ, Prometheus, Grafana down);
  Ollama left running (not started by this slice).
