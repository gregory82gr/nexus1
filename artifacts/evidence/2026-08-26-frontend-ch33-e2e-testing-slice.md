# Evidence: Angular console + backend fix, Ch. 33 — Testing the Whole Console

## Scope

The first committed, real E2E infrastructure for this console, plus a
significant backend correctness fix discovered while building it.

1. **Backend fix**: a real, previously-undiscovered `IUnitOfWork`
   multi-context DI composition bug, affecting every context in the
   solution.
2. **`console/nexus-console/playwright.config.ts`** + **`e2e/`** —
   committed Playwright project (separate from Jest).
3. **`e2e/operator-session.e2e.ts`** — the real 5-step golden-path
   session, spanning 4 screens, both of Ch. 33's investigated real
   cross-screen properties.
4. **`e2e/operator-session.spec-of-specs.e2e.ts`** — proof the
   selection-propagation assertions are load-bearing, with a documented
   red/green run.

Read the full chapter before building. Unlike every prior chapter, the
book's own subject here (E2E test infrastructure encoding a cross-screen
regression) has no fabrication to correct in this port — the real work
was finding this project's own equivalent-value cross-screen property,
since the book's own regression (Incident Analysis vs Root Cause Graph
disagreeing) has no built screen in this project (Ch. 29 Root Cause is
deferred).

## Investigation, reviewed before writing final code

Reported back and reviewed before any build work:

