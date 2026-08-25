# Evidence: Angular console, Ch. 23 — Alarms & Events

## Scope

One real screen, `alarms` route, no new BFF route (list + acknowledge
were already live and proven since the first BFF vertical slice):

1. `AlarmsEventsComponent` (`features/alarms-events/`) — real fleet-wide
   active alarms, grouped by real severity, with a real acknowledge
   write per alarm.
2. No decorative alarm generation ported. No new backend code.

## Investigation

Ch. 23's own subject is the aggregator itself, not a new measurement: its
source mixes 11 hand-written pooled events drawn at random every 5
seconds (decorative — none check any simulated condition) with a real
`#rod-scram` click handler that pushes "Manual SCRAM initiated" the
instant the button is clicked, before any rod has actually moved (a real
trigger that overclaims its own effect).

Checked directly before building:

- **Domain layer** (`Nexus1.AlarmManagement.Domain`): `AlarmEvent.Raise`
  is a bare factory — one non-empty-message guard, no condition check.
  The one genuine trigger, `AlarmDefinition.Evaluate`, does a real
  threshold comparison (`if (sourceValue < ThresholdValue) return null`)
  before calling `Raise`.
- **Application layer**: `EvaluateReadingCommand` is the one command
  that calls `AlarmDefinition.Evaluate` off a real upstream value
  (ReactorFleet's `UnitPowerSnapshotRecordedV1`) — but nothing in this
  solution invokes it automatically. No background service, no event
  handler, no message consumer calls it outside of unit/component tests.
  Alarm-raising in this system is **purely manual command invocation**.
- **SCRAM / rod-control write path — solution-wide check**: zero matches
  for `SCRAM`, `RodScram`, `rod-scram`, `ControlRod`, or any rod-position
  write command anywhere in `src/`. Control Rods are read-only
  everywhere in this console (already established in the Reactor
  cluster's own investigation); the only SCRAM-adjacent code anywhere is
  Training Mode's `drill-runner.ts`, a pure, stateless client-side
  reducer with no backend call. **The book's own premature-firing risk
  does not exist in this system today**, because there is no real state
  change (no rod-move command) for an alarm to get ahead of — named here
  explicitly, not assumed.
- **Live-proven surface**: exactly `GET .../alarms/active` (fleet-wide
  list) and `POST .../alarms/{id}/acknowledge` — both already live and
  proven in the original `2026-08-22-bff-alarmmanagement-read-write-slice.md`
  evidence. All 96 alarms ever seeded there are described in that
  evidence as "real dev-run residue data from earlier Phase 1
  campaigns," not alarms produced by any live upstream trigger.
- **Provenance check**: `AlarmEvent`'s persisted shape (`SourceValue`,
  `ThresholdValue`) has no field recording which command or handler
  raised it. A hand-seeded alarm is structurally indistinguishable from
  one that went through `AlarmDefinition.Evaluate` — confirmed by 8
  direct `AlarmEvent.Raise(...)` call sites across the test suite that
  bypass `Evaluate` entirely.

**Conclusion**: neither of the book's two structural problems is ported.
No timer, no invented pooled events (item 1 of the book's finding). No
premature-firing risk exists to reproduce (item 2), because there is no
rod-write path anywhere in this backend — named explicitly on-screen and
in code, not left implicit. What's real and shown here: the fleet-wide
active-alarms list (95 alarms at this slice's own live check, 2 real
severities) and a genuinely real acknowledge write.

## No new BFF route needed

`GET /api/v1/alarm-management/alarms/active` and
`POST /api/v1/alarm-management/alarms/{id}/acknowledge` (Program.cs,
unchanged) already existed, fully proven, with zero backend code
changes this slice.

.NET build/test: unchanged this slice — no backend code was touched.
```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `core/api/alarm-management-api.ts` — dedicated client for the
  fleet-wide list + acknowledge routes (mirrors `ActiveAlarmSummaryDto`
  and `AcknowledgeAlarmRequest` exactly), not the composite `/overview`
  response's own per-unit alarm field.
- `features/alarms-events/alarm-grouping.ts` — pure `groupBySeverity()`,
  sorted alphabetically, same "group by the real field, never a
  hardcoded rank" discipline as `zone-grouping.ts`.
- `features/alarms-events/alarms-events.ts/.html/.scss` — real
  fleet-wide alarm list grouped by severity, each row with a real
  ACKNOWLEDGE button; the doc comment explicitly declares (1) no
  decorative generation ported, (2) `EvaluateReadingCommand` exists but
  has zero live wiring so displayed alarms are demo residue, not proof
  of live monitoring, and (3) no SCRAM/rod-write path exists anywhere in
  this backend, so the book's own premature-firing risk cannot occur
  here — named so a future action button added to this screen doesn't
  reintroduce it without re-checking. `AcknowledgedByUserId` reuses the
  same placeholder GUID (`1111...1111`) as the original read/write BFF
  evidence session — no login/auth system exists in this console yet.
- `app.routes.ts` — the single `alarms` route now points at
  `AlarmsEventsComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest alarms-events alarm-grouping → 7/7 passing (new specs alone)
npx jest (full suite)                 → 171/171 passing (was 164)
```

- `alarm-grouping.spec.ts` — real-severity grouping, alphabetical sort,
  empty-list case.
- `alarms-events.spec.ts` — loading/error/loaded states, fetches the
  real fleet-wide endpoint, acknowledge posts the real request body and
  refetches the list on success (asserting the acknowledged alarm drops
  off the list on refetch rather than being patched in place client-side
  — proving the effect comes from the server), real error state.

Production build:
```
npx ng build → 0 errors, 0 warnings. alarms-events compiles to its own
               lazy chunk (~2.16 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially, not concurrently. Available memory was checked before
starting the live hosts (1.47 GB — low again) and `dotnet build-server
shutdown` brought it to 1.96 GB before proceeding — same pattern as the
two preceding slices.

## Live evidence — real host, real database, real screenshot, real write

`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=AlarmManagement`; `ng serve --port 4200`
alongside it.

```
GET /health/ready                              → Healthy, HTTP 200
GET /api/v1/alarm-management/alarms/active     → 95 active alarms,
  2 severities (Critical: 11, High: 84) — matching direct SQL count.
```

`/alarms` rendered live, no console errors: banner text exact, 95 active
alarms grouped correctly.

**Exercised the real write through the actual UI**, not curl: clicked
the ACKNOWLEDGE button on the first Critical alarm (`alarmEventId
90030`, unit 9005, "Compliance fan-out test breach."). Network log
confirmed the real request sequence:

```
POST /api/v1/alarm-management/alarms/90030/acknowledge → 200 OK
GET  /api/v1/alarm-management/alarms/active            → 200 OK  (refetch)
```

Confirmed via direct SQL, not just the HTTP response:
```sql
SELECT AlarmEventId, State, AcknowledgedByUserId, AcknowledgedAtUtc
FROM AlarmManagement.AlarmEvent WHERE AlarmEventId = 90030;
```
```
AlarmEventId  State         AcknowledgedByUserId                   AcknowledgedAtUtc
90030         Acknowledged  11111111-1111-1111-1111-111111111111   2026-08-25 07:48:40.39...
```

The on-screen count dropped from 95 to 94 live (`ACTIVE ALARMS` panel,
Critical group from 11 to 10 alarms) — confirming the acknowledged alarm
disappeared from the active list because the server's own state changed,
not a client-side guess.

## Data-cleanliness pass (post-review)

The first screenshot, while entirely real writes, showed leftover
component/e2e-test fixture rows ("Compliance fan-out test breach.",
"Audit e2e test breach.", "HIGH-POWER breached (full-chain complete
campaign).", units `9001`/`9002`/`9004`/`9005`/`9101`/...) — real data,
but not plausible operator-facing text for durable repo evidence.
Cleaned up before the final screenshot:

- **Checked safety of removal first**: `ReactorFleet.Unit` only has real
  units `1` and `2` (`UNIT-1`, `UNIT-2`) — the alarm rows' `UnitId`
  values (`9001`, `9002`, ...) don't correspond to any real plant unit at
  all; they're pure test-harness synthetic IDs (`AlarmEvent.UnitId` is a
  passport-only int, not FK-enforced). Checked `EventManagement.EventAlarmLink`
  (the one real cross-context FK into `AlarmEvent`) and confirmed it
  references exactly one row, `AlarmEventId 90001`, already
  `Acknowledged` — explicitly excluded from the cleanup, everything else
  was safe to remove.
- **Removed**: `DELETE FROM AlarmManagement.AlarmEvent WHERE UnitId NOT
  IN (1,2) AND AlarmEventId <> 90001` — 95 rows (the 94 active
  test-harness alarms plus the one acknowledged earlier in this same
  session), preserving `90001` for `EventAlarmLink`'s FK integrity.
- **Seeded 6 realistic alarms** for the real units (1 Critical/High
  `AlarmDefinition` per unit, then 6 `AlarmEvent` rows), each tied to a
  real concept already established in this project's own prior clusters
  — continuity, not arbitrary text:
  ```
  Critical, unit 1: "Containment radiation monitor RM-CONT-1 exceeds high-dose threshold."
  Critical, unit 2: "Steam generator SG-2 level deviation beyond normal band."
  Critical, unit 1: "Turbine shaft speed sensor UNIT1-TURB-001 signal quality degraded."
  High,     unit 1: "Aux building radiation monitor RM-AUX-1 trending upward."
  High,     unit 2: "Reactor coolant pump vibration above alert threshold."
  High,     unit 1: "Neutron flux channel UNIT1-NI-002 stale, no data received."
  ```
- **Re-verified live**: `GET /api/v1/alarm-management/alarms/active`
  returned exactly these 6, correctly grouped (3 Critical/3 High).
- **Re-confirmed the acknowledge write against the clean rows**:
  clicked ACKNOWLEDGE on `AlarmEventId 1` ("Containment radiation monitor
  RM-CONT-1...") through the actual UI. Network log:
  `POST .../alarms/1/acknowledge → 200 OK`, followed by a real refetch.
  Confirmed via direct SQL:
  ```sql
  SELECT AlarmEventId, State, AcknowledgedByUserId, AcknowledgedAtUtc
  FROM AlarmManagement.AlarmEvent WHERE AlarmEventId = 1;
  ```
  ```
  AlarmEventId  State         AcknowledgedByUserId                   AcknowledgedAtUtc
  1             Acknowledged  11111111-1111-1111-1111-111111111111   2026-08-25 08:00:50.99...
  ```
  On-screen count dropped from 6 to 5 live (Critical group 3 → 2) —
  same real-write confirmation as the original pass, now against clean,
  plausible data.

No component/route/gate changes — this was a data-cleanliness pass
only, per the user's own scoping.

### Screenshot

- `alarms-events.png` — `/alarms`, full-width shell, sidebar correctly
  highlighting "Alarms & Events" active, banner intact, `5` active
  alarms (post-acknowledge state, clean data), 2 Critical + 3 High, each
  row a distinct, plausible operator-facing message tied to a real unit,
  each with a real ACKNOWLEDGE button.

Reviewed directly before reporting done.

Session/database verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x1)
```

One session, matching the one composed context. Both processes stopped
cleanly after capture; `sys.databases` confirmed all 9 databases
`ONLINE` afterward.

## Summary

Checked AlarmManagement's real trigger mechanism and found alarm-raising
is purely manual invocation in this system — the one genuine
threshold-check command exists but is never wired to run automatically.
Checked solution-wide for any SCRAM/rod-write path and confirmed none
exists — Control Rods stay read-only everywhere, so the book's own
"alarm fires before the effect" risk cannot occur here, named explicitly
rather than left to chance. Built a real fleet-wide alarm list (no
decorative generator) with a real acknowledge write, exercised through
the actual UI and confirmed via direct SQL, not just an HTTP 200.
