# Evidence: BFF second vertical slice — AlarmManagement, read + write (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` (still one host, ADR-030) with a second vertical slice,
composed alongside the existing ReactorFleet slice, not a separate host:

- `GET /api/v1/alarm-management/alarms/active` — fleet-wide active/unacknowledged
  alarms (new: `GetActiveAlarmsQuery`/handler, `IAlarmEventFinder.GetAllActiveAsync`,
  `ActiveAlarmSummaryDto` — the existing `GetActiveAlarmsForUnitQuery` is scoped
  to one unit and doesn't serve a fleet-wide monitoring screen, so a sibling
  query was added rather than reusing it, following the same "check what
  exists, add the minimal necessary complement" approach used for ReactorFleet).
- `POST /api/v1/alarm-management/alarms/{id}/acknowledge` — routes directly to
  the existing `AcknowledgeAlarmCommand`/`AcknowledgeAlarmCommandHandler`
  (Phase 1), unchanged.

No new ADR — per the task's own framing, this is recorded inline here. One
real architectural finding did surface (below) and is documented in full,
same standard as an ADR would require, just without a separate numbered file.

## The main finding: composing AlarmManagement.Infrastructure in the BFF would have crashed it

`AddAlarmManagementInfrastructure` unconditionally registered two hosted
background services — `OutboxPublisherBackgroundService` (needs
`IBrokerPublisher`, from `AddNexusMessaging`) and
`OutboxMetricRefreshBackgroundService` (needs `OutboxMetricState`, from
`AddNexusObservability`). The BFF registers neither, per its MVP scope. This
was **reproduced directly**, not just reasoned about:

```
Unhandled exception. System.AggregateException: Some services are not able to be constructed
 ---> Unable to resolve service for type 'Nexus1.BuildingBlocks.Messaging.IBrokerPublisher'
      while attempting to activate 'Nexus1.AlarmManagement.Infrastructure.Messaging.OutboxRelay'
 ---> Unable to resolve service for type 'Nexus1.BuildingBlocks.Observability.OutboxMetricState'
      while attempting to activate 'Nexus1.AlarmManagement.Infrastructure.Messaging.OutboxMetricRefreshBackgroundService'
```

**Fix**: added `bool enableOutboxRelay = true` to
`AddAlarmManagementInfrastructure` (`src/Contexts/AlarmManagement/Nexus1.AlarmManagement.Infrastructure/ServiceCollectionExtensions.cs`),
gating both hosted-service registrations. Default `true` preserves
`Nexus1.ModularRuntime`'s exact existing behavior (verified: full regression
suite still green). `Nexus1.Bff` passes `enableOutboxRelay: false` — it is a
read/write API surface for this slice, not a second outbox-relay/metrics
process for AlarmManagement's messaging backbone; `Nexus1.ModularRuntime`
already owns that job, and running a second relay loop against the same
outbox table from a second process would be a duplicate-relay hazard even if
the DI crash didn't exist. `IOutboxWriter`/`EfOutboxWriter` stay registered
either way — enqueuing a row has no broker/observability dependency, only
relaying one does.

After the fix, the BFF host starts cleanly with zero DI errors (confirmed
directly this session, both on the initial post-fix run and the resumed run
after the memory-instability pause below).

## The messaging/outbox question the task asked to settle

**Answer: `AcknowledgeAlarmCommand` has zero outbox/messaging side effects,
regardless of which host invokes it — this is not a BFF-specific limitation.**

Confirmed two ways:

