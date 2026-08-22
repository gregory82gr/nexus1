# Evidence: Observability metrics extension — AlarmManagement, Audit, Compliance, Reporting

Date: 2026-08-16
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 / Erlang OTP 27.3.4.16,
`otelcol-contrib` v0.158.0 (portable, `docs/runbooks/local-otel-collector.md`).

Scope: extending Ch.52 metrics (message attempts/duration, inbox outcomes,
the outbox gauge trio, workflow duration where applicable) from RootCause's
proof context (`artifacts/evidence/2026-08-16-observability-metrics-foundation.md`,
ADR-014) to the remaining four contexts — same order and discipline as the
tracing extension (`artifacts/evidence/2026-08-16-observability-tracing-extension.md`).
One checkpoint covering all four, per the user's explicit instruction, since
the shared plumbing (admission gate, cardinality budget, `OutboxMetricState`)
was already proven working across contexts by the RootCause step.

## Automated regression: 194/194 passing (was 186/186 before this step)

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
Nexus1.AlarmManagement.ComponentTests              19/19 passed
Nexus1.Audit.ComponentTests                        13/13 passed  (was 11;
                                                    +2 MetricsTests)
Nexus1.Compliance.ComponentTests                   13/13 passed  (was 11;
                                                    +2 MetricsTests)
Nexus1.Reporting.ComponentTests                    16/16 passed  (was 12;
                                                    +4 MetricsTests)
Nexus1.ArchitectureTests                            7/7  passed
```

## What each context actually needed — verified against real per-context scope, not assumed uniform

Per-context work turned out narrower than "every context gets every
instrument" would suggest, once checked against what each context actually
does:

- **AlarmManagement** — outbox gauge trio only
  (`AlarmManagementOutboxMetricSnapshotReader` + `OutboxMetricRefreshBackgroundService`,
  mirroring RootCause's copies exactly, per-context "duplication until
  proven" as ADR-014 already committed to). AlarmManagement has no inbox
  (it only produces, never consumes) and no workflow-duration ownership
  (RootCause's `CloseAnalysisCommandHandler` already owns the one
  `alarm-to-verdict` measurement point) — those two groups genuinely do not
  apply here, not merely skipped for convenience. "publish" message.attempts
  needed no new code at all: `RabbitMqBrokerPublisher` already records
  every context's outbox dispatch, AlarmManagement's included, since
  RootCause's own step.
- **Audit / Compliance / Reporting** — "process" message.attempts/duration
  (their own `XxxConsumerBackgroundService`, mirroring
  `AlarmFloodConsumerBackgroundService`'s helper pair exactly) + inbox
  outcomes (their own message handler, mirroring `AlarmFloodMessageHandler`'s
  `RecordInboxOutcome` helper). No outbox gauge trio (none of the three own
  an outbox) and no workflow-duration (none owns a terminal workflow fact).
  Reporting's handler already computed the right COMMITTED/DUPLICATE_MATCH/
  ABSTAINED classification per reducer for its tracing INTERNAL spans
  (ADR-013's extension step) — the metrics wiring reuses that same
  `reducerOutcome` value directly for `InboxOutcomes` rather than
  re-deriving it, plus a `REJECTED` outcome for the unsupported-contract
  quarantine branch, which none of the other three contexts have.
- **`retry-dispatch`** (a reviewed `MetricVocabulary.Operations` value since
  ADR-014) remains unused in every context, RootCause included — a retry
  republish flows through the same shared `RabbitMqBrokerPublisher.PublishAsync`
  as any other outbox dispatch, already recorded there as a `"publish"`
  attempt. Noted explicitly rather than silently wired in without being
  asked: the value exists in the reviewed vocabulary for a future step that
  wants to distinguish "logical retry" from "fresh publish" as two separate
  questions (ch.52 52-R's own distinction), not because anything currently
  emits it.

## Evidence-campaign shape: judgment applied, reasoning below

**AlarmManagement got its own dedicated campaign** (outbox gauge trio,
steady-state proof) — the same reasoning as tracing: it is the other
outbox-writing context, and its own reader/refresh-worker copy needed its
own real-collector proof, not inference from RootCause's identical-shaped
code.

**Audit, Compliance, and Reporting shared one fan-out campaign**, reusing
the tracing extension's exact technique (one `RootCauseVerdictIssuedV1`
publish, three independent real deliveries) — for the same reason it was
right for tracing: it is the same real infrastructure event exercising all
three contexts' own hand-copied "process"/inbox-outcome recording code in
one run, cheaper than three isolated flood-to-verdict campaigns and no
weaker evidence, since none of the three needs cross-context structural
proof metrics don't carry (unlike tracing's parent/child claim, a metrics
counter has no shared-graph claim to prove across contexts — each
context's counter is independently correct or not).

**No separate broken/fault campaign was run for any of the four contexts.**
The admission gate (`MetricLabelPolicy`) and the cardinality budget
(`MetricCardinality.Product`) are shared, unchanged code, already proven
against the real collector once at RootCause
(`2026-08-16-observability-metrics-foundation.md`'s Proof 2). Re-running
the identical mechanism against a different context's `NexusRuntimeMetrics`
singleton would exercise the same code path with a different resource
label, not new evidence about a context-specific fault — the honest thing
was to say so rather than pad the report with four repetitions of the same
proof.

## Proof 1 — AlarmManagement outbox gauge trio, against the real collector

A throwaway harness (not committed) drove a real flood detection (seeded
alarm events, `DetectFloodCommandHandler`), let the relay drain the one
resulting outbox row, and scraped `http://localhost:9464/metrics`:

