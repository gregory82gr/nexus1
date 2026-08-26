# e2e/ — real, unmocked cross-screen tests (Ch. 33)

## What this suite proves

Two real, investigated cross-screen properties, out of the console's full
39-route surface:

1. **Selection propagation.** `PlantStateService.selectedId` is real,
   shared, root-provided state, read by 12 components today. `select uX`
   (NX-Script Console, Ch. 32) is the only real writer. `operator-session
   .e2e.ts` proves a `select u2` genuinely changes what Overview and
   Reactor Kinetics render, live, with no page reload between screens.
   `operator-session.spec-of-specs.e2e.ts` proves this test is load-
   bearing, not decorative, by deliberately breaking the property in
   source, watching the suite fail with a message that names the actual
   mismatch, then reverting and watching it pass again.
2. **Alarm-acknowledge cross-query consistency.** Alarms & Events queries
   `GetActiveAlarmsQuery` (fleet-wide); Overview independently queries
   `GetActiveAlarmsForUnitQuery` (per-unit) for the same real
   `AlarmEvent` data. The one real write this console exposes
   (`acknowledge`) changes state that both screens read separately.
   `operator-session.e2e.ts`'s steps 4-5 prove acknowledging a real
   active alarm for a unit is reflected in Overview's independently-
   fetched `alarmCount` for that same unit.

## What this suite does NOT prove

- **Not exhaustive.** These are the two real cross-screen properties this
  investigation found worth encoding permanently (see the Ch. 33
  investigation report) -- not full coverage of every screen pair on the
  sitemap. Most of the 39 routes have no cross-screen shared-data
  relationship to test in the first place (most read one unit-scoped
  endpoint and nothing else does).
- **Not wired into CI.** No GitHub Actions job runs this suite yet
  (matching the book's own Ch. 34 deferral of that exact step). A green
  suite that only ever runs when someone remembers to run it by hand
  proves less than one gated on every change -- the same caveat Ch. 30's
  hash chain carried before it was ever anchored to anything. Wiring
  this into CI is named, explicitly, as future work, not implied as done.
- **Not mocked, therefore not fast or hermetic.** These tests hit a real,
  running `Nexus1.Bff` and a real LocalDB instance. They are not a
  substitute for the Jest unit/component suite (which is fast, mocked,
  and runs on every `npm test`) -- they are a different kind of evidence,
  the same distinction this whole project has kept between rule-layer
  tests and real-runner-path tests since `SESSION-LOG.md`'s own REQ-04
  incident (adbcheckerequivalent discipline, ported here).

## Running it

Both processes must already be running:

```bash
# Terminal 1 (repo root)
BffContexts__Enabled__0=ReactorFleet BffContexts__Enabled__1=Instrumentation \
BffContexts__Enabled__2=AlarmManagement BffContexts__Enabled__3=RadiationMonitoring \
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5103 \
dotnet run --project src/Hosts/Nexus1.Bff

# Terminal 2 (console/nexus-console)
npm start
```

Then, from `console/nexus-console`:

```bash
npm run e2e
```

`operator-session.e2e.ts` acknowledges one real active alarm belonging to
unit 1 each time it runs (State: Active -> Acknowledged, permanently, in
the real dev database) -- it is not idempotent across unlimited reruns.
It picks whichever unit-1 alarm is first in the real list rather than a
hardcoded id, so it keeps working as long as at least one exists.
