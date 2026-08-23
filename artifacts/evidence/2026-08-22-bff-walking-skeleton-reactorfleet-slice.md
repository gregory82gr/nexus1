# Evidence: BFF walking skeleton — ReactorFleet vertical slice (ADR-030)

## Scope

New host `Nexus1.Bff` (`src/Hosts/Nexus1.Bff`), composing `Nexus1.ReactorFleet.Application`/
`.Infrastructure` in-process, exposing:

- `GET /api/v1/reactor-fleet/units` — fleet-overview screen
- `GET /api/v1/reactor-fleet/units/{id}` — unit-detail screen
- `GET /health/live`, `GET /health/ready` (`DbContextHealthCheck<ReactorFleetDbContext>`)

Required additions surfaced during implementation (see ADR-030 for full
rationale): `Nexus1.ReactorFleet.Application` gained its first queries
(`GetUnitsQuery`/`GetUnitByIdQuery` + handlers, `IUnitFleetFinder`);
`Nexus1.ReactorFleet.Infrastructure` gained its first Finder implementation
(`EfUnitFleetFinder`). `Nexus1.ArchitectureTests.DependencyLawTests.Classify`
updated to recognize `Nexus1.Bff` as a `Host`.

## Build

```
dotnet build Nexus1.Runtime.sln
Build succeeded. 0 Warning(s). 0 Error(s).
```

(One unrelated stale-restore issue hit three pre-existing test projects —
`Nexus1.RadiationMonitoring.ComponentTests`, `Nexus1.EmergencyPreparedness.ComponentTests`,
`Nexus1.ReinforcementLearning.ComponentTests` — on the first build attempt;
`dotnet restore` on the full solution resolved it. Confirmed via `git status`
that none of the files in those three projects were touched by this change —
purely a stale `obj/project.assets.json` from before this session, unrelated
to the BFF work.)

## Full regression suite

```
dotnet test Nexus1.Runtime.sln
```

All 37 test assemblies green, **869/869 total** — exactly matching the
baseline ADR-028 recorded at the last Phase 2 checkpoint. Zero regressions
from the new ReactorFleet queries/finder, the solution file change, or the
`DependencyLawTests.Classify` update. `Nexus1.ArchitectureTests.dll`:
7/7 passed, confirming the new `Nexus1.Bff` host doesn't violate the
dependency-direction rules.

## Real host, real database

Memory checked before starting the host (3.28 GB free — well above the
~1.4 GB precondition that previously corresponded to LocalDB corruption
incidents earlier in this project).

Seeded minimal dev data directly into the real `AlarmManagementDb` (LocalDB)
for live evidence — two units, one with power history, one without (to
exercise the nullable-fields path):

```sql
INSERT INTO ReactorFleet.Unit (UnitId, Code, Name) VALUES (1, 'UNIT-1', 'Demonstrator Unit 1');
INSERT INTO ReactorFleet.Unit (UnitId, Code, Name) VALUES (2, 'UNIT-2', 'Demonstrator Unit 2');
INSERT INTO ReactorFleet.UnitPowerSnapshot (Id, UnitId, PowerPercent, RecordedAtUtc) VALUES (1, 1, 87.500000, '2026-08-22T09:00:00');
INSERT INTO ReactorFleet.UnitPowerSnapshot (Id, UnitId, PowerPercent, RecordedAtUtc) VALUES (2, 1, 91.250000, '2026-08-22T10:00:00');
INSERT INTO ReactorFleet.UnitPowerSnapshot (Id, UnitId, PowerPercent, RecordedAtUtc) VALUES (3, 1, 95.000000, '2026-08-22T11:00:00');
```

Host started: `dotnet run --project src/Hosts/Nexus1.Bff --urls http://localhost:5103`.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/reactor-fleet/units`

```json
[
  {"id":1,"code":"UNIT-1","name":"Demonstrator Unit 1","latestPowerPercent":95.000000,"latestPowerRecordedAtUtc":"2026-08-22T11:00:00"},
  {"id":2,"code":"UNIT-2","name":"Demonstrator Unit 2","latestPowerPercent":null,"latestPowerRecordedAtUtc":null}
]
```

HTTP 200 — confirms the nullable-power path for a unit with zero recorded
snapshots (UNIT-2) renders correctly rather than crashing or defaulting to 0.

### `GET /api/v1/reactor-fleet/units/1`

```json
{
  "id":1,"code":"UNIT-1","name":"Demonstrator Unit 1",
  "latestPowerPercent":95.000000,"latestPowerRecordedAtUtc":"2026-08-22T11:00:00",
  "recentPowerSnapshots":[
    {"powerPercent":95.000000,"recordedAtUtc":"2026-08-22T11:00:00"},
    {"powerPercent":91.250000,"recordedAtUtc":"2026-08-22T10:00:00"},
    {"powerPercent":87.500000,"recordedAtUtc":"2026-08-22T09:00:00"}
  ]
}
```

HTTP 200 — most-recent-first ordering confirmed.

### `GET /api/v1/reactor-fleet/units/2` (unit with no snapshots)

```json
{"id":2,"code":"UNIT-2","name":"Demonstrator Unit 2","latestPowerPercent":null,"latestPowerRecordedAtUtc":null,"recentPowerSnapshots":[]}
```

HTTP 200 — empty history renders as `[]`, not an error.

### `GET /api/v1/reactor-fleet/units/999` (nonexistent unit)

```json
{"error":"Unit 999 does not exist."}
```

HTTP 404 — the `Result<T>.Failure` path surfaces correctly as a 404, not a
500 or a silently-empty 200.

### Login verification (not just a green health check)

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                          status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

Confirms the BFF genuinely connected as the scoped `nexus1_app` login
(ADR-028), not a silent fallback to another credential.

### RootCause integration untouched

```
git status --short | grep -i rootcause
```

No output — no file under `src/Contexts/RootCause` or
`src/Hosts/Nexus1.RootCause.Host` was modified by this change, confirming
ADR-030's claim that RootCause's existing (out-of-process, ADR-001)
integration path is unaffected.

Host process stopped after evidence capture (`taskkill` on the PID bound to
port 5103).

## Known gaps recorded, not silently worked around

- **No plant or status field** on the fleet-overview/detail DTOs — `ReactorFleet.Unit`
  has no such column today (ADR-003's bare-identity Phase 1 model). See
  ADR-030's reversal condition.
- **Authentication is fully out of scope** for this walking skeleton — no
  login, no token validation, no authorization policy. See ADR-030's
  reversal condition (real login once the Angular console needs it).
- **Only ReactorFleet is composed** — every other context is added the same
  way, one vertical slice at a time, in future work; this is a walking
  skeleton, not the finished BFF.
