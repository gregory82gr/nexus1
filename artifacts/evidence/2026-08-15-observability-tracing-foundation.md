# Evidence: Observability foundation — tracing (Ch.51), RootCause proof context

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16,
`otelcol-contrib` v0.158.0 (portable, `docs/runbooks/local-otel-collector.md`).

Scope and design decisions are recorded in
`docs/adr/ADR-013-observability-tracing-foundation.md`. This report is the
real proof: `Nexus1.BuildingBlocks.Observability` built and unit-tested in
isolation, `ServiceDefaults.AddNexusObservability` wired into both hosts and
proven against a real `otelcol-contrib` collector (not in-process capture
only), RootCause fully instrumented as the proof context, and both a
**complete** trace campaign and a **deliberately broken** trace campaign
run against the live broker + live hosts + live collector, per Ch.51's own
two-campaign discipline.

## Automated regression: 140/140 passing (was 128 before this step)

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.BuildingBlocks.Observability.UnitTests      9/9  passed  (new —
                                                    source inventory,
                                                    SafeTags/SafeError
                                                    leak-free projection,
                                                    ProducerTraceSnapshot
                                                    round-trip)
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   20/20 passed  (was 17;
                                                    +3 TracingTests —
                                                    in-process
                                                    ActivityListener proof
                                                    of the local span graph
                                                    and outcome tagging)
Nexus1.Audit.ComponentTests                        9/9  passed
Nexus1.Compliance.ComponentTests                   9/9  passed
Nexus1.Reporting.ComponentTests                    9/9  passed
Nexus1.AlarmManagement.ComponentTests              15/15 passed
Nexus1.ArchitectureTests                            7/7  passed  (no
                                                    special-casing needed
                                                    for the new
                                                    Observability project)