1. **By reading the code**: `AcknowledgeAlarmCommandHandler` never injects or
   calls `IOutboxWriter`. Only `DetectFloodCommandHandler` does (for
   `AlarmFloodDetectedV1`, the event RootCause's fan-out actually consumes).
   `AlarmEvent.Acknowledge()` does raise an in-memory `AlarmAcknowledged`
   domain event, but nothing in this codebase dispatches `Entity.DomainEvents`
   to anything — no dispatcher exists anywhere in the solution (`grep`-confirmed);
   the event is recorded on the aggregate and never read. This was true before
   this task and is unrelated to which host runs the command.
2. **Live, against the real database** (see below): outbox row count
   identical before and after the acknowledge call.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Exactly the same 869 baseline as the ReactorFleet slice and the last Phase 2
checkpoint — zero regressions from the new query/finder method, the
`enableOutboxRelay` parameter (default preserves existing behavior), or the
BFF's new AlarmManagement composition.

## Real host, real database — live evidence

Two attempts were needed due to environment memory instability (see below);
the second completed the sequence, using data already captured in the first
where nothing had changed.

### `GET /api/v1/alarm-management/alarms/active`

Returned all 96 currently-active alarms (real dev-run residue data from
earlier Phase 1 campaigns), each with `unitId` — confirming the fleet-wide
shape works and crosses units correctly (`9001`, `9002`, `9004`, `9005`,
`9101`, `9201`, `9301`, `9302`, `9501` all represented). `alarmEventId 90001`
present, `unitId 9001`, `severity "Critical"`.

### Outbox count before the write

```sql
SELECT COUNT(*) FROM messaging.OutboxMessage;
-- 1267
```

### `POST /api/v1/alarm-management/alarms/90001/acknowledge`

```json
{"acknowledgedByUserId":"11111111-1111-1111-1111-111111111111"}
```

→ `HTTP 200`

### State verified directly in the database (not inferred from the 200)

```sql
SELECT AlarmEventId, State, AcknowledgedByUserId, AcknowledgedAtUtc
FROM AlarmManagement.AlarmEvent WHERE AlarmEventId = 90001;
```

```
AlarmEventId  State         AcknowledgedByUserId                  AcknowledgedAtUtc
90001         Acknowledged  11111111-1111-1111-1111-111111111111  2026-08-22 18:44:30.1251819
```

Real state change, not just a 200 response.

### Outbox count after the write

```sql
SELECT COUNT(*) FROM messaging.OutboxMessage;
-- 1267
```

**Unchanged (1267 → 1267)** — settles the messaging question with live
evidence matching the code-reading answer above.

## Known gap: re-list-via-endpoint not run

The task's sequence ends with re-listing `GET .../alarms/active` through the
BFF endpoint itself to show `90001` no longer appears. **This step was
skipped, by agreement, not completed.** The direct database check above
already proves `State = Acknowledged`, and `GetAllActiveAsync`'s
`WHERE e.State == AlarmState.Active` filter trivially excludes a non-Active
row — so the outcome isn't in doubt, but it was not demonstrated through the
HTTP endpoint itself. Recorded here explicitly as a real, trivial-to-complete
gap rather than folded silently into "all steps verified" — a two-line curl
call whenever the environment is stable enough to be worth spending on.

Similarly, the scoped `nexus1_app` login was not independently re-verified
via `sys.dm_exec_sessions` in this run specifically — the BFF's AlarmManagement
composition uses the exact same `alarmManagementConnectionString` (same
`appsettings.json` entry) already confirmed as `nexus1_app` in the ReactorFleet
slice's evidence, so it is the same login by construction, not a separate,
unverified claim — but it wasn't re-checked live here given the memory
constraints below.

## Environment issue: repeated memory instability (recorded, out of scope to fix)

Free physical memory on the dev machine fluctuated sharply and repeatedly
during this task, independent of and in addition to the earlier-established
~1.4 GB LocalDB-corruption precondition:

- First attempt: 3.28 GB (build/test) → 2.13 GB → **1.20 GB** during health-check
  calls, well below the known danger line; stopped immediately, no corruption
  (`sys.databases` confirmed all `ONLINE`).
- After a user-side memory-freeing pass: 1.7 GB → 1.5 GB → 1.47 GB → **1.45 GB**
  over four checks spanning the list-alarms call; stopped before the write.
- After a second free-up: 2.13 GB (stable across two checks) → dropped to
  **1.27 GB** immediately after the acknowledge write completed — a sharp,
  fast decline with no sustained recovery observed at any point in this task.

Every stop point was verified against `sys.databases` (`state_desc`) directly
afterward — all databases remained `ONLINE` throughout; no corruption
recurred this session. This pattern (memory dropping by 0.5–1 GB across a
span of a few seconds/minutes, without recovering) is worth having on record
as a real, ongoing environment condition on this machine, separate from any
one incident — but fixing it is outside this task's scope.

## Summary

Two vertical slices now exist in `Nexus1.Bff`:

- **ReactorFleet** — read-only (fleet overview + unit detail).
- **AlarmManagement** — read + write (fleet-wide active alarms + acknowledge),
  with the messaging/outbox question settled: acknowledging has no outbox
  side effect, proven both by code and by live evidence, and the pattern
  generalizes to a write path without requiring the BFF to run any part of
  AlarmManagement's messaging backbone.

Stopping here, as agreed — no further slices until the next one is decided.
