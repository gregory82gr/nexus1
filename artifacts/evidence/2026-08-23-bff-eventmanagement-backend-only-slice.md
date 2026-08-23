# Evidence: BFF fifteenth vertical slice — EventManagement (backend-only, no console screen)

## Scope

Extended `Nexus1.Bff` with a fifteenth vertical slice, wiring in
EventManagement's three already-existing queries:

- `GET /api/v1/event-management/events/{eventCode}` — an event by code, with
  status/severity and every linked alarm event/flood id.
- `GET /api/v1/event-management/events/{operationalEventId}/timeline` — an
  event's chronological narrative.
- `GET /api/v1/event-management/incident-actions/open` — fleet-wide open
  incident actions (not completed/verified/cancelled), ordered by due date.

Investigated first, per instruction, against the Angular companion book's
(`From_File_to_Framework`) full sitemap before writing anything — this
context is deliberately exposed **backend-only**, not fitted to a screen
shape it doesn't have.

## 1. Screen mapping — investigated, no screen found

Cross-referenced `EventManagement.Domain`'s real concepts (`OperationalEvent`,
`Incident`, `IncidentAction`, `EventTimelineEntry`, links into
`AlarmManagement.AlarmEvent`/`AlarmFlood`) against the book's full
39-screen sitemap (Appendix A) and the two chapters whose names sound
closest:

- **Ch. 23, "Alarms & Events"** (`alarms`) — read in full. Its entire
  subject is `AlarmManagement`'s own alarm feed: an `AlarmEvent` shape
  (severity/source/message/status/origin), the SCRAM-alarm request/confirm
  fix, the decorative-event-pool removal. Zero mention of `Incident`,
  `IncidentAction`, or any EventManagement concept anywhere in the chapter.
- **Ch. 29, "Root Cause"** (covers both "Incident Analysis" `incident` and
  "Root Cause Graph" `rcgraph`) — read in full. Entirely about `RootCause`'s
  fault-tree/hypothesis synthesis for EVT-2026-0418's causal ranking — a
  different domain's job. One sentence mentions an "Event Reconstruction
  timeline" on the Incident Analysis screen that "stays" un-audited, with
  **zero code shown and no dedicated nav target** — too thin to confirm as
  `EventTimelineEntry`/`GetEventTimelineQuery` in disguise, so not treated
  as a match.

**Conclusion, reported to the user before building anything:** no screen in
the sitemap is confirmed to back EventManagement. Unlike Security's "zone
access doesn't exist" finding, this is not a case of the domain being
thin or fabricated — `OperationalEvent`/`Incident`/`IncidentAction` are a
real, rich, atlas-C.8.1-designed model with genuine per-unit scoping and a
real three-step action lifecycle (`Create` → `Complete` → `Verify`, each
idempotency-guarded). The gap is on the console's side: the sitemap simply
never got a screen for this domain. Per explicit instruction, built anyway
and documented honestly as backend-only.

## 2. What EventManagement's Application layer already exposed

Fully built already — zero new Application-layer code needed. Three
queries matching atlas C.8.5.2's own three named queries verbatim
(`GetEventWithAlarmsAndFloodQuery` by `EventCode`, `GetEventTimelineQuery`
by `OperationalEventId`, `GetOpenIncidentActionsQuery` with no parameter —
fleet-wide), plus five commands (`ReportOperationalEvent`,
`LinkEventToAlarm`, `LinkEventToFlood`, `OpenIncident`,
`RecordIncidentAction`) not used by this read-only BFF slice.

## 3. Hosted-service check — confirmed directly: zero

Read `AddEventManagementInfrastructure` directly: no `AddHostedService<...>()`
calls anywhere — Phase-2-style, shares `AlarmManagementDb` (ADR-022,
joining ReactorFleet/CorePlatform/AlarmManagement/Instrumentation/
DigitalTwin/Maintenance). No opt-out parameter needed, unlike Audit/
Compliance/Reporting/AlarmManagement.

## 4. Build and full regression suite

