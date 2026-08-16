# ADR-013: Observability foundation — building-block placement, sampler/exporter profile, and the real collector

## Status

Accepted. Closed 2026-08-16 — see "Closing note" at the end of this ADR.

## Context

Ch.51 ("Distributed Tracing with OpenTelemetry") and Ch.52 ("Metrics,
Service Objectives, and Dashboards"), Part IX of the book, apply across the
whole Phase 1 slice already built — every context, both hosts, and the
messaging backbone — not to one context in isolation. Ch.43's TB-07 trust
boundary ("service/collector -> telemetry pipeline") threat-models the
observability path itself: sensitive-data leakage, spoofed signals,
cardinality-exhaustion denial of service, collector-as-lateral-movement,
export-outage-must-not-block-correctness, and telemetry being mistaken for
audit truth (THR-043-016 through THR-043-025).

Two questions had to be settled before any code: where does the shared
instrumentation surface live, and does proving it require a real collector
or is in-process capture enough. Both were raised to the user explicitly
(not picked silently) and confirmed before this ADR was written.

## Decision

### Nexus1.BuildingBlocks.Observability — a new building block, not ServiceDefaults

`Nexus1.ServiceDefaults` currently holds exactly one file
(`DbContextHealthCheck.cs`) and is documented in `CLAUDE.md` as "shared host
composition (health checks, telemetry wiring)". Verified against what the
book actually specifies rather than assumed: the book's
`NexusActivitySources`, `SpanNames`, safe-tag/attribute projection,
`NexusRuntimeMetrics`, `MetricLabels`/`MetricVocabulary`, and
`ProducerTraceSnapshot` (51-A/51-B/51-C/52-A/52-B/51-I) are cross-cutting
types that every context's **Application and Infrastructure** code
references directly (a command handler starts a span; a dispatcher records
a metric) — structurally identical to what `Nexus1.BuildingBlocks.Messaging`
already does for outbox/inbox/AMQP concerns, not to what `ServiceDefaults`
does (one thin extension method called once per host composition root).
Putting the instrumentation catalogue in `ServiceDefaults` would force
every context's Application/Infrastructure project to depend on an
assembly scoped to host composition — a dependency-direction violation
`Nexus1.ArchitectureTests` is built to catch.

**`Nexus1.BuildingBlocks.Observability`** (new project, sibling to
`.Messaging`) holds:

- `NexusActivitySources` — one static `ActivitySource` per context
  (`Nexus1.ReactorFleet`, `Nexus1.AlarmManagement`, `Nexus1.RootCauseAnalysis`,
  `Nexus1.Audit`, `Nexus1.Compliance`, `Nexus1.Reporting`) plus one shared
  `Nexus1.Messaging` source for the cross-cutting publish/consume spans
  that live in `Nexus1.BuildingBlocks.Messaging` itself.
- `SpanNames` / `MetricNames` — closed, reviewed catalogues (51-D/52-C).
- `SafeTags` / `MetricLabels` + `MetricVocabulary` — bounded, allow-listed
  attribute projections (51-E/52-B), enforcing TB-07's confidentiality rule
  directly: `MessageId`/`CorrelationId` may appear as trace attributes but
  are never admitted as metric label values.
- A fail-closed cardinality-budget guard (52-C's `Product()` check).
- `NexusRuntimeMetrics` — the DI-singleton `Meter`/instrument owner.
- `ProducerTraceSnapshot` — the nullable trace-coordinate record captured
  beside an outbox row at commit time (51-I), stored, not read from ambient
  `Activity.Current` by the dispatcher.

`Nexus1.ServiceDefaults` keeps its narrow role and gains exactly one new
extension method, `AddNexusObservability`, that performs the actual
`.AddOpenTelemetry().WithTracing(...).WithMetrics(...)` SDK registration —
genuine host-composition-root code, called once per host `Program.cs`,
same shape as the existing `AddHealthChecks()` call.

`Nexus1.BuildingBlocks.Messaging` is extended (not replaced) with the
bounded AMQP trace-carrier (51-K: validates `traceparent`/`tracestate`
grammar, rejects oversized headers — TB-07's "trace-context injection:
diagnostic only; validate grammar") and PRODUCER/CONSUMER span calls in
`RabbitMqBrokerPublisher` and each context's consumer background service,
since that is exactly where AMQP headers are already read and written.

### Sampler and exporter profile

`AlwaysOnSampler` for local development and evidence campaigns (51-T's
"evidence" profile) — deterministic, not a production sampling-rate
decision, which this project makes no claim about. Ch.51's independence
test (51-D: sampling on vs. off must produce identical DB/outbox/broker
state) is carried forward as a real component test once instrumentation
lands, not merely asserted.

### Real collector, not in-process-only capture

Confirmed with the user: prove the OTLP exporter pipeline itself, not just
in-process `ActivityListener`/`MeterListener` capture. This matches the
project's existing evidence discipline — real LocalDB, real RabbitMQ, now a
real collector — rather than settling for a weaker proof because the
stronger one is more setup.

**Portable `otelcol-contrib` v0.158.0** (Windows amd64 tarball), same
pattern as Erlang/RabbitMQ: no admin rights available to Claude Code
sessions on this machine, so the collector runs as a plain background
process from `%LOCALAPPDATA%\otelcol-contrib\`, not a Windows service.
Ch.51's reference collector profile (Configuration Asset 51-A) is used
almost verbatim: `otlp` receiver (grpc+http), `memory_limiter`+`batch`
processors, `file` exporter writing structural JSON to a local evidence
path. No Prometheus/Grafana wiring yet — that is Ch.52's concern, deferred
until metrics instrumentation is actually built (this ADR covers the
tracing foundation only; a later ADR amendment or new ADR covers the
metrics pipeline when that step starts). `otelcol-contrib` (not the
smaller core `otelcol` distribution) is used because the same binary will
also need the Prometheus exporter for the metrics phase — one collector
binary for both phases, not two.

Setup is documented in `docs/runbooks/local-otel-collector.md`, mirroring
`docs/runbooks/local-rabbitmq.md`'s shape (install location, start/stop,
health check, what it does and does not prove).

### Proof context: RootCause first, then extend

RootCause already owns both a producer path (verdict-issued outbox) and a
consumer path (`AlarmFloodMessageHandler`), plus a command-handler surface
(open/hypothesis/evidence/close) and its own background work
(`RetryDispatcher`) — the smallest single context that exercises every span
kind Ch.51 defines (INTERNAL owner spans, PRODUCER/CONSUMER messaging
spans, CLIENT database spans via automatic SqlClient instrumentation, and
INTERNAL background-work spans). Proven end-to-end against the real
collector with one **complete** trace campaign and one **deliberately
broken** trace campaign (51-A's `DropTraceContextFault`: strip the trace
carrier on one publish, confirm the structural validator classifies exactly
one propagation gap while business state stays correct) before extending
the same shared plumbing to AlarmManagement, Audit, Compliance, and
Reporting.

## Consequences

- `Nexus1.BuildingBlocks.Observability` becomes a new project every
  context's Application and Infrastructure layer may reference — added to
  `Nexus1.ArchitectureTests`' dependency rules the same way
  `Nexus1.BuildingBlocks.Messaging` already is.
- `OutboxMessage` gains four new nullable columns per context
  (`TraceId`/`SpanId`/`TraceFlags`/`TraceState`) for the `ProducerTraceSnapshot`
  — a small migration per context, added incrementally as each context is
  instrumented, not all at once.
- A new local runtime dependency (`otelcol-contrib`) joins RabbitMQ as
  something a fresh environment must stand up before evidence campaigns can
  run; documented the same way.
- Metrics instrumentation (Ch.52) is explicitly deferred to a later step —
  this ADR and the work it authorizes cover tracing only.

## Rejected alternatives

- **Put the instrumentation catalogue in `Nexus1.ServiceDefaults`.**
  Rejected: verified against the book and the project's own dependency
  law — `ServiceDefaults` is host-composition-only; the catalogue is
  consumed from within context code, the same shape as
  `Nexus1.BuildingBlocks.Messaging`, not a host-composition concern.
- **Prove tracing with in-process `ActivityListener` capture only, defer a
  real collector.** Rejected by explicit user decision — the OTLP exporter
  pipeline itself is part of what needs proving, and this project has not
  settled for a weaker proof anywhere else (real broker, real LocalDB).
- **Instrument all six contexts in one pass.** Rejected: matches this
  project's own "no large diff" discipline, same reasoning as building the
  fan-out subscribers one at a time — RootCause proves the shared plumbing
  once before it is repeated four more times.

## Evidence required

- `Nexus1.BuildingBlocks.Observability` unit tests (registration/leak/
  cardinality) passing in isolation, no host or broker involved.
- `otelcol-contrib` running as a portable process, reachable on its OTLP
  endpoint, documented in `docs/runbooks/local-otel-collector.md`.
- A complete-trace campaign against the real chain (flood detected -> auto-
  open consumed -> hypothesis/evidence added -> close/verdict published)
  producing the expected span graph in the collector's exported file
  corpus, validated by a structural oracle (51-H's `ValidateComplete`
  shape).
- A deliberately-broken-trace campaign (dropped carrier on one publish)
  producing exactly one classified propagation gap, with business state
  (RootCauseDb, outbox, inbox) unaffected — proving THR-043-017's
  "compromised or missing telemetry may impair detection, but it must not
  manufacture a business or repair outcome" directly, not just by
  assertion.

## Closing note (2026-08-16)

Tracing is done, across all six contexts, proven at three levels: RootCause
alone (`2026-08-15-observability-tracing-foundation.md`), the four
remaining contexts plus the verdict fan-out
(`2026-08-16-observability-tracing-extension.md`), and the whole chain in
one continuous run, complete and deliberately broken at two independent
hops at once
(`2026-08-16-observability-final-chain-proof.md`). No further tracing work
is planned under this ADR. See ADR-014 for the metrics half's own closing
note — both close together as one observability workstream.
