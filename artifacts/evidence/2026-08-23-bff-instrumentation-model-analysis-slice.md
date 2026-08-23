# Evidence: BFF seventh vertical slice — Instrumentation, Model Analysis, grouping 2 of 2 (ADR-030 follow-up)

## Scope

Second and final Instrumentation grouping, composed alongside grouping 1
(signals/readings) and all six other contexts in the same host:

- `GET /api/v1/instrumentation/units/{id}/signal-quality` — the book's 7th
  Reactor screen, Model Analysis.

## 1 & 2. Is this a real, distinct grouping, or the same Signal/Measurement shape again?

Checked before building anything, per the task's instruction not to assume
it needs its own endpoint just because the book gives it its own screen.

**It is genuinely distinct** — not a re-slice of grouping 1's data.
`GetOpenSignalQualityEventsForUnitQuery` already existed (keyed by
`UnitCode` string, same inconsistency as grouping 1's existing query — see
tracked cleanup item below), backed by `SignalQualityEvent`: a **separate
aggregate** from `Measurement`, with its own open/close lifecycle
(`StartedAtUtc`/`EndedAtUtc`/`ReasonCode`), representing a period during
which a signal's data was untrustworthy (`BAD`/`STALE`/`UNCERTAIN`). This
answers a different question than grouping 1: grouping 1 shows "the latest
value and its instant quality flag"; this shows "is there an *ongoing*,
unresolved data-quality incident, and why." Different table, different
lifecycle, different question — a real second grouping, not a filtered
view manufactured to match the book's screen count.

This is Instrumentation's own actual "verification" concept: whether a
unit's telemetry can currently be trusted — **not** physics-model
verification (that's DigitalTwin's divergence data, a separate context with
its own already-recorded gap, see the DigitalTwin slice's evidence).

**Added, same minimal-sibling pattern as grouping 1**:
`IOpenSignalQualityEventFinder.GetOpenByUnitIdAsync(int unitId, ...)` —
keyed by int `UnitId` directly (route-shape consistency), reusing the
existing `OpenSignalQualityEventDto` as-is since the projection shape is
identical to the existing UnitCode-keyed query — no new DTO needed. New
`GetUnitSignalQualityEventsQuery`/Handler.

## Hosted-service check

Not re-checked separately — same `Instrumentation.Infrastructure`
registration already confirmed hosted-service-free in grouping 1's
evidence; this grouping adds no new registration beyond the one finder
method.

## Tracked cleanup item (not fixed now, per instruction)

Both of Instrumentation's per-unit queries that predate this task
(`GetActiveHistorizedSignalsForUnitQuery`, `GetOpenSignalQualityEventsForUnitQuery`)
are keyed by `UnitCode` (string), inconsistent with every other BFF route's
`{id:int}` convention and with the new sibling methods added in both
Instrumentation groupings (which are keyed by int `UnitId`). Recorded here
as a tracked cleanup item for later — not fixed in this task, per explicit
instruction. Whenever it is addressed, the natural fix is either renaming/
retyping the two existing queries to take `int UnitId` (breaking their
current callers, if any exist beyond this BFF) or leaving them as-is and
treating the int-keyed siblings as the sole BFF-facing surface going
forward — a decision for whoever picks this up, not decided here.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as all prior slices — zero regressions.

## Real host, real database — live evidence

Memory checked before starting the host (2.82 GB, confirmed stable across
two checks — 2.82 → 2.81 GB). Rechecked after host start (2.79 GB) and
before the endpoint call (2.76 GB) — stable throughout, no incident this
run.

`Instrumentation.SignalQualityEvent` had **zero rows**. Seeded one open
event on `UNIT1-NI-002` — the same signal seeded in grouping 1 with no
measurements yet, giving it a coherent story (never reported, and flagged
stale):

```sql
INSERT INTO Instrumentation.SignalQuality (SignalQualityId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc)
  VALUES (2, 'STALE', 'Stale', 2, 1, SYSUTCDATETIME());
INSERT INTO Instrumentation.SignalQualityEvent (SignalQualityEventId, SignalId, SignalQualityId, StartedAtUtc, ReasonCode, CreatedAtUtc)
  VALUES (1, 2, 2, '2026-08-22T11:00:00', 'NO_DATA_RECEIVED', SYSUTCDATETIME());
```

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/instrumentation/units/1/signal-quality`

```json
[{"tag":"UNIT1-NI-002","qualityCode":"STALE","startedAtUtc":"2026-08-22T11:00:00","endedAtUtc":null,"reasonCode":"NO_DATA_RECEIVED"}]
```

HTTP 200 — the open (unresolved) event for `UNIT1-NI-002` is returned;
`UNIT1-NI-001` (which has real measurements and no quality event) correctly
does not appear.

### `GET /api/v1/instrumentation/units/999/signal-quality` (unit with no events)

```json
[]
```

HTTP 200.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                          status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x4)
```

Confirmed — `nexus1_app`, no fallback.

Host stopped cleanly; `sys.databases` confirmed all `ONLINE` afterward.

## Instrumentation slice — both groupings, final summary

The book's seven Reactor screens map to exactly **two** real groupings in
Instrumentation's domain model, not seven:

1. **Signals + latest readings** (`.../signals`) — covers Core, Control
   Rods, Kinetics, Neutronics, Coolant/TH, Steam Generators (six screens),
   because none of those six are separate domain entities; they're all
   `Signal`/`Measurement` rows differentiated only by tag/category.
2. **Signal-quality/verification** (`.../signal-quality`) — covers Model
   Analysis, Instrumentation's own real "is this telemetry trustworthy"
   concept via the separate `SignalQualityEvent` aggregate.

No endpoints were manufactured to match the book's screen count; two
endpoints were built because the domain model genuinely supports exactly
two distinct shapes of information, confirmed by reading the domain model
before writing either one.

## Overall summary

Instrumentation is now fully composed into `Nexus1.Bff` (seven contexts
total): ReactorFleet, AlarmManagement, DigitalTwin, RadiationMonitoring,
Reporting, Robotics, Instrumentation.