```
nexus1_outbox_pending{...,nexus1_component="Nexus1.AlarmManagement",...} 0
nexus1_outbox_oldest_age_seconds{...,nexus1_component="Nexus1.AlarmManagement",...} 0
nexus1_outbox_snapshot_age_seconds{...,nexus1_component="Nexus1.AlarmManagement",...} 1.4681252
```

Zero pending, checked *beside* a fresh (~1.5s) snapshot age — the same
"zero is healthy only when fresh" case RootCause's own campaign
established, now confirmed for AlarmManagement's own reader/worker copy,
not merely assumed identical because the code looks identical.

### Scope note: the mid-flight non-zero window was not captured, and why

An earlier design for this campaign tried to also capture a genuine
non-zero *mid-flight* pending reading — seeding a batch of undispatched
rows and racing the relay's drain against the 2s metrics export interval.
That turned out harder to demonstrate honestly than expected: the real
local relay drains any seed size fast enough that catching it mid-flight
became a coin flip against the export cadence, and enlarging the seed to
widen the window (800 rows) introduced a second, worse problem — a
concurrent polling `COUNT` query on the same table blocked behind the
relay's own read/write cycle under LocalDB's locking (one query observed
at 23 seconds; not a slow query plan, genuine lock contention from running
a reader against the same rows the relay was actively updating). Rather
than keep fighting that race, this campaign settled for the same
steady-state proof RootCause's own campaign already established. This is a
real scope reduction from the original ambition, recorded honestly rather
than silently dropped.

## Proof 2 — verdict fan-out metrics, against the real collector

The same fan-out harness shape as tracing's (RootCause opens, adds a
hypothesis and evidence, closes — driving a real `RootCauseVerdictIssuedV1`
publish) with a composed Audit+Compliance+Reporting host, scraping
Prometheus instead of reading the trace corpus:

```
nexus1_inbox_outcomes_total{...,nexus1_component="Nexus1.Audit",...,nexus1_outcome="COMMITTED",...} 3
nexus1_inbox_outcomes_total{...,nexus1_component="Nexus1.Compliance",...,nexus1_outcome="COMMITTED",...} 3
nexus1_inbox_outcomes_total{...,nexus1_component="Nexus1.Reporting",...,nexus1_outcome="COMMITTED",...} 73
nexus1_message_attempts_total{...,nexus1_operation="process",...} 337
```

The counts are cumulative since the collector's last restart, not per-run —
RabbitMQ's durable quorum queues retained backlog from earlier campaigns in
this same session (Reporting's wildcard `root-cause.#` binding in
particular accumulates every root-cause event any prior harness run in
this session ever published), so these are large, not per-scenario, counts.
The assertions this campaign actually checks are existence and
monotonic-increase style (`>= 1` per component, `>= 3` total process
attempts), not exact values — consistent with Ch.52 52-Z's own point that
counters are cumulative sums requiring a rate/delta query, not a snapshot
comparison, to mean anything about "this run" specifically. What matters is
confirmed: Audit's, Compliance's, and Reporting's own independently-written
"process"/inbox-outcome recording code all fired correctly from the same
one real delivery.

## Owned

- **A harness-only culture-parsing bug**, not a product bug: `double.Parse`
  without `CultureInfo.InvariantCulture` misread a Prometheus-format value
  like `"2.3467655"` under the local machine culture (dropping the `.` as
  if it were a thousands separator, producing `23467655`). The underlying
  metric value was correct throughout; only the harness's own assertion
  parsing was wrong. Fixed in both new harnesses.
- **A harness-only timing/ordering bug**: seeding alarm events with a
  timestamp captured *before* building and starting the host pushed the
  oldest seeded event outside the 30-second flood-detection window once
  host startup itself took long enough to matter, so `DetectFloodCommandHandler`
  legitimately found no flood to detect. Fixed by capturing the seed
  timestamp *after* `host.StartAsync()` returns, not before.
- **A harness-only lock-contention finding**, described above under Proof 1
  — informative in its own right (a concurrent LocalDB reader can
  meaningfully slow a writer sharing the same rows), but not a product
  defect since no production code path does what the harness's polling
  loop did.
- `AlarmManagementDb`, `AuditDb`, `ComplianceDb`, `ReportingDb`, and the
  underlying RabbitMQ queues were left in place — harmless local dev/session
  state, same reasoning as every prior step (the accumulated queue backlog
  described in Proof 2 is a direct, visible consequence of this policy,
  not a surprise).
- Both harnesses and the collector process were stopped after evidence
  capture; `AlarmManagementOutboxMetricsCampaignHarness.cs` and
  `VerdictFanOutMetricsCampaignHarness.cs` were deleted, leaving only the
  tracked empty `Nexus1.DistributedSlice.EndToEndTests.csproj` — same
  pattern as every prior real-collector proof in this project.

## Scope explicitly not covered by this step

Same residuals as the foundation step, still true across all six contexts
now: `nexus1.edge.requests` (no BFF), `nexus1.projection.lag` (Reporting is
the one context that genuinely owns a projection, and still does not have
this instrument — a real gap, not an oversight, deferred the same way it
was at the foundation step), Prometheus recording rules, the educational-
objective evaluator, and a provisioned Grafana dashboard. All six contexts
now have tracing parity *and* the metrics groups applicable to them; no
context has every Ch.52 instrument the book's full chapter defines.

This closes step 6 of ADR-013's plan (metrics, extended to every context
that needs it) — the final cross-cutting proof spanning the whole chain,
if wanted next, is a distinct, later step, not part of this one.
