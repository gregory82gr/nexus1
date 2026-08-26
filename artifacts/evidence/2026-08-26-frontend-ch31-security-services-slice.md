# Evidence: Angular console, Ch. 31 — Security & Services

## Scope

One screen, `sec` route, one new backend endpoint exposing an
already-real mechanism as structured data:

1. `Nexus1.Bff/Program.cs` — added `GET /health/contexts`, a real
   per-context breakdown of the already-registered
   `DbContextHealthCheck<T>` checks (previously only reachable as
   `/health/ready`'s ASP.NET Core default aggregate plain-text status).
2. `SecurityComponent` (`features/security/`) — Context Health (real,
   live), OT Security Posture (static documentation), Network Zones
   (static documentation).

Read the full chapter (pp. 308–322 of `From_File_to_Framework_Final.pdf`,
extracted via `pdftotext -f 308 -l 322 -layout`) before building.

## Investigation, reviewed before writing final code

The book's finding is different in kind from every prior chapter: not a
wrong computation, no computation at all — 12 status lights (6
"Microservice Health" services, 6 "OT Security Posture" controls under a
"Zero-Trust" tag) are literal `<span class="led ok">` markup with no
`renderSecurity()` anywhere. The book's own honest fix keeps 2
Microservice Health rows genuinely live (threshold-driven from real
`telRate`/`twinLag` signals) and marks the other 4 "not monitored"; OT
Security Posture drops every LED and the Zero-Trust tag, becoming plain
documentation in Network Zones' own already-honest format.

Checked directly before assuming the book's own "2 genuinely live rows"
premise carries over — **it does not, on both signals**:

- **Telemetry ingestion rate**: total absence. Real outbox metrics exist
  (`OutboxMetricState`: pending count, oldest-message age) but are
  counts/ages, never a rate. Critically, even this isn't reachable from
  the BFF at all — `Program.cs` states explicitly the BFF registers
  neither `AddNexusMessaging` nor `AddNexusObservability`.
- **Digital-twin sync lag**: total absence. Confirmed the exact
  "four-hop join" gap (`IActiveTwinFinder.cs`'s own doc comment,
  matching an earlier cluster's finding precisely) — no timing-delta or
  staleness field exists anywhere in `DigitalTwin.Domain`.
- **OT Security Posture's 6 controls**: confirmed absent, all 6, by a
  solution-wide grep for HSM/MFA/IDS/segmentation/zero-trust/
  audit-streaming — zero real matches anywhere.

So neither of the book's own "live" rows has real data here either — a
bigger gap than the book's own premise assumes, reported to the user
before building.

**The one genuinely real thing found**: `DbContextHealthCheck<T>`,
already registered for 16 real composed contexts, already backing
`/health/ready` — but only as ASP.NET Core's default aggregate
plain-text status (`Healthy`/`Unhealthy`), never a per-context
breakdown.

**Decision, reviewed and approved**: not total absence (unlike Ch.26/28)
— build the real per-context breakdown as a genuinely different kind of
"health" than the book's own concept, labeled precisely so it can never
be mistaken for message-processing/business-logic health: **database
connectivity reachability, nothing more.**

## Backend: the new endpoint

```csharp
app.MapGet("/health/contexts", async ([FromServices] HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
{
    var report = await healthCheckService.CheckHealthAsync(cancellationToken);
    var results = report.Entries
        .Select(entry => new ContextHealthResult(ContextNameForCheckName(entry.Key), entry.Value.Status.ToString(), entry.Value.Duration.TotalMilliseconds))
        .OrderBy(r => r.ContextName, StringComparer.Ordinal)
        .ToList();
    return Results.Ok(results);
});
```

`ContextNameForCheckName` maps each check's real registration key
(`"reactorfleet-db"`) to its real context name (`"ReactorFleet"`) —
mirroring the exact 16 `AddCheck<DbContextHealthCheck<T>>("x-db")` calls
already in `Program.cs` one-for-one, never an independent guess. The
endpoint naturally reflects whichever contexts are actually composed in
the running instance (`report.Entries` only ever contains what
`IsContextEnabled(...)` actually registered) — no hardcoded active list.

Zero new domain/application logic — every value returned (`Status`,
`Duration`) is already computed by the existing, unmodified
`DbContextHealthCheck<T>.CheckHealthAsync` (`CanConnectAsync` +
pending-migration check); this endpoint only re-shapes an
already-real result as JSON instead of discarding it into a plain-text
aggregate.

One build issue hit and fixed: the new local function and record
initially landed after an existing top-level-statement record
(`AcknowledgeAlarmRequest`), which C# rejects (`CS8803`: local functions/
statements must precede type declarations in a top-level-statement
file). Reordered so `ContextNameForCheckName` precedes both records.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `core/api/security-api.ts` — mirrors `ContextHealthResult` exactly.
  Calls `/health/contexts` directly (not under `/api/v1/...` — a health
  endpoint alongside the existing `/health/live`/`/health/ready`).
- `features/security/context-status.ts` — `statusTone()`, an exact match
  (not a keyword guess) against the three real `HealthStatus` values
  (`Healthy`/`Degraded`/`Unhealthy`), since — unlike free-text severities
  elsewhere in this project — this is a real, closed enum.
- `features/security/security.ts/.html/.scss` — three panels:
  - **Context Health**: real per-context rows, each with a real `.led`
    (deliberately not `.pill`, mirroring the book's own vocabulary for
    this exact screen since it happens to align with genuinely real
    behavior here) driven by `toneOf(r.status)`. An explicit badge reads
    "DB CONNECTIVITY CHECK — NOT SERVICE-LEVEL MONITORING."
  - **OT Security Posture**: the 6 named controls as a plain fact list,
    no LEDs, no "Zero-Trust" tag — Network Zones' own honest static
    format, per the book's own final state.
  - **Network Zones**: static architecture description (L0/L1 Safety →
    L2 Control → L3 Supervisory → L4 Enterprise), no backend call at
    all.
- `app.routes.ts` — the single `sec` route now points at
  `SecurityComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest security context-status → 8/8 passing (new specs alone)
npx jest (full suite)            → 233/233 passing (was 225)
```

- `context-status.spec.ts`: exact mapping for all three real
  `HealthStatus` values; an unrecognized string never gets guessed a
  tone.
- `security.spec.ts`: fetches the real endpoint; **a genuinely different
  per-context result renders a genuinely different LED class** (`ok` vs
  `crit` for two different real statuses in the same response — proving
  the LED is driven by data, not hardcoded); real context names and real
  per-check durations render, not placeholders; the panel explicitly
  labels itself DB-connectivity-only; **OT Security Posture and Network
  Zones render zero `.led` elements and no "Zero-Trust" badge anywhere**
  (checked as an actual absent element, not a text-match — since the
  panel's own honest prose legitimately quotes "Zero-Trust" once to
  explain why it's gone, a bare text-match test would have
  self-contradicted, same lesson as an earlier cluster); a real error
  state on an unreachable endpoint.

Production build:
```
npx ng build → 0 errors, 0 warnings. security compiles to its own lazy
               chunk (~2.18 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially. The .NET gate was run once immediately after the backend
change (confirmed clean, unchanged) and not re-run after the
purely-frontend work that followed.

## Live evidence — the genuine break-one-context proof

`Nexus1.Bff` started subset-composed to `BffContexts__Enabled__0=Audit`,
`__1=Compliance`, with Compliance's connection string **deliberately
overridden** via an environment variable to a nonexistent database:

```
ConnectionStrings__ComplianceDb="Server=(localdb)\mssqllocaldb;Database=Nonexistent_Ch31_Proof_Db;..."
```

No file edit and no revert needed (unlike the Overview slice's
`appsettings.json` edit/revert cycle) — an environment variable is
process-scoped and simply ceases to exist once this one host process
exits; the real `ComplianceDb` was never touched (confirmed by
`sys.databases` showing it `ONLINE`, unaffected, after the run).

```
GET /health/ready    → Unhealthy   (the real ASP.NET Core aggregate — correctly reflects the one broken context)
GET /health/contexts → [{"contextName":"Audit","status":"Healthy","durationMs":24.6},
                         {"contextName":"Compliance","status":"Unhealthy","durationMs":18.1}]
```

Two different real per-context statuses in the same live response — not
a hypothetical, not a mocked fixture. `/sec` rendered live
(`get_page_text`, real request confirmed `200 OK` via the network log):
Audit's row shows a green `Healthy` LED, Compliance's row shows a red
`Unhealthy` LED, both with real per-check millisecond durations. OT
Security Posture and Network Zones rendered as pure documentation, no
LEDs anywhere in either panel.

### Screenshot

- `security.png` — `/sec`, full-width shell, sidebar correctly
  highlighting "Security / Services" active, Context Health panel
  showing one real green and one real red LED side by side (the genuine
  break-proof, live, not staged after the fact), OT Security Posture and
  Network Zones as clean static fact lists.

Reviewed directly before reporting done.

Session/database verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x1)
```

One real session (Audit's genuine connection; Compliance's connection
attempt never succeeded, as intended). `sys.databases` confirmed all 9
real databases `ONLINE` afterward, including the untouched `ComplianceDb`.

## Summary

Read the full chapter before building. Investigated both of the book's
own "genuinely live" signals (telemetry rate, twin sync lag) and found
neither has real data anywhere in this backend — a bigger gap than the
book's own premise. Rather than declare total absence, built a
genuinely different real thing already sitting unexposed in this
backend: per-context `DbContextHealthCheck` reachability, labeled
precisely as database connectivity, never service-level monitoring, so
the vocabulary this console has earned since Ch.6 (a green LED means
"checked, right now, against something real") is never spent on a claim
this backend can't back. Proved it live by genuinely breaking one
composed context's connection and watching the new endpoint —
correctly, honestly — report it as `Unhealthy` while its sibling stayed
`Healthy`, the exact property a real health check is supposed to have
and the book's own hollow LEDs never could.
