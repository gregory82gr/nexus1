# Evidence: Angular console, Ch. 32 — NX-Script Console & Help & Guide

## Scope

The last two screens on the sitemap. No backend changes at all this
chapter — both real signals the interpreter exposes were already
reachable through existing endpoints.

1. `features/nx-script/` — a read-only command interpreter: signal
   catalog, pure command parser, pure command evaluator, and the
   component that wires them to the real `ReactorFleetApi`,
   `InstrumentationApi`, and `PlantStateService`.
2. `features/help/` — pure static scope text, no backend call.

Read the full chapter before building. Also checked the project's own
Phase-0 illustrative demo (gregory82gr.github.io/Nexus-1-phase-0, a pure
client-side simulation, not connected to this backend) for the intended
14-signal naming, per direction.

## Investigation, reviewed before writing final code

Unlike every prior chapter, the book's own premise here needed no
fabrication correction — NX-Script is already a refusal-by-design
console in its own source, and Help & Guide is already static. The real
work was determining, signal by signal, which of the demo's 14 named
identifiers this specific backend can honestly answer.

Traced each one directly against real code (not guessed):

- **power** — real, two genuinely different sources at two
  granularities: fleet-wide array from `ReactorFleet.UnitSummaryDto.
  LatestPowerPercent` (`GetUnitsQuery`), and a per-unit Instrumentation
  `POWER`-category signal (the same one Reactor Kinetics already polls).
- **period** — real, client-derived (`deriveRateFromReadings`/
  `periodFromRate`, already built for Reactor Kinetics), from two
  consecutive real readings of the per-unit power-like signal.
- **kin_power** — real, the same per-unit reading `period` uses,
  exposed under its own name per the demo's own dual-tier split.
- **coolant_temp, xenon, thermal_mw, electrical_mw, rod_insert,
  capacity, online, reactivity_pcm, decay_heat, fuel_temp, kin_xenon**
  — 11 total absences, each independently confirmed (no temperature-
  category signal ever seeded; no xenon concept anywhere; no MWe/
  capacity field on the real `Unit` aggregate; electrical fields
  declared absent by Ch. 21's own finding; rod position exists only as
  a Training Mode simulation input; no per-unit operating-status flag
  anywhere).
- **Shared selection state**: `PlantStateService` (`core/state/
  plant-state.ts`) already exists, `providedIn: 'root'`, already read by
  ~10 screens — but nothing writes to it yet (Plant Fleet's own `select`
  stayed local/visual-only). NX-Script's `select uX` is the first real
  caller of the existing `.select()` writer, not new state.
- **Act verbs**: the entire BFF route table has exactly one write
  endpoint — `POST /api/v1/alarm-management/alarms/{id}/acknowledge`.
  `acknowledge` is refused as a real-but-unexposed capability;
  `set/scram/hold/step/wait` correspond to nothing real anywhere and are
  refused as a pure design statement.

Full findings reported and reviewed before building (see prior
investigation-report turn in this session).

## What was built

- `features/nx-script/signal-catalog.ts` — the 14-signal table (`real`
  flag, `absenceReason` per absent signal, tier `fleet`/`kinetics`),
  plus `absenceRefusal`/`offUnitRefusal`/`verbRefusal` helpers. Every
  absence message is the specific, investigated reason for that signal,
  not a shared generic string — 11 absent signals produce 11 distinct
  messages (`coolant_temp`/`fuel_temp` and `xenon`/`kin_xenon` share
  wording only because they are, genuinely, the same underlying gap).
- `features/nx-script/command-parser.ts` — pure syntax layer:
  `get <signal>` / `get fleet.<signal>` / `get uN.<signal>`,
  `sum/mean/max/min(fleet.<signal>)`, `select uN`, and the six act
  verbs. Deliberately signal-agnostic: an identifier outside the
  14-signal vocabulary parses fine and is rejected later, by the
  evaluator, as "unknown identifier" — a different, honest message from
  "recognized but not tracked."
- `features/nx-script/command-evaluator.ts` — the async evaluator.
  Absence check happens first and short-circuits regardless of scope.
  For the two real point-kinetics signals, an off-unit request (`get
  u2.period` while u1 is selected) is refused by name, reusing one
  shared `offUnitRefusal` for both `period` and `kin_power`. `period`
  keys its "last reading" cache by unit id (`Map<number, TimedReading>`)
  — not a single field like the fixed-unit Reactor Kinetics screen —
  since NX-Script can switch units between two `get period` calls, and
  comparing readings across a unit switch would derive a rate from two
  different reactors. `fleet.power` aggregation excludes units with no
  reading rather than treating null as zero, and reports how many of
  how many units actually contributed.
- `features/nx-script/nx-script.ts/.html/.scss` — the console:
  currently-selected-unit display, command input, scrollable
  command/output history. Explicitly discloses on-screen that `select
  uX` changes the unit for every other screen, not just this one — the
  one correction the book's own premise called for.
- `features/help/help.ts/.html/.scss` — static scope text: a
  screen-by-screen reference, an explicit "advisory only, no control
  authority" statement, no backend call anywhere in the component.

## Tests

```
npx jest --testPathPattern "nx-script|command-parser|command-evaluator|features/help"
  -> 4 suites, 28/28 passing
npx jest (full suite) -> 51 suites, 261/261 passing (was 233)
```

- `command-parser.spec.ts`: every grammar shape, including the
  deliberate rejection of `sum(power)` (non-fleet-scoped aggregation) as
  a syntax error rather than a silent bare aggregation, and the
  first-token act-verb check.
