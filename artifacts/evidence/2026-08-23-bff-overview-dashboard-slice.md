# Evidence: BFF eighth vertical slice — Overview (Plant Overview / Dashboard), the first cross-context composition (ADR-030 follow-up)

## Scope

`GET /api/v1/overview/units/{id}` — composes four already-existing per-unit
queries from four different contexts into one dashboard response. Unlike
every prior slice, this endpoint adds **zero new Application-layer code to
any context** — it reuses:

- ReactorFleet: `GetUnitByIdQueryHandler` (`GetUnitByIdQuery`) → `UnitDetailDto?`
- AlarmManagement: `GetActiveAlarmsForUnitQueryHandler` (`GetActiveAlarmsForUnitQuery`)
  — the **original, pre-BFF, per-unit** query (not the fleet-wide
  `GetActiveAlarmsQueryHandler` built for AlarmManagement's own slice) →
  `IReadOnlyList<ActiveAlarmDto>`
- RadiationMonitoring: `GetUnitRadiationSafetyQueryHandler` (built for that
  slice) → `UnitRadiationSafetyDto`
- Instrumentation: `GetUnitSignalReadingsQueryHandler` (built for that
  slice's grouping 1) → `IReadOnlyList<UnitSignalReadingDto>`

New code, all in `Nexus1.Bff` itself (the composition root — no single
context's Application layer is the right home for cross-context
composition; that's what a BFF is for): `OverviewDto`, the `/api/v1/overview/units/{id}`
endpoint, and a small `SafeCallAsync` helper.

## Design question 1: parallel vs sequential — resolved, and proven

**Decision**: all four calls start immediately and are awaited via
`Task.WhenAll`, not sequentially. This is safe because each call resolves a
genuinely separate scoped `DbContext` instance (`ReactorFleetDbContext`,
`AlarmManagementDbContext`, `RadiationMonitoringDbContext`,
`InstrumentationDbContext` are four distinct types, each with its own
instance per request scope) — no single `DbContext` instance is ever used
concurrently from two places, which would be unsafe; using four different
instances concurrently is standard, safe EF Core practice.

**Proof, not just the async code "hopefully" running concurrently**:
temporarily instrumented each of the four calls with a distinct artificial
delay (200ms / 400ms / 600ms / 800ms on unit / alarms / radiation / signals
respectively) plus per-call `Stopwatch` timing logged to console, called the
endpoint once JIT-warm, then reverted both the delays and the logging
immediately after capturing the result.

```
[Overview-Timing] unit: 254ms
[Overview-Timing] alarms: 407ms
[Overview-Timing] radiation: 608ms
[Overview-Timing] signals: 795ms
[Overview-Timing] TOTAL: 795ms
```

If these ran sequentially, `TOTAL` would be ≈ the sum (254+407+608+795 ≈
2064ms). It is instead ≈ the **max** of the four (795ms ≈ signals' own
795ms) — decisive, unambiguous proof of real concurrency, not just
non-blocking-looking code. `curl`'s own wall-clock timer independently
confirmed this from outside the process: `0.903s` total for that same call.

The instrumentation (artificial delays + `Console.WriteLine` calls) was
then fully removed; the shipped code has neither.

## Design question 2: partial-failure behavior — decided, implemented, and proven live

**Decision**: partial success with explicit per-section marking, not
whole-endpoint failure. Reasoning: this is a dashboard screen — three
working sections and one clearly-marked failure is more useful to an
operator than a 500 for the whole page over one context's transient issue.
Each of the four calls is wrapped (`SafeCallAsync`) so a thrown exception
becomes `(null, "the exception message")` instead of propagating; the
failed section's DTO field is `null` and its error text is recorded in a
sibling `Errors` dictionary keyed by section name (`"unit"`, `"activeAlarms"`,
`"radiation"`, `"signals"`). A section that succeeds with a real "nothing to
show" result (e.g. no active alarms) is a populated **empty list**, never
`null` — `null` means "this section's query failed," nothing else.

**Proven live, not just asserted from reading the code.** Feasibility note:
the four contexts shared one connection-string variable in `Program.cs`
before this task, so breaking one context's connection alone wasn't
possible without also breaking the others. Fixed with a small, permanent,
minimal addition — a fallback-pattern connection-string override for
RadiationMonitoring only:

```csharp
var radiationMonitoringConnectionString = builder.Configuration.GetConnectionString("RadiationMonitoringDb")
    ?? alarmManagementConnectionString;
```

With no `RadiationMonitoringDb` key configured (the normal, shipped state),
this is byte-for-byte the same connection string every other context uses —
zero behavior change. For the test, a temporary `RadiationMonitoringDb`
entry pointing at a nonexistent database name was added to `appsettings.json`,
the host restarted, and the composed endpoint called:

```json
{
  "unitId": 1,
  "unit": { "id": 1, "code": "UNIT-1", "name": "Demonstrator Unit 1", "latestPowerPercent": 95.000000, ... },
  "activeAlarms": [],
  "radiation": null,
  "signals": [ { "tag": "UNIT1-NI-001", ... }, { "tag": "UNIT1-NI-002", ... } ],
  "errors": { "radiation": "An exception has been raised that is likely due to a transient failure. Consider enabling transient error resiliency by adding 'EnableRetryOnFailure' to the 'UseSqlServer' call." }
}
```

`HTTP 200` — the whole endpoint did **not** fail. `unit`, `activeAlarms`,
`signals` all populated correctly (three of four contexts genuinely
succeeded, proving the failure was isolated, not a symptom of a broader
issue). `radiation: null` with the real EF Core exception message recorded
under `errors.radiation`. Exactly the designed behavior, demonstrated
against a genuine connection failure, not a hand-thrown test exception.

The temporary bad connection string was then removed from `appsettings.json`
(reverting to the shipped, override-free state); the `radiationMonitoringConnectionString`
fallback pattern in `Program.cs` was kept, since the task asked for this
capability to be added, not just borrowed for one test.

## Design question 3: unit not found — decided and proven

**Decision**: if ReactorFleet's own query reports `IsFailure` (a real "no
such unit" result, not an exception), the whole endpoint returns `404` —
there is genuinely nothing to overview. If ReactorFleet's call instead
*throws*, that is treated as an ordinary partial failure (the `unit`
section comes back `null` with an `errors.unit` entry, same as any other
section) and the endpoint still returns `200` with whatever else succeeded
— a broken query is not proof the unit doesn't exist, and shouldn't be
treated as if it were.

**Proven live**: `GET /api/v1/overview/units/999999` → `HTTP 404`,
`{"error":"Unit 999999 does not exist."}`.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as all seven prior slices — zero regressions, including
after the permanent `radiationMonitoringConnectionString` fallback addition
and the temporary instrumentation/revert cycles.

## Real host, real database — live evidence

Memory discipline across this multi-restart task: checked before every
host start and every deliberate test. First session ended paused at a
declining trend (2.30 → 2.02 GB across five checks) before the two
proof-of-behavior tests; resumed only once memory was fresh and confirmed
stable (2.36 → 2.38 GB). Each of the three restarts in this resumed session
(concurrency-proof run, partial-failure run, final clean confirmation run)
was preceded by a stable-or-rising memory check (2.25→2.33, 2.36→2.39,
2.33→2.23) before proceeding, and by an in-flight check immediately before
each risky call.

Reused existing seeded data from prior slices rather than reseeding, per
the task's instruction — composed against **`UnitId 1` (`UNIT-1`)**, which
already had real state across three of the four contexts (ReactorFleet
power history, RadiationMonitoring monitors/zone, Instrumentation signals)
from earlier slices; `activeAlarms` for unit 1 is a genuine empty list
(unit 1 was never one of AlarmManagement's dev-run-residue units), which is
itself useful evidence that "no data" renders as `[]`, not `null` or an
error.

### Full composed response (clean, final run)

```json
{
  "unitId": 1,
  "unit": {
    "id": 1, "code": "UNIT-1", "name": "Demonstrator Unit 1",
    "latestPowerPercent": 95.000000, "latestPowerRecordedAtUtc": "2026-08-22T11:00:00",
    "recentPowerSnapshots": [ ... 3 entries ... ]
  },
  "activeAlarms": [],
  "radiation": {
    "unitId": 1,
    "monitors": [
      {"monitorCode":"RM-UNIT-1", ..., "latestValue":0.180000, ...},
      {"monitorCode":"RM-UNIT-1-B", ..., "latestValue":null, ...}
    ],
    "zones": [{"code":"ZONE-UNIT-1", "classification":"LOW", "status":"POSTED"}]
  },
  "signals": [
    {"tag":"UNIT1-NI-001", "latestValue":99.2, ...},
    {"tag":"UNIT1-NI-002", "latestValue":null, ...}
  ],
  "errors": {}
}
```

`HTTP 200`. `GET /health/ready` → `Healthy`, `HTTP 200`.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

Four sessions, all `nexus1_app`, no fallback — confirmed on the final clean
run.

`sys.databases` confirmed all `ONLINE` after every one of the three
restarts in this resumed session; no corruption at any point across the
whole slice.

## Summary

Eight vertical slices now exist in `Nexus1.Bff`. This is the first
cross-context one, and all three of its design questions were resolved with
reasoning stated up front and then proven against the real running host,
not just asserted from reading the code:

1. **Concurrency**: genuinely parallel, proven by an unambiguous
   max-vs-sum timing comparison (795ms observed vs ~2064ms if sequential).
2. **Partial failure**: partial success with explicit per-section marking,
   proven against a real broken connection (not a hand-thrown exception),
   isolated via a small permanent connection-string override addition.
3. **Unit not found**: a real 404 only for confirmed non-existence, never
   for a section that merely threw — proven live for both the not-found
   and the exists-but-mostly-empty cases.

No context's Application layer was touched to build this — the entire
slice is new composition code in `Nexus1.Bff` reusing four already-proven
handlers exactly as built for their own slices.
