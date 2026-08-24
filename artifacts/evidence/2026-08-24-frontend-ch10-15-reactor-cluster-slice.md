# Evidence: Angular console, Ch. 10–15 — Reactor cluster (consolidated)

## Scope

Three screens, per the investigation and recommendation reported and
approved before any code was written:

1. `ReactorInstrumentationComponent` (`features/reactor-instrumentation/`)
   — consolidates Core, Control Rods (read-only), Neutronics, Coolant/TH,
   and Steam & Secondary (five of the book's Reactor sub-screens) behind
   one real component, wired to `GET /api/v1/instrumentation/units/{id}/signals`.
2. `ReactorKineticsComponent` (`features/reactor-kinetics/`) — its own
   real screen, same endpoint, deriving reactor period from real polled
   readings via a refactored-out shared physics module.
3. `ModelAnalysisComponent` (`features/model-analysis/`) — fully
   client-side solver verification, no BFF call, per the explicit
   decision to build the book's own concept rather than the real
   `signal-quality` endpoint (a genuinely different thing, tracked
   separately for a future screen).

## A refactor first: point-kinetics moved to `core/physics/`

Reactor Kinetics and Model Analysis both needed the reactor model
Training Mode (Ch. 9) already built. Rather than have a real-telemetry
screen import from `features/training/` (mixing "simulator" and "live
derivation" namespaces, and awkward directionally even though not
forbidden by the training containment rule), the model moved to
`core/physics/point-kinetics.ts` — matching the book's own file
organization, where one model file is genuinely shared by both Training
Mode and Reactor Kinetics. `features/training/training-sim.ts` and its
spec were deleted; `drill-store.ts`/`drill-runner.ts` now import from the
new location. Two new exports were added for the new consumers:
`fractionalRate` (the rate calculation on its own, for Model Analysis to
check independently) and `deriveRateFromReadings`/`periodFromRate` (the
period formula applied to two real timestamped readings instead of a
simulated state, for Reactor Kinetics). All prior training tests still
pass unchanged after the move (43/43 for training + point-kinetics
combined).

## 1. Reactor Instrumentation — the consolidation

Confirmed directly before building, not assumed: `UnitSignalReadingDto`'s
own doc comment already states there is no `CoreState`,
`ControlRodPosition`, `ReactivityMeasurement`, `CoolantReading`, or
`SteamGeneratorReading` entity in the domain — every one of the five
screens is a filtered view over the same generic `Signal`/`Measurement`
rows. Checked the real seeded `CategoryCode` values across this codebase
(`POWER`, `VIBRATION`, `NEUTRONICS`) before committing to a grouping
strategy: none resemble the book's subsystem names, so grouping by them
would mean inventing a mapping the backend doesn't have. `groupByCategory`
(pure, tested) groups by the real `CategoryCode` instead.

All five routes (`core`, `rods`, `neutronics`, `coolant`, `steam`) point
at this one component (`app.routes.ts`), each passing a `focusLabel` via
route `data` + `withComponentInputBinding()` — orientation only (which
nav entry was clicked), never a data filter; confirmed live that `/core`
and `/rods` render identically except for that label (see live evidence
below).

Control Rods is read-only here, deliberately: the book's own Ch. 10
permanently refuses to let the browser move rods ("not the front end's
decision to make... not ever"), matching this project's own
no-control-authority discipline. There is no rod-command UI to omit,
because none should exist on any Reactor screen, regardless of what a
future endpoint might offer.

## 2. Reactor Kinetics — real period derivation, not a different data source

Same one signals endpoint as above — kept as its own screen not because
of a distinct backend source, but because it does genuine client-side
work the consolidated screen doesn't: reactor period is a rate of change,
and Ch. 11's own point is that a naive raw percent-per-poll delta is a
worse answer than the textbook rate, d(ln P)/dt, applied to real
consecutive readings. `power-signal.ts` (pure, tested) picks the first
live signal whose real `CategoryCode` is power-adjacent (`POWER` or
`NEUTRONICS`, both seen seeded in this codebase) — never guessing from an
arbitrary `Tag` string — and reports `NO SOURCE` honestly if nothing
matches. The component polls the real endpoint every 5s
(`interval(5000).pipe(startWith(0), switchMap(...))`) and derives period
via `core/physics/point-kinetics.ts`'s real-reading functions, applied to
telemetry, not a simulated state.

## 3. Model Analysis — client-side solver verification (decision (a))

No `HttpClient` provider at all in its own spec — proven by omission: if
this component tried to call the backend, DI would throw loudly. Runs
five checks live, in the browser, on page load (`solver-checks.ts`,
pure, tested): critical at zero reactivity, the period formula against
its own defining equation, that doubling reactivity halves the period,
the exact exponential power-growth formula, and the SCRAM half-life.

Named explicitly, in the screen's own header and in code comments, why
this is narrower than the book's own Model Analysis: the book verifies a
numerically-**integrated** six-group solver against closed-form
references (checking for discretization error). This app's shared model
evaluates the exact analytic exponential directly — there is no
discretization error to find, only whether the code matches its own
documented formula. Same verification-vs-validation discipline as the
book either way: this screen can prove the arithmetic is self-consistent;
it cannot and does not claim the model matches any real reactor.

## Tests

```
npx jest   → 97/97 passing (was 69 before this slice; 28 new specs)
```

- `signal-grouping.spec.ts` — real-category grouping, sorted, deterministic.
- `reactor-instrumentation.spec.ts` — loading/error/loaded states, honest
  reporting-count math, and that `focusLabel` changes the header without
  changing the underlying data.
- `power-signal.spec.ts` — picks the right signal, skips one with no
  reading, returns `null` (not a guess) when nothing power-like exists.
- `reactor-kinetics.spec.ts` — polls immediately on creation; derives a
  real period from two distinct fake-timer-driven polls (matches the
  live-evidence mechanism exactly); stays critical when the value never
  changes between polls (naming this as the expected behavior against
  this project's own static/seeded dev data); NO SOURCE when nothing
  power-like is present; real error state on an unreachable endpoint.
- `solver-checks.spec.ts` — every check passes, and expected/actual are
  computed independently rather than one asserting the other blindly.
- `model-analysis.spec.ts` — creates successfully with **no** `HttpClient`
  provider registered at all (the strongest test available that this
  screen makes no backend call), all checks pass, constants render from
  the real shared module, not hardcoded display text.
- `point-kinetics.spec.ts` (relocated + extended) — the original
  `advanceReactor` properties, plus the new real-reading derivation
  functions (correct rate from two distinct readings, `null` on identical
  timestamps, `null` on a non-positive reading).

Production build:

```
npx ng build → 0 errors, 0 warnings. reactor-instrumentation compiles to
               one shared lazy chunk referenced by all five of its
               routes; reactor-kinetics and model-analysis each get their
               own small chunk.
```

One real mistake caught by the build, not by review: the template called
`totalCount()` on what is actually a getter (`get totalCount()`), which
compiles fine in isolation but fails Angular's template type-checking
("this expression is not callable because it is a 'get' accessor") --
fixed to `totalCount` before the build was accepted as green.

## Live evidence — real host, real database, real screenshots

Memory checked before starting both processes (2.92 GB, healthy).
`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Instrumentation`, `__1=ReactorFleet`; `ng serve
--port 4200` alongside it.

```
GET /health/ready                                    → Healthy, HTTP 200
GET /api/v1/instrumentation/units/1/signals           →
  [{"tag":"UNIT1-NI-001","name":"Neutron Flux Channel 1","categoryCode":"NEUTRONICS",
    "latestValue":99.2,"latestQualityCode":"GOOD","latestTimestampUtc":"2026-08-22T10:00:00"},
   {"tag":"UNIT1-NI-002","name":"Neutron Flux Channel 2 (no readings yet)",
    "categoryCode":"NEUTRONICS","latestValue":null,"latestQualityCode":null,"latestTimestampUtc":null}]
```

Same two signals seeded during the earlier backend slice
(`2026-08-22-bff-instrumentation-reactor-signals-slice.md`) -- no
reseeding needed.

**`/core` and `/rods` rendered live**, confirmed via `get_page_text`:
identical body content (same `1 / 2` reporting count, same `NEUTRONICS`
group, same two signal rows) except the header pill reads `CORE` on one
and `CONTROL RODS` on the other -- proving `focusLabel` really is
orientation-only, live, not asserted from the code alone.

**`/analysis` rendered live** with all five checks showing `PASS` and
real computed figures (e.g. "Power at ρ = 30 pcm, t = 4s... expected
112.749685 actual 112.749685"), confirming the checks genuinely execute
in the browser rather than displaying static text.

**`/kinetics` period derivation, caught live**: this took three attempts
to capture correctly, and the difficulty itself is worth recording. The
derived-period state is a genuine single-poll (5s) transient: it appears
for exactly one tick after a new real value arrives, then reverts to
"∞ s" once that value becomes the baseline for the next comparison
(nothing has changed again since the last poll). The first two attempts
tried to catch it via separate tool calls (a `sqlcmd` insert, then a
separate browser check) and missed every time -- the round-trip latency
between issuing the SQL insert and the next tool call consistently
exceeded the 5s window, so by the time the page was checked, the
transient had already come and gone. Confirmed this precisely: an
in-browser `Date.now()`-timed loop showed the poll cadence itself is
correctly ~5s (8002ms elapsed for 2 polls) -- the issue was tool
orchestration latency, not the app.

Fixed by installing a `setInterval`-based logger directly in the page
(`window.__log`), which keeps running in the browser independent of any
tool round-trip, then issuing the SQL insert separately and reading the
accumulated log afterward. That log caught the transient exactly:

```
poll 68: power 91,   period "∞ s"
poll 69: power 87.3, period "-1988.1 s"   <- the transient
poll 70: power 87.3, period "∞ s"          (87.3 is now both readings)
```

A negative period on a power decrease is the correct sign convention (a
real reactor's period is negative while power falls). For the permanent
screenshot record, the same mechanism was reproduced inside a single
Playwright script (SQL insert via `execSync`, then one `waitForTimeout`
before the screenshot) -- no separate-tool latency problem inside one
script -- yielding `reactor-kinetics-derived-period.png`: power `93.5`,
period `+1101.2 s` (positive, power rising), poll 2. The Jest suite's own
`reactor-kinetics.spec.ts` proves the same mechanism deterministically
under fake timers, independent of this live-timing difficulty.

### Screenshots

- `reactor-core-consolidated.png` — `/core`, showing the consolidation
  banner, `1 / 2` signals reporting, the real `NEUTRONICS` group.
- `reactor-rods-consolidated.png` — `/rods`, same data, `CONTROL RODS`
  header, no command controls anywhere on the page.
- `model-analysis.png` — all 5 checks passing with real live-computed
  figures, model constants panel.
- `reactor-kinetics-critical.png` — baseline critical state (∞ s).
- `reactor-kinetics-derived-period.png` — the caught transient described
  above.

All reviewed directly: full-width shell (no regression), correct
two/three-column layouts, no cramped columns, no dead space.

Login/session verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

Both processes stopped cleanly after capture; `sys.databases` confirmed
all 9 databases `ONLINE` afterward.

## Summary

Consolidated five of the book's Reactor screens into one real component
after confirming directly (not assuming) that the backend has one
generic signal model behind all of them, and that the real seeded
category data doesn't support a subsystem-shaped split either. Kept
Reactor Kinetics separate for a genuine client-side reason (period
derivation from real polled readings, not a different data source) and
refactored the shared physics model out of Training Mode into
`core/physics/` to support that honestly. Built Model Analysis as the
book's own client-side verification concept, explicitly narrower than the
book's own six-group solver check since this app's model has no
discretization error to find, and named that narrowing on the screen
itself rather than pretending to check something that doesn't apply. Live
evidence includes a real, if initially elusive, capture of the one-tick
period-derivation transient, and the methodology difficulty (tool
round-trip latency exceeding a 5s poll window) is recorded here alongside
its fix (an in-page logger immune to that latency), not silently worked
around.