1. **`PlantStateService.selectedId`**: confirmed 12 real component
   consumers today (grew from Ch. 32's own count of ~10-11): Overview,
   Reactor Kinetics, Reactor Instrumentation, Radiation Safety, Power
   Grid, Mission Readiness, Asset Condition, Dependencies, Plant 3D,
   Robotics Fleet, AI Diagnostics, NX-Script Console. `select uX`
   (NX-Script, Ch. 32) remains the only real writer.
2. **Alarm acknowledge cross-screen effect**: no sidebar badge exists
   (checked directly). But Overview independently queries
   `GetActiveAlarmsForUnitQuery` (per-unit) for its own `alarmCount`,
   while Alarms & Events queries `GetActiveAlarmsQuery` (fleet-wide) --
   two real, separate queries over the same real `AlarmEvent` data. This
   is closer to the book's own "write changes state, two screens must
   agree" pattern than PlantStateService's read-only shared config.
3. **Playwright infra**: `@playwright/test` was already a devDependency,
   but zero config, zero test files, zero npm script -- genuinely new
   committed infrastructure was needed, separate from the throwaway
   `capture-*.mjs` screenshot scripts every prior chapter used and
   deleted.
4. Other real "value computed once, displayed twice" candidates found
   (radiation duplicate, signal duplicate, unit power shown in 3 places)
   -- all real, none with a natural write-triggered "break it live"
   mechanism the way alarm-acknowledge has.

Direction: build both real findings into one 5-step session; spec-of-
specs isolated to selection-propagation only (the property with a
natural, realistic silent-regression story).

## Backend fix: the IUnitOfWork multi-context composition bug

Discovered live while wiring the E2E session's real dual-process
backend (not assumed, not theoretical): step 4's alarm acknowledge
returned `200 OK` but the alarm's row never changed state in the real
database when the BFF composed all four contexts Overview needs.

**Root cause, isolated precisely.** `IUnitOfWork`
(`Nexus1.BuildingBlocks.Application.IUnitOfWork`) is one shared
interface. Every one of 14 contexts registers its own `EfUnitOfWork`
(wrapping its own `DbContext`) against that same interface --
`services.AddScoped<IUnitOfWork, EfUnitOfWork>()`, identical pattern
everywhere (confirmed: ReactorFleet, CorePlatform, AlarmManagement,
Instrumentation, DigitalTwin, Maintenance, EventManagement, Robotics,
RadiationMonitoring, EmergencyPreparedness, ReinforcementLearning,
Security, Organization, RootCause -- 14 files, 14 contexts). When 2+ of
these are composed into one process, resolving a single `IUnitOfWork`
constructor parameter always returns the **last-registered** context's
implementation -- so any handler in an earlier-registered context
mutates its own entity correctly, then calls `SaveChangesAsync()` on a
completely different, empty-changeset `DbContext`. A silent no-op that
still reports success.

Minimal repro, confirmed via direct SQL both times:
```
AlarmManagement alone                        -> acknowledge persists correctly
AlarmManagement + RadiationMonitoring (2 ctx) -> 200 OK, zero UPDATE, state unchanged
```

**Severity beyond this chapter**: `Nexus1.ModularRuntime` -- the
project's own always-on runtime -- composes ~16 contexts in one process
(ReactorFleet, CorePlatform, AlarmManagement, Instrumentation,
DigitalTwin, Maintenance, EventManagement, Robotics, RadiationMonitoring,
EmergencyPreparedness, ReinforcementLearning, Audit, Compliance,
Reporting, Security, Organization). This was a live defect in the
project's own production-shaped composition, not only a BFF/E2E-test
concern -- reported to the user before fixing, given the scope.

**Fix**: switched every context's registration to
`AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("ContextName")` (the .NET
8-native mechanism for "one interface, many per-owner implementations"),
and every command handler that injects `IUnitOfWork` (42 files) to
request it via `[FromKeyedServices("ContextName")]`.
`Microsoft.Extensions.DependencyInjection.Abstractions` -- the package
`FromKeyedServicesAttribute` lives in -- was already a direct dependency
of every Application project (confirmed by reading each `.csproj`
before assuming), so this added no new package dependency anywhere.

Live re-verification after the fix, same two-context composition that
failed before:
```
AlarmManagement + RadiationMonitoring + ReactorFleet + Instrumentation (4 ctx)
  -> acknowledge alarm 4 -> 200 OK -> real SQL confirms State=Acknowledged, real AcknowledgedAtUtc
```

## Gates

```
dotnet build Nexus1.Runtime.sln           -> 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln --no-build -> 869 tests across 37 assemblies, 0 failed
                                              (same 869 total as before the fix -- confirmed
                                              via a full untruncated log, not the harness's
                                              own truncated background-task capture)
```

Frontend, unaffected by the backend fix or the new `e2e/` directory:
```
npx jest       -> 51 suites, 261/261 passing (unchanged)
npx ng build   -> 0 errors, 0 warnings
```

`e2e/*.e2e.ts` files are excluded from both the Angular app/spec
tsconfig (`tsconfig.app.json`/`tsconfig.spec.json` both scope to
`src/**`) and from Jest's default testMatch (`*.spec.ts` only) --
confirmed structurally, not merely assumed to not collide.

## What was built

- `playwright.config.ts` -- `testDir: './e2e'`, `workers: 1` (session
  steps share real backend state, deliberately not parallelized), no
  `webServer` entry (both real processes are started manually with
  per-scenario context composition, matching every other live-evidence
  session this project has run).
- `e2e/README.md` -- explicit "what this proves / does not prove"
  boundary: 2 real cross-screen properties demonstrated, not exhaustive
  screen-pair coverage; not wired into CI yet (matching the book's own
  Ch. 34 deferral); not a replacement for the mocked, fast Jest suite.
- `e2e/support.ts` -- shared `runCommand` helper for NX-Script.
- `e2e/operator-session.e2e.ts` -- the real 5-step session:
  1. `select u2` on NX-Script Console -- on-screen confirmation.
  2. SPA-navigate (href-based locators, no page reload) to Overview --
     asserts real, differing content for unit 2 (0 signals, NO READING)
     vs unit 1. Note: Overview itself never renders `unit.code`/`name`
     (checked directly -- not in `overview.html`; the topbar's "Unit 1 -
     PWR-900" is a separate, static placeholder) -- so this uses
     Overview's own real differing data, not a literal unit-code string
     it has no field for.
  3. SPA-navigate to Reactor Kinetics -- asserts the real, honest "No
     power-like signal ... reporting for this unit" for unit 2 (which
     genuinely has zero Instrumentation signals) -- not a forced
     happier assertion.
  4. `select u1` back, Alarms & Events, acknowledges whichever real
     active alarm for unit 1 is first in the list (never a hardcoded
     id), confirms it disappears.
  5. Overview -- asserts its independently-queried `alarmCount` for
     unit 1 decreased by exactly one relative to a baseline captured
     (via a direct API call) before the acknowledge.
- `e2e/operator-session.spec-of-specs.e2e.ts` -- selection-propagation
  steps only (1-3), isolated so proving the property never consumes
  real alarm data.

## Live evidence

Dual-process session: `Nexus1.Bff` composed with ReactorFleet +
Instrumentation + AlarmManagement + RadiationMonitoring (all four --
Overview's endpoint resolves every handler via DI up front and 500s
whole-endpoint otherwise), `ng serve` on 4200.

One locator bug caught and fixed before the first real pass: sidebar
links render with a leading icon glyph (`▣Overview`), so
`getByRole('link', { name: 'Overview', exact: true })` never matched --
switched every sidebar navigation to href-based locators
(`a[href="/overview"]`), robust to the icon prefix.

### Golden path

```
npx playwright test operator-session.e2e.ts
  ok 1 operator session: selection propagates across screens, and an alarm
       acknowledgement is reflected by an independently-queried screen (6.4s)
  1 passed (10.0s)
```

All 5 steps passed against the real, running backend: real `select u2`
write, real differing Overview/Kinetics content per unit, a real
alarm genuinely acknowledged (state persisted, confirmed by the fix
above), and Overview's independently-fetched `alarmCount` for unit 1
genuinely decreasing by exactly one.

### Spec-of-specs: red/green proof

**Baseline (before breaking anything):**
```
npx playwright test operator-session.spec-of-specs.e2e.ts
  ok 1 ... (2.2s)
  1 passed (4.5s)
```

**Break** (`features/overview/overview.ts`):
```diff
- readonly unitId = this.plantState.selectedId;
+ readonly unitId = signal(1); // TEMPORARY Ch.33 spec-of-specs break
```

**Red**, with a message naming the actual mismatch, not a generic
timeout:
```
npx playwright test operator-session.spec-of-specs.e2e.ts
  x  1 selection propagation: select u2 on NX-Script Console changes what
     Overview and Reactor Kinetics render (6.8s)

    Error: expect(locator).toHaveText(expected) failed
    Locator:  locator('.ph').filter({ hasText: 'Live Signals' }).locator('.tag')
    Expected: "0 signals"
    Received: "4 signals"

  1 failed
```

**Revert**, confirmed via `git diff --stat` showing zero content
difference (byte-identical to the committed version, not just visually
similar), then **green** again:
```
npx playwright test operator-session.spec-of-specs.e2e.ts
  ok 1 ... (2.3s)
  1 passed (4.5s)
```

Re-ran the full golden-path suite once more after the revert to confirm
no residual state issue: passed again, 3.5s.

### Real alarm data consumed

The E2E session acknowledges one real active alarm per golden-path run.
Topped up unit 1's active-alarm count (real, valid `AlarmEvent` rows,
same shape as the existing seeded ones, disclosed here rather than
silently) from 3 to 9 before this build/debug session, to survive
repeated runs during development. Final tally after this session: unit
1 has 5 Active + 5 Acknowledged, unit 2 has 2 Active untouched -- plenty
of headroom for future runs, `e2e/README.md` documents this
non-idempotency explicitly.

## Summary

Read the full chapter before building. This chapter's investigation
found the book's own core regression has no equivalent screen pair in
this project (Root Cause deferred) -- so the real work was finding this
project's own two real cross-screen properties worth encoding
permanently: `PlantStateService` selection propagation (12 real
consumers, first real writer from Ch. 32) and alarm-acknowledge
cross-query consistency (Alarms & Events' fleet-wide query vs
Overview's per-unit query, same real `AlarmEvent` data). Building the
live E2E session surfaced a real, previously-undiscovered `IUnitOfWork`
composition defect affecting all 14 contexts and this project's own
production-shaped `ModularRuntime` host, not just this chapter's test
infrastructure -- fixed properly via .NET 8 keyed DI services rather
than worked around, re-verified against the full 869-test suite
(unchanged) before continuing. The resulting suite -- one real 5-step
session and one spec-of-specs proof with a documented, descriptive
red/green cycle -- is committed, but explicitly named as a sample of 2
real properties, not exhaustive coverage, and not yet wired into CI.
