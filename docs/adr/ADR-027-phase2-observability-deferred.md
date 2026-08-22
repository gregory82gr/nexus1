# ADR-027: OpenTelemetry instrumentation deferred for the eleven Phase 2 sectors

## Status

Accepted.

## Context

ADR-013/ADR-014 built the observability foundation
(`Nexus1.BuildingBlocks.Observability`, `AddNexusObservability`, a real
`otelcol-contrib` collector) and instrumented all six Phase 1 contexts
(`ReactorFleet`, `AlarmManagement`, `RootCause`, `Audit`, `Compliance`,
`Reporting`) with tracing and metrics, closed out with a proven complete-
and-broken trace campaign across the real messaging chain.

None of the eleven Phase 2 sectors (`CorePlatform` through
`ReinforcementLearning`) received that same instrumentation. Now that
Phase 2 is complete, the question is whether that gap should be closed
before moving on, or left explicitly open.

**What actually exists today for these eleven sectors**: Domain,
Application, and Infrastructure layers, each proven by unit tests,
component tests against real LocalDB, and a real-host health-check
check. None of them have a caller beyond their own test suites and the
health-check probe itself. Ch.51's own instrumentation targets — PRODUCER/
CONSUMER messaging spans, CLIENT database spans tied to a request, a
traced command-handler surface reachable from outside the process — all
presuppose a request or message arriving from somewhere. For these eleven
sectors, nothing arrives from anywhere yet: no HTTP surface (ADR-007's
Query BFF is itself still deferred), no messaging (only EventManagement/
Maintenance's mutual reconnection and the still-deferred RL advisory
branch involve cross-context messages at all, and neither is instrumented
either), and no UI or external caller of any kind.

## Decision

**Defer OpenTelemetry instrumentation for all eleven Phase 2 sectors.**
`Nexus1.BuildingBlocks.Observability`'s existing catalogue
(`NexusActivitySources`, `SpanNames`/`MetricNames`, `SafeTags`/
`MetricLabels`, `NexusRuntimeMetrics`) is a real, tested foundation
already available to reference — this is not a claim that observability
is hard to add later, only that adding it now would produce spans and
metrics with nothing driving them and no consumer reading them.

This is the same deferral shape as three prior decisions in this
project, not a new kind of restraint:

- **MediatR** (ADR-002-amend) — deferred because hand-rolled dispatch
  already served every real need; no present complexity justified the
  indirection.
- **The Query BFF** (ADR-007) — deferred because `GetActiveAlarmsForUnitQuery`/
  `GetAnalysisByIdQuery` had no external consumer yet; component tests
  already proved them at the Application layer.
- **The RL advisory messaging branch** (ADR-026) — deferred because the
  book's own decision ledger found present evidence insufficient to
  justify even a service boundary, let alone a messaging path.

The common thread: build the layer that has a present, provable need;
name the deferred layer explicitly; record the condition under which it
gets revisited. Instrumenting eleven sectors with no request path to
trace would be the same mistake in reverse — building ahead of a real
need rather than behind one.

## Consequences

- No `Nexus1.BuildingBlocks.Observability` reference is added to any
  Phase 2 sector's Application/Infrastructure project.
- The `Nexus1.ArchitectureTests` dependency rule permitting that
  reference (added for the six Phase 1 contexts under ADR-013) is not
  extended to the eleven Phase 2 contexts yet — extending it now with
  nothing behind it would be scaffolding for a consumer that does not
  exist, the same anti-pattern ADR-007 rejected for the Query BFF.
- `DbContextHealthCheck<T>` remains the only operational signal these
  eleven sectors emit today — sufficient for what they currently do
  (respond to a health probe), not a substitute for tracing once they
  have real callers.
- Phase 2's own evidence reports (build → test → real host → health
  check → evidence → commit, sector by sector) remain the operative
  proof discipline for these contexts until this ADR's reversal
  condition is met.

## Rejected alternatives

- **Instrument all eleven sectors now, for consistency with Phase 1.**
  Rejected — consistency with Phase 1 is not itself a reason when Phase 1's
  own instrumentation was justified by a real messaging chain and command-
  handler surface that these eleven sectors don't yet have. Matching the
  Phase 1 shape without a Phase 1 caller would produce traces of health
  checks and component tests, not of anything a future operator would
  ever want to read.
- **Instrument only the sectors with a real cross-context FK or
  messaging path today (EventManagement/Maintenance's reconnection).**
  Considered — rejected as a partial half-measure: that reconnection is
  itself an in-process EF Core write, not a traced boundary (no HTTP, no
  message), so there is nothing for a span to represent there either that
  isn't already covered by `CLIENT` auto-instrumentation waiting to be
  turned on project-wide once a real reason exists.

## Reversal condition

Revisit sector by sector, not as one blanket re-instrumentation pass,
once any Phase 2 sector starts receiving real external traffic — a Query
BFF or other HTTP surface finally built (reversing ADR-007), a UI or
demonstrator scenario calling into one of these contexts, or the RL
advisory branch's own reversal condition (ADR-026) being met and bringing
a real messaging path with it. At that point, instrument the specific
sector that gained the real caller, using the existing
`Nexus1.BuildingBlocks.Observability` foundation and `AddNexusObservability`
registration exactly as Phase 1 did — no new observability infrastructure
is anticipated, only its extension to a sector that has finally earned
it. This mirrors ADR-007's own reversal condition and ADR-026's advisory-
branch reversal condition exactly: a deferral with a named trigger, not
an open-ended "maybe later."
