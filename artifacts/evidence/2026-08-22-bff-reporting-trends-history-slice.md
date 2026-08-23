# Evidence: BFF fifth vertical slice — Reporting, Trends & History (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with a fifth vertical slice, composed alongside
ReactorFleet, AlarmManagement, DigitalTwin, and RadiationMonitoring in the
same host:

- `GET /api/v1/reporting/units/{id}` — per-unit root-cause case history for
  a "Trends & History" screen.

No new ADR — recorded inline, same as the prior three slices. This slice
surfaced the two largest findings of any slice so far.

## 1. What Reporting's Application layer already exposed

**There was no `Nexus1.Reporting.Application` project at all.** Unlike
every other context (which at minimum had *some* Application layer, even if
zero queries, like ReactorFleet), Reporting had none — its entire prior
existence was write-side only: a message consumer
(`ReportingConsumerBackgroundService`/`ReportingProjectionMessageHandler`)
that projects two RootCause events (`RootCauseCaseOpenedV1`,
`RootCauseVerdictIssuedV1`) into `RootCauseCaseSummary` rows, plus a retry
dispatcher for failed publishes (ADR-012). Nothing in the codebase read
`RootCauseCaseSummary` back out through any Application-layer mechanism —
`Nexus1.Reporting.Infrastructure`'s `ServiceCollectionExtensions` registered
no repository, no finder, nothing queryable at all before this task.

Reporting is also **Phase 1**, not Phase 2 — its own physical database
(`ReportingDb`, ADR-012), separate from the `AlarmManagementDb` the last
three slices all shared. This meant the BFF needed a new connection string,
not a reused one.

**Added, following the same convention every other context already uses**:
a brand-new `Nexus1.Reporting.Application` project (`IQuery`/`IQueryHandler`,
`ICaseSummaryFinder`, `CaseSummaryDto`, `GetCaseSummariesForUnitQuery`/Handler),
plus `EfCaseSummaryFinder` in `Nexus1.Reporting.Infrastructure`. This is a
bigger addition than any prior slice's "add one sibling query" pattern — an
entire missing layer, not a missing method — but the same established
IQuery/IQueryHandler/Finder shape applies unchanged; nothing new was
invented to accommodate it. `DependencyLawTests` (7/7) confirms the new
project classifies correctly as an `Application` layer for context
`Reporting` with zero changes to the architecture test itself.

## 2. Hosted-service check — the biggest finding

**Reporting has two unconditional hosted services, both messaging-dependent,
confirmed by reading each constructor directly:**

- `ReportingConsumerBackgroundService` — needs `RabbitMqConnectionManager`
  and `RabbitMqOptions` (from `AddNexusMessaging`).
- `ReportingProjectionMessageHandler` (a singleton the consumer resolves)
  — needs `NexusRuntimeMetrics` (from `AddNexusObservability`).
- `RetryDispatcher` (resolved per-scope by `RetryDispatcherBackgroundService`)
  — needs `IBrokerPublisher` (from `AddNexusMessaging`).

Three separate unresolvable dependencies, all confirmed by direct code
reading before touching `Program.cs` — this is exactly the AlarmManagement
outbox-relay class of startup crash, not the DigitalTwin/RadiationMonitoring
"none exists" precedent holding a third time. The task's own framing
anticipated the Phase 2 precedent might not generalize to a Phase 1 context,
and it didn't.

**Fix**: the same opt-out pattern already used for AlarmManagement — added
`bool enableMessagingConsumer = true` to `AddReportingInfrastructure`,
gating all three registrations (`ReportingProjectionMessageHandler`,
`ReportingConsumerBackgroundService`, `RetryDispatcher`,
`RetryDispatcherBackgroundService`). Default `true` preserves
`Nexus1.ModularRuntime`'s exact existing behavior. `Nexus1.Bff` passes
`enableMessagingConsumer: false`. `ICaseSummaryFinder`/`EfCaseSummaryFinder`
stay registered unconditionally — reading has no messaging/observability
dependency, only consuming and retry-dispatching do.

The BFF host started cleanly on the very first run after this fix — no
separate reproduce-then-fix cycle was needed this time, since all three
dependencies were confirmed unresolved by direct inspection before the fix
was written (matching, not repeating, the AlarmManagement discipline).

## 3. The endpoint, and the named gap (the second big finding)

`GET /api/v1/reporting/units/{id}` returns real case-history rows: case id,
unit id, the alarm flood that triggered the investigation, status
(`Open`/`VerdictIssued`), verdict text (once issued), and both timestamps.

