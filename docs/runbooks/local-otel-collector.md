# Runbook: running the OpenTelemetry Collector locally

Same constraint as RabbitMQ (`local-rabbitmq.md`): this machine has no admin
rights available to Claude Code sessions, so the collector runs as
**portable binaries**, not a Windows service — started as a regular
background process, stopped by killing that process.

## Why `otelcol-contrib`, not the core `otelcol` distribution

Ch.51's reference collector profile only needs `otlp` receiver, `memory_limiter`/
`batch` processors and a `file` exporter — all present in the small core
`otelcol` distribution. `otelcol-contrib` is used anyway because the metrics
phase (ch.52, a later step) needs the `prometheus` exporter, which is
contrib-only. One collector binary serves both phases rather than two.

## Install location

- Collector binary (portable): `%LOCALAPPDATA%\otelcol-contrib\otelcol-contrib.exe`
  (v0.158.0, downloaded from the `open-telemetry/opentelemetry-collector-releases`
  GitHub releases, Windows amd64 tarball, ~99.4 MB download / ~367 MB extracted).
- Config: `%LOCALAPPDATA%\otelcol-contrib\config.yaml`
- Evidence output: `%LOCALAPPDATA%\otelcol-contrib\evidence\traces.json`
  (structural JSON trace corpus — this file, not a screenshot, is the
  executable evidence for the complete/broken trace campaigns).

## Configuration (Ch.51 Configuration Asset 51-A + Ch.52 Executable Asset 52-S, adapted)

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  memory_limiter:
    check_interval: 1s
    limit_mib: 256
    spike_limit_mib: 64
  batch:
    send_batch_size: 256
    timeout: 5s

exporters:
  file/traces:
    path: C:/Users/USER/AppData/Local/otelcol-contrib/evidence/traces.json
  prometheus:
    endpoint: "0.0.0.0:9464"

extensions:
  health_check:
    endpoint: 0.0.0.0:13133

service:
  extensions: [health_check]
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [file/traces]
    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [prometheus]
```

The `health_check` extension is an addition beyond the book's reference
profile — it gives a pollable `/` endpoint on port 13133 so a startup
sequence can wait for genuine readiness instead of a fixed sleep, matching
this project's existing pattern for RabbitMQ/LocalDB/host readiness checks.

Metrics use a **different exporter shape than traces** — Ch.52's own
reference profile (52-S) exports metrics to a scrapable `prometheus`
endpoint (`:9464`) rather than a retained file, since Prometheus's
pull/scrape model is the evidence mechanism (`curl http://localhost:9464/metrics`
or an HTTP GET, read back as Prometheus text exposition format), not a
newline-delimited JSON corpus. Traces keep the `file/traces` exporter
unchanged — the two signals do not share a pipeline or an exporter.

## Start

```powershell
& "$env:LOCALAPPDATA\otelcol-contrib\otelcol-contrib.exe" --config "$env:LOCALAPPDATA\otelcol-contrib\config.yaml"
```

Run this as a background process (it blocks in the foreground otherwise).
Boots in a couple of seconds; poll health rather than assuming readiness
immediately.

## Check status

```powershell
Invoke-RestMethod http://localhost:13133/
```

`{"status":"Server available", ...}` means it's up. OTLP gRPC listens on
`4317`, OTLP HTTP on `4318` (both `[::]` and `0.0.0.0`).

```powershell
Invoke-WebRequest http://localhost:9464/metrics -UseBasicParsing
```

A `200` with a Prometheus text-exposition body (even an empty one before
any host has exported yet) means the metrics pipeline is up.

## Stop

Find the OS PID from `netstat -ano | findstr :4317` (or the PID printed by
the process launcher) and `taskkill /F /PID <pid>`. No Windows service to
stop — this is a plain process, same as RabbitMQ.

## Evidence file behavior

- `evidence\traces.json` accumulates one JSON object per exported batch
  (newline-delimited, not a single JSON array) — a trace-corpus reader must
  parse it that way, not `JsonSerializer.Deserialize` the whole file as one
  document.
- Delete/truncate the file before a campaign run that needs a clean corpus;
  the collector does not rotate or clear it automatically.
- The file only contains what the OTLP exporter in the .NET host actually
  sent and the collector actually accepted — export failures, dropped
  batches or a collector that was not yet listening all show up as gaps
  here, not as an exception the business code sees (ch.51 "EXPORTER
  INVARIANTS": exporter unavailable -> business continues, evidence
  degrades).

## Metrics endpoint behavior

- `http://localhost:9464/metrics` reflects the collector's **current**
  aggregation state — there is no retained historical corpus the way
  `traces.json` retains every batch. A campaign that needs a clean series
  set restarts the collector (same as clearing `traces.json`), not just
  re-scrapes.
- Counters/histograms accumulate across the collector's process lifetime
  (OTel's cumulative temporality, ch.52 52-Z) — a metric's current value is
  a running total since the last collector restart, not since the last
  scrape. Evidence campaigns read the delta they care about by scraping
  before and after a stimulus, or by restarting the collector for a clean
  baseline.
- A restarted host re-registers its instruments and starts new series from
  zero; the Prometheus exporter does not persist state across a collector
  restart either — both sides starting fresh is expected, not a bug.

## What this does and does not prove

Matches ch.51's own "CONFIGURATION STATUS" caveat: this is a reference
evidence-campaign profile, not a production deployment. It proves the OTLP
export pipeline genuinely works end-to-end (host -> collector -> retained
file corpus) for local evidence campaigns. It says nothing about collector
sizing, retention, multi-instance routing or production backend query
behavior.