```

## Nexus1.BuildingBlocks.Observability — built and proven in isolation first

`NexusActivitySources` (one `ActivitySource` per context plus a shared
`Nexus1.Messaging` source), `SpanNames`, `SafeTags`/`SafeError` (bounded
attribute projection — TB-07's confidentiality rule enforced in code, not
convention), `ProducerTraceSnapshot` (Ch.51 51-I). Registration/leak/
round-trip proven with 9 unit tests, zero host or broker involved, before
any wiring happened.

## ServiceDefaults.AddNexusObservability — proven against the real collector

Both hosts wired (`AlwaysOnSampler`, `AddAspNetCoreInstrumentation`,
`AddHttpClientInstrumentation`, `AddSqlClientInstrumentation`, OTLP
exporter). Confirmed with real spans reaching the collector's file corpus
before any custom instrumentation existed — an HTTP `CLIENT` span (DLX
policy provisioning call) and a `SqlClient` span (health-check query) from
`Nexus1.RootCause.Host`, proving the whole pipeline (SDK → OTLP exporter →
collector → retained file) end to end.

## RootCause instrumentation — every span kind Ch.51 defines

- PRODUCER/CONSUMER spans in `RabbitMqBrokerPublisher` and
  `AlarmFloodConsumerBackgroundService`, with a bounded W3C trace-carrier
  (`AmqpCarrier`, ch.51 51-K) injected/extracted over AMQP headers.
- INTERNAL owner spans in `OpenAnalysisCommandHandler`,
  `AddHypothesisCommandHandler`, `AddEvidenceCommandHandler`,
  `CloseAnalysisCommandHandler`, and the auto-open path inside
  `AlarmFloodMessageHandler`.
- INTERNAL background-work span in `RetryDispatcher`.
- `ProducerTraceSnapshot` captured beside the outbox row at commit time
  (`EfOutboxWriter`, four new nullable columns on RootCause's
  `OutboxMessage` — migration `20260815202412_AddOutboxMessageTraceSnapshot`,
  applied to `RootCauseDb`) and used as an `ActivityLink` when
  `OutboxRelay` eventually dispatches it — never as a parent, matching
  "delayed publish is a new attempt linked to its origin" (ch.51 51-J).

## Proof 1 — complete trace, against the real collector

A throwaway harness (not committed) drove the real chain — flood detected
→ auto-open consumed → hypothesis/evidence added → close/verdict published
— against the live `ModularRuntime` and `RootCause.Host` processes, the
live broker, and a live `otelcol-contrib` collector restarted fresh
beforehand for a clean corpus. The harness's own RootCause command calls
run inside a real started `IHost` (not a bare `ServiceProvider` — a bare
one never starts OpenTelemetry's hosted-service-based `TracerProvider`
construction, which was caught and fixed during this proof, see "Owned"
below), so its own owner spans are recorded too, exactly as a real caller's
would be.

The full span graph, read back from the collector's exported file corpus
and verified structurally (parent/child, not just presence):

```
publish nexus1.alarm-management.alarm-flood-detected.v1   spanId=fd8466e6c6a2fddb
process nexus1.alarm-management.alarm-flood-detected.v1   spanId=7c0335482b984124  parent=fd8466e6c6a2fddb
root-cause case open                                       spanId=f15b8476bdb1de6b  parent=7c0335482b984124
publish nexus1.root-cause.root-cause-case-opened.v1        spanId=b169396e5dfb3296  (linked, not parented)
root-cause add hypothesis                                   spanId=5deed74761c4c8f4
root-cause add evidence                                     spanId=3d81f4c2033a4a32
root-cause verdict commit                                   spanId=211162c562468957
publish nexus1.root-cause.root-cause-verdict-issued.v1      spanId=e88256e6080a3fc1  (linked, not parented)
```

`Assert.Equal(floodPublish.SpanId, floodProcess.ParentSpanId)` and
`Assert.Equal(floodProcess.SpanId, caseOpen.ParentSpanId)` both passed —
propagation genuinely crossed the broker boundary, not merely "both spans
exist." Every owner span carries `nexus1.outcome.code = COMMITTED`. The
exported corpus was scanned for the verdict text and the operator string
used in this run (`"Loose fitting confirmed as cause."`,
`"harness:tracing-e2e"`) — neither appears, confirming `SafeTags`'
allow-list held under a real export, not just in a unit test.

## Proof 2 — deliberately broken trace (dropped carrier), against the real collector

Matching ch.51's `DropTraceContextFault` (51-A), adapted: published an
`AlarmFloodDetectedV1` envelope directly to the broker via a bare AMQP
channel (bypassing `RabbitMqBrokerPublisher`, which always injects the
carrier) with every business field intact but no `traceparent`/`tracestate`
header.

```
Published AlarmFloodDetectedV1 directly with NO trace carrier. MessageId=67b73147-31f5-4b49-bdb6-3d517ed3b43c, AlarmFloodId=639224241772270410.
Business state correct despite the dropped carrier: RootCauseAnalysis auto-opened, AnalysisId=639224232949124437.
Confirmed: CONSUMER span for the dropped-carrier delivery has no parent (new root). spanId=512e755b70f4b4b8
```

Both halves of the proof, checked independently: **(a)** the live
`AlarmFloodConsumerBackgroundService` still auto-opened a real
`RootCauseAnalysis` row correctly — the missing carrier degraded telemetry
only; **(b)** the exported `process nexus1.alarm-management.alarm-flood-detected.v1`
span for that specific message (`nexus1.message.id` tag matched) has no
`parentSpanId` — a genuinely new diagnostic root, exactly Ch.51's
classified gap (51-I: "consumer-creation-context-missing"). This is the
concrete proof of TB-07's THR-043-017 ("compromised or missing telemetry
may impair detection, but it must not manufacture a business or repair
outcome") — not asserted, demonstrated against a real delivery.

## Broker-side evidence — real counters

```
GET /api/exchanges/%2F/nexus.events
  publish_in=20, publish_out=32   (cumulative across this project's whole history)

GET /api/queues/%2F/rootcause.alarm-events.v1
  ack=7, deliver=7, messages=0    (both the auto-open flood-detected delivery
                                    and the deliberately-broken-carrier
                                    delivery were fully consumed and acked)
```

## Owned

- Building the harness surfaced two harness-only bugs, not product bugs,
  both caught before being reported as evidence:
  1. Reading the collector's `traces.json` with `File.ReadAllLines` while
     the collector holds the file open for continuous writes throws
     `IOException`. Fixed by reading with `FileShare.ReadWrite`.
  2. `OpenTelemetry.Extensions.Hosting`'s `AddOpenTelemetry()` registers
     `TracerProvider` construction as an `IHostedService`
     (`TelemetryHostedService`) — it only runs when the host is actually
     started. The harness's first attempt called
     `new ServiceCollection()....BuildServiceProvider()` directly (matching
     the pattern used for AlarmManagement/RootCause command calls in the
     Reporting proof, which never needed observability); that never starts
     hosted services, so no `ActivityListener` was ever registered and the
     harness's own owner spans (add-hypothesis/add-evidence/verdict-commit)
     went unrecorded despite `StartActivity` being called correctly. Fixed
     by building and explicitly starting a real `IHost` for the harness's
     RootCause command calls. Neither bug touched any file under `src/`.
- The retry/DLQ mechanism itself, the fan-out topology, and the
  outbox/inbox dedup machinery are not re-proven here — those already have
  their own real-broker evidence (Audit/Compliance/Reporting reports).
  What needed a *real* proof specific to this step was the tracing
  pipeline itself: genuine OTLP export to a genuine collector, genuine
  propagation across the broker boundary, and a genuine (not simulated)
  missing-carrier gap.
- `RootCauseDb`, `AuditDb`, `ComplianceDb`, `AlarmManagementDb`, `ReportingDb`
  were left in place, same reasoning as every prior step (destructive drop
  correctly gated by the auto-mode classifier; harmless local dev state).
- Both host processes and the collector process were stopped after evidence
  capture; the throwaway harness
  (`tests/Nexus1.DistributedSlice.EndToEndTests/RootCauseTracingHarness.cs`)
  was deleted, leaving only the tracked empty `.csproj` — same pattern as
  every prior fan-out proof.

## Scope explicitly not covered by this step

Metrics (Ch.52) are deferred to a later step per ADR-013 — this step covers
tracing only. AlarmManagement, Audit, Compliance, and Reporting do not yet
have their own owner spans or `ProducerTraceSnapshot` columns — the shared
plumbing (AMQP carrier, PRODUCER/CONSUMER spans in the shared
`RabbitMqBrokerPublisher`/consumer path) already benefits every context
that publishes or consumes through it, but the per-context INTERNAL owner
spans and outbox trace-snapshot columns are RootCause-only until the next
steps extend them context by context, per the confirmed plan.