**Named gap, the central finding of this slice**: Reporting's real domain
model is **root-cause investigation case history**, not a generic sensor
time-series. There is no "trend" concept anywhere in this context — no
readings, no measurements, no time-series of any physical quantity. The task
named the screen "Trends & History"; what Reporting actually has is the
"History" half only — a record of which investigations were opened for a
unit, when, and how they were resolved. A literal reading of "historical
trend data" doesn't map onto anything this context contains. The endpoint
is shaped honestly around what Reporting genuinely is (case history), not
around what the screen name might imply it should be.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as all four prior slices — zero regressions from the new
project, the new finder, or the `enableMessagingConsumer` parameter.

## Real host, real database — live evidence

Memory checked before starting the host (2.46 GB, confirmed stable across
two checks). Rechecked after host start (2.33 GB) and before the endpoint
call (2.29 GB) — stable throughout, no incident this run.

`Reporting.RootCauseCaseSummary` had **942 existing rows** — real dev-run
residue from earlier campaigns, no seeding needed. `UnitId 9101` had 11 real
cases, used for evidence.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/reporting/units/9101`

```json
[
  {"caseId":639224729512739008,"unitId":9101,"alarmFloodId":639224729494415914,"status":"VerdictIssued","verdict":"Loose fitting confirmed as cause.","openedAtUtc":"2026-08-16T10:29:11.2745244","verdictIssuedAtUtc":"2026-08-16T10:29:13.1553512"},
  ... (11 rows total, most-recent-first)
]
```

HTTP 200 — mix of `Open` (verdict/verdictIssuedAtUtc null) and `VerdictIssued`
(both populated) rows present, confirming the DTO's nullable fields render
correctly for both states.

### `GET /api/v1/reporting/units/999999` (unit with no cases)

```json
[]
```

HTTP 200.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

Run against `ReportingDb` specifically (its own physical database, not
shared with the other four contexts, so this needed independent
verification, not an inherited assumption from the earlier checks):

```
login_name  program_name                          status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

Confirmed — `nexus1_app`, no fallback, on `ReportingDb`.

Host stopped cleanly; `sys.databases` confirmed all `ONLINE` afterward.

## Summary

Five vertical slices now exist in `Nexus1.Bff`. Reporting is the first
Phase 1 context added, and the first to require both a brand-new Application
project and an AlarmManagement-style messaging opt-out — the two largest
structural additions of any slice so far, both confirmed by direct
inspection before writing any fix, and both landing clean on the first
attempt.

Robotics is next, evidence to follow separately per the task's instruction
not to batch the two reports.

## Closing note (added 2026-08-23, after the seventeenth and final slice): this endpoint is also "the RootCause slice"

Before designing a would-be eighteenth vertical slice for RootCause, its
actual reachability was investigated rather than assumed. Findings,
recorded here because they resolve directly against this slice's own
endpoint:

- **`Nexus1.RootCause.Host` has no query-capable HTTP surface at all.**
  Its `Program.cs` was read in full: the only `app.Map*` calls are
  `/health/live` and `/health/ready`. `AddRootCauseApplication`/
  `AddRootCauseInfrastructure`/`AddNexusMessaging` are registered, but
  nothing exposes RootCause's Application layer over HTTP — it exists
  purely to back RootCause's own message publishing/consumption.
  `Nexus1.RootCause.Host` was confirmed **untouched by any BFF work**,
  consistent with ADR-001 (RootCause stays out-of-process).
- **The only real path to RootCause data is via its published events.**
  Searched the whole solution for every consumer of
  `RootCauseCaseOpenedV1`/`RootCauseVerdictIssuedV1`: exactly three exist
  (Audit, Compliance, Reporting). Audit's and Compliance's projections are
  the narrow ledgers already covered by their own slices (12 and 14).
  Reporting's `ReportingProjectionMessageHandler` is the one that builds an
  actual case-investigation-history shape (`RootCauseCaseSummary`), and
  `GetCaseSummariesForUnitQuery` — this slice's own endpoint — is already
  that path, shipped since slice 5.

**Conclusion: no new RootCause BFF slice was built.** There was nothing on
the other end of a direct HTTP integration to call, and no gap to close —
`GET /api/v1/reporting/units/{id}` (this slice's endpoint) already is the
real, practical RootCause data path available to the BFF today, under
Reporting's name rather than RootCause's own.

This closes out the vertical-slice effort: all seventeen Schema Atlas
sectors, plus the Overview aggregation, are now live in `Nexus1.Bff`.