- `command-evaluator.spec.ts`: real fleet-wide power array and
  aggregate (excluding a null-reading unit, with an honest "N of M
  units reporting" count); real `kin_power`; `period`'s two-call
  behavior (insufficient-data on the first read, a real derived rate on
  the second); the off-unit point-kinetics refusal; **all 11 absent
  signals asserted individually, confirming 11 distinct message
  strings, never a shared generic one**; `select` writing through to
  the injected `selectUnit`/validated against the real fleet list; the
  two distinct act-verb refusal templates.
- `nx-script.spec.ts` (component, real `HttpTestingController`): the
  same real-vs-absent-vs-verb behavior end to end through the actual
  `ReactorFleetApi`/`InstrumentationApi` HTTP calls, and — the specific
  test the brief asked for — **`select u2` writes to the real,
  shared, root-provided `PlantStateService` instance**, verified by
  reading that exact service via `TestBed.inject(PlantStateService)`,
  not a private copy inside the component.
- `help.spec.ts`: renders with no `HttpClient` provider registered at
  all — if anything in the component tried to inject `HttpClient`,
  `TestBed.createComponent` would throw; it doesn't.

Production build:
```
npx ng build -> 0 errors, 0 warnings. nx-script and help each compile to
                their own lazy chunk (3.50 KB / 1.47 KB transfer).
```

.NET, confirmed unaffected (no backend files touched this chapter):
```
git status --short src/          -> empty
dotnet build Nexus1.Runtime.sln  -> 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln --no-build -> every assembly "Passed!", exit code 0, unchanged
```

## Live evidence

`Nexus1.Bff` composed to `ReactorFleet` + `Instrumentation` only.
Checked the real dev database directly before driving the browser
(`AlarmManagementDb`, which both contexts share per ADR-022):

```sql
SELECT * FROM ReactorFleet.Unit;
-- UnitId 1  UNIT-1  Demonstrator Unit 1
-- UnitId 2  UNIT-2  Demonstrator Unit 2

SELECT UnitId, PowerPercent, RecordedAtUtc FROM ReactorFleet.UnitPowerSnapshot;
-- unit 1 only: 95.0 / 91.25 / 87.5 -- unit 2 has zero snapshots

SELECT UnitId, Tag, Name FROM Instrumentation.Signal;
-- unit 1 only: NX1-U1.RX.POWER (POWER), two NEUTRONICS channels, one TURBINE
-- -- unit 2 has zero signals
```

Two real, already-existing, differently-shaped units — no seeding
needed for this evidence.

Ran real commands in the live console (`/console`, `ng serve` against
the live BFF):

```
> get power
power (u1/UNIT-1) = 95%
> get kin_power
kin_power (u1, NX1-U1.RX.POWER) = 100.1
> get period
period: cannot derive a rate from the two most recent readings for u1
(need two distinct, positive, time-separated readings) -- try again shortly.
> get coolant_temp
coolant_temp: no temperature-category signal is tracked anywhere in this backend
> get fleet.power
fleet.power = [UNIT-1: 95%, UNIT-2: no reading yet]
> mean(fleet.power)
mean(fleet.power) = 95.0% (from 1 of 2 units reporting)
> scram
act verb 'scram' is not available in the read-only console.
> acknowledge
act verb 'acknowledge' is a real capability (alarm acknowledgement) but is not exposed in this read-only console.
```

Honest finding worth naming: `get period` returned the rate-derivation
refusal, not the "only one reading observed" message, on its very first
call in this sequence — because `get kin_power` ran immediately before
it and both read the same per-unit signal cache, and this dev fixture's
`NX1-U1.RX.POWER` reading is a static seeded value (fixed
`latestTimestampUtc`), not something that changes between polls in this
environment. `deriveRateFromReadings` correctly refuses rather than
reporting a fabricated or stale rate when two reads carry the identical
timestamp. Real behavior, real (if unexciting) dev data — not a bug.

**`select uX` write-path proof.** Ran `select u2` in the console —
displayed "Currently selected unit: u2" on that same screen. Then, in
the same browser session, clicked through the app's own sidebar
(Angular router `routerLink`, no URL bar navigation, no page reload) to
Reactor Kinetics — a screen that has never been touched this chapter and
reads `PlantStateService.selectedId` independently. It rendered:

```
NO SOURCE
No power-like signal (CategoryCode POWER or NEUTRONICS) is reporting for this unit.
```

— correctly reflecting unit 2's real state (zero Instrumentation
signals), not unit 1's. This is the exact, genuine write-path proof
required: NX-Script's `select` reached the one shared `PlantStateService`
instance, and an unrelated, pre-existing consumer picked up the change
live, with no reload.

One tool-level note for future sessions: the browser automation's `key`
action needs the literal key name `"Enter"` for Angular's
`(keydown.enter)` host binding to fire — `"Return"` types into the field
correctly but never triggers the handler. Caught by checking the actual
DOM value/focus via `javascript_tool` after an apparently silent first
attempt, not assumed.

### Screenshots

- `nx-script-console.png` — the sequence above rendered live: a real
  result, a real fleet array, a real aggregate, an honest absence
  refusal, and both act-verb refusal types, all visible in one frame.
- `help-guide.png` — static reference content, explicit "STATIC
  REFERENCE — NO BACKEND CALL" label, advisory-only scope statement.

Both reviewed directly before reporting done.

## Summary

Read the full chapter before building. This chapter's investigation was
different in kind from every prior one: not finding a fabrication to
correct, but determining, signal by signal, which of a 14-name
illustrative vocabulary this specific backend can honestly back. 2 are
real (power, and period/kin_power off the same real per-unit signal);
11 are refused with their own specific, investigated reason, never a
generic placeholder and never a fabricated value. `select uX` reuses
real, already-existing shared state for the first time as a real writer,
proven live against a screen that has never been part of this chapter's
own build. Help & Guide needed no correction and got none.