```
dotnet build src/Hosts/Nexus1.Bff/Nexus1.Bff.csproj → 0 Warning(s), 0 Error(s)
dotnet build Nexus1.Runtime.sln                     → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln                       → 37/37 assemblies green, 869/869 total, 0 failed
```

Unchanged from the Compliance slice's baseline — no regressions.

## 5. Memory discipline

| Check | Reading | Notes |
|---|---|---|
| Before host start, 1st | 2.00 GB | |
| Before host start, 2nd (+5s) | 1.99 GB | stable |

## 6. Real host, real database — live evidence (subset composition: ReactorFleet + EventManagement)

All EventManagement tables (entities and lookups) had **zero rows** — no
dev-run residue from any prior slice touched this context. Seeded minimal
dev data: one `OperationalEvent` (`EVT-2026-0001`, Unit 1, feedwater pump
trip), one `Incident` (`INC-2026-0001`), two `IncidentAction` rows (one
`ASSIGNED` — open — one already `VERIFIED` — should be excluded), two
`EventTimelineEntry` rows, and one link each into AlarmManagement's real
existing data (`AlarmEvent` id `90001`, `AlarmFlood` id
`639223865633744300` — both genuine pre-existing rows from the
AlarmManagement slice, not fabricated ids) to exercise the fan-out LEFT
JOINs non-trivially. All lookup-table `Code` values are plain strings (no
EF enum-conversion column here, unlike CorePlatform's `QuantityType`) —
confirmed by reading each lookup's EF configuration first, so no repeat of
that earlier mismatch risk.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/event-management/events/EVT-2026-0001`

```json
{"eventCode":"EVT-2026-0001","title":"Feedwater pump trip on Unit 1","eventStatus":"OPEN","eventSeverity":"HIGH","alarmEventIds":[90001],"alarmFloodIds":[639223865633744300]}
```

HTTP 200. Both linked ids resolved correctly through the LEFT JOIN
fan-out, against real pre-existing AlarmManagement rows, not seeded ones.

### `GET /api/v1/event-management/events/1/timeline`

```json
[{"entryAtUtc":"2026-08-20T09:05:00","entryType":"NOTE","title":"Event reported","body":"Operator reported feedwater pump trip."},{"entryAtUtc":"2026-08-20T09:10:00","entryType":"STATUS_CHANGE","title":"Incident opened","body":"Incident INC-2026-0001 opened for investigation."}]
```

HTTP 200. Both entries returned in chronological order.

### `GET /api/v1/event-management/incident-actions/open`

```json
[{"incidentNumber":"INC-2026-0001","title":"Replace pump 2A bearing","actionStatus":"ASSIGNED","dueAtUtc":"2026-08-27T00:00:00"}]
```

HTTP 200. Exactly one row — the `ASSIGNED` action. The second, `VERIFIED`
action was correctly excluded, confirming the query's
`NOT IN (COMPLETED, VERIFIED, CANCELLED)` filter live, not just by reading
the query's doc comment.

### `GET /api/v1/event-management/events/NOPE-0000` (nonexistent event code)

```
HTTP 404
```

Empty body, not a fabricated 200 — matches the query's nullable DTO
(`EventWithAlarmsAndFloodDto?`), mapped to `Results.NotFound()`.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                            status
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
```

**Two sessions** — matching exactly the two composed contexts
(`ReactorFleet`, `EventManagement`), both under `nexus1_app`.

`sys.databases` confirmed all `ONLINE` after host stop; no corruption.

## Summary

Fifteen vertical slices now exist in `Nexus1.Bff`. EventManagement is the
first slice built without a confirmed console screen backing it —
investigated first against the Angular companion book's full sitemap
(both plausible name matches, "Alarms & Events" and "Incident Analysis,"
read in full and ruled out), then built anyway per explicit instruction:
real, rich, atlas-C.8.1 domain data (incidents, corrective-action
lifecycle, event timelines, cross-context alarm/flood links) exposed
honestly as backend-only, not forced into either of those two screens'
shape. All three endpoints reuse already-existing, unmodified Application
handlers with zero new Application-layer code.

EmergencyPreparedness's own investigation is reported separately, so its
build-or-skip decision can be made on its own terms.
