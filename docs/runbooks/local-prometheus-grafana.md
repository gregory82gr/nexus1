# Runbook: local Prometheus + Grafana for the diagnosis metrics (Appendix I)

Dev-only observability for the RootCause diagnosis/RAG metrics (Appendix I; ADR-038).
Like RabbitMQ and Ollama on this machine, Prometheus and Grafana run as **portable
binaries** started as background processes — no Docker, no admin/service install (this
box has no admin rights for the session). Prometheus scrapes RootCause.Host's
`/metrics` endpoint; Grafana reads Prometheus.

## Scope

RootCause.Host only. The BFF and ModularRuntime are deliberately not scraped
(BFF/edge metrics are deferred to ADR-007). The four metrics are:

- `nexus1.diagnosis.abstentions` — abstention count
- `nexus1.diagnosis.validator_rejections` — H3/H4 rejection count
- `nexus1.diagnosis.duration` (ms) — diagnosis latency
- `nexus1.diagnosis.grounding_hit` — a **grounding-hit PROXY, never recall** (true recall
  needs a labelled set / Appendix J, which does not exist yet)

## Install locations (this box)

- Prometheus 3.14.0 (portable): `%LOCALAPPDATA%\prometheus\prometheus-3.14.0.windows-amd64\`
  (download: `github.com/prometheus/prometheus/releases`, `*.windows-amd64.zip`)
- Grafana 10.2.3 OSS (portable): `%LOCALAPPDATA%\grafana\grafana-v10.2.3\`
  (download: `dl.grafana.com/oss/release/grafana-10.2.3.windows-amd64.zip`)
- Committed dev config (this repo): `docs/observability/`
  - `prometheus.yml` — scrape config (job `nexus1-rootcause`, `localhost:5102`, `/metrics`)
  - `grafana/provisioning/datasources/prometheus.yml` — the Prometheus datasource
  - `grafana/provisioning/dashboards/nexus1.yml` — the dashboard provider (absolute
    `path:` — adjust to your checkout)
  - `grafana/dashboards/nexus1-diagnostics.json` — the dashboard

## Prometheus 3.x metric names (important)

Prometheus 3.x defaults to UTF-8 metric names, which preserves OpenTelemetry's dotted
names (`nexus1.diagnosis.*`) that Grafana 10.x cannot query. `prometheus.yml` therefore
sets `global.metric_name_validation_scheme: legacy`, so Prometheus sanitizes to the
conventional underscore names (`nexus1_diagnosis_*`) the dashboard uses. (The bare
`/metrics` exposition already shows underscore names; the scheme controls what
Prometheus 3.x negotiates and stores.)

## Prerequisites

The full RootCause stack up (so `/metrics` has data to scrape): RabbitMQ, Ollama,
RootCause.Host on `:5102`. See `local-rabbitmq.md` and
`local-rootcause-diagnosis-provisioning.md`.

## Start Prometheus

```powershell
& "$env:LOCALAPPDATA\prometheus\prometheus-3.14.0.windows-amd64\prometheus.exe" `
  --config.file="C:\dev\nexus1\docs\observability\prometheus.yml" `
  --storage.tsdb.path="$env:LOCALAPPDATA\prometheus\data" `
  --web.listen-address="localhost:9090"
```

Run as a background process. UI/API at http://localhost:9090. Confirm the target is up:
`GET http://localhost:9090/api/v1/targets` → job `nexus1-rootcause`, `health":"up"`.

## Start Grafana

```powershell
$env:GF_PATHS_PROVISIONING = "C:\dev\nexus1\docs\observability\grafana\provisioning"
$env:GF_AUTH_ANONYMOUS_ENABLED = "true"   # dev-only convenience
$env:GF_AUTH_ANONYMOUS_ORG_ROLE = "Admin"
$home = "$env:LOCALAPPDATA\grafana\grafana-v10.2.3"
& "$home\bin\grafana-server.exe" --homepath "$home"
```

Run as a background process. UI at http://localhost:3000 (default admin/admin). The
Prometheus datasource and the "NEXUS-1 Diagnosis (Appendix I)" dashboard
(uid `nexus1-diagnostics`) are provisioned on start. If you edit the committed
dashboard JSON while Grafana runs, force a reload:
`POST http://localhost:3000/api/admin/provisioning/dashboards/reload`.

## Generate data

Run diagnoses (see `local-rootcause-diagnosis-provisioning.md`):

```bash
curl -X POST http://localhost:5103/api/v1/root-cause/incidents/EVT-2026-0418/diagnoses
curl -X POST http://localhost:5103/api/v1/root-cause/incidents/EVT-2026-0419/diagnoses
```

The dashboard's stat panels update within a scrape interval (15s).

## Stop

Find the PID on the port and `Stop-Process`:

```powershell
Get-NetTCPConnection -State Listen -LocalPort 9090 | % { Stop-Process -Id $_.OwningProcess -Force }  # Prometheus
Get-NetTCPConnection -State Listen -LocalPort 3000 | % { Stop-Process -Id $_.OwningProcess -Force }  # Grafana
```

There is no Windows service — these are plain processes.

## Honest boundaries

- Dev-only. No alerting/paging (no Alertmanager). Not a production deployment.
- `grounding_hit` is a proxy for retrieval health, **not** recall (ADR-038).
- The Grafana admin credentials here are the dev defaults; do not reuse them anywhere real.
