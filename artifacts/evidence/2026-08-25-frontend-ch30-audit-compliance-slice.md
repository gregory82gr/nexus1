# Evidence: Angular console, Ch. 30 — Audit & Compliance

## Scope

One screen, `audit` route, one new thin BFF route + one client-side
hash-chain construction over real data:

1. `Nexus1.Bff/Program.cs` — added the missing `GET
   /api/v1/compliance/analyses/{analysisId}/reviews` route (handler
   already existed, fully DI-wired, never mapped).
2. `AuditLogComponent` (`features/audit/`) — composes real Audit
   evidence + real Compliance review status per analysis id, with a
   genuine client-side SHA-256 chain over the real, previously-isolated
   per-record content hashes.

Read the full chapter (pp. 296–303 of `From_File_to_Framework_Final.pdf`,
extracted via `pdftotext -f 296 -l 312 -layout`) before building.

## Investigation, reviewed before writing final code

The book's fictional screen claims "append-only, hash-chained ... each
seal references the previous" — its own finding: the seal function is
two calls to `Math.random()`, referencing nothing. Its honest fix: a
real SHA-256 chain computed and verified client-side via the Web Crypto
API, explicitly labeled "chain verifies locally, not anchored," never
"tamper-proof" — a client-computed chain can't prove anything about the
very party (the browser operator) it's meant to catch.

Investigated this backend directly before assuming anything carried
over:

- **Corrected a working assumption**: only the Audit route was actually
  live. The Compliance route's handler (`GetComplianceReviewsBySourceAnalysisIdQueryHandler`)
  existed and was DI-wired, but no `app.MapGet` had ever been added for
  it — confirmed by reading `Program.cs` directly, not by trusting the
  prior summary.
- **Hash/seal/chain search, confirmed**: real `SHA256.HashData(...)`
  calls exist in several contexts (Audit's own
  `AuditEvidenceRecord.EnvelopeSha256`, plus dead-letter/retry
  fingerprints in several message-handler failure paths), but every one
  is an isolated per-message hash. No entity anywhere references a
  *previous* record's hash — no chain exists server-side, confirmed by
  grep across every Domain/Application/Infrastructure project.
- **Generic actor+action+timestamp log, checked across 6 candidates**:
  none is a ready-made log. AlarmManagement's acknowledgment fields and
  Security's role/permission-grant fields are real but have no read path
  at all. EventManagement's `EventTimelineEntry` has a live route, but
  its actor field is dropped at the DTO/projection level even though the
  entity and the query both already have it in scope. RootCause's
  open/close actor fields are real but RootCause has zero presence in
  the BFF host at all (ADR-001). No viable candidate existed to add an
  actor/action column from.
- **Real scoping constraint, structural not incidental**: both real
  endpoints are keyed by an opaque RootCause analysis id (unique index
  on `SourceAnalysisId` in both `AuditEvidenceRecord` and
  `ComplianceReview` — at most one row per analysis, confirmed by
  reading the EF configuration directly). No fleet-wide listing exists
  or could exist without a new architectural seam into RootCause, which
  ADR-001 deliberately avoids.

**Decision, reviewed and approved**: build a genuine client-side hash
chain over the real, already-computed `EnvelopeSha256Hex` values — not a
fabrication, since it chains real server-computed hashes — labeled
exactly like the book's own honest fix ("verifies locally, not
anchored"). Add the one missing thin route for Compliance. No
actor/action column, since none is honestly reachable.

## Backend: the one missing thin route

```csharp
app.MapGet("/api/v1/compliance/analyses/{analysisId:long}/reviews", async (long analysisId, [FromServices] GetComplianceReviewsBySourceAnalysisIdQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetComplianceReviewsBySourceAnalysisIdQuery(analysisId), cancellationToken);
    return Results.Ok(result.Value);
});
```

Zero new Application-layer code — the handler, query, and DTO all
already existed and were already DI-wired; only the route mapping was
missing.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `core/api/audit-api.ts` — mirrors `AuditEvidenceRecordDto` and
  `ComplianceReviewDto` exactly. `sourceAnalysisId` (a C# `long`) is
  deliberately never read from either response — real analysis ids
  already seen live (e.g. `639223958897100060`) exceed
  `Number.MAX_SAFE_INTEGER`, so the JSON-deserialized value can silently
  lose precision in JS. Every caller already knows the id it looked up
  (the string the operator typed), so the response's own numeric copy is
  never trusted for round-tripping — a documented hazard, not a silent
  risk.
- `features/audit/hash-chain.ts` — `computeSeal` (one real SHA-256 via
  Web Crypto, over an entry's own real content hash plus the previous
  entry's seal), `chainEntries` (builds a chain over an ordered list),
  `verifyChain` (re-derives every seal and compares to what each entry
  currently carries, reporting the first index that doesn't match).
  Every seal is a client-side construction from the start — the server
  never supplied one to re-derive, unlike the book's own fictional API.
- `features/audit/audit-log.ts` — manually-keyed analysis-id lookup
  (same UX pattern as Mission Readiness, Ch. 19, since no fleet-wide
  listing exists for either real endpoint). Each lookup fetches the real
  Compliance review (displayed per-id) and appends any real Audit
  evidence record to a growing, session-local, deduplicated list that
  the hash chain is computed and re-verified over. Two independent HTTP
  calls per lookup (not combined via `forkJoin`), matching this
  project's own established pattern of keeping independent real calls
  independent.
- `setup-jest.ts` — added a `crypto.subtle` polyfill (Node's built-in
  `webcrypto`) for the Jest/jsdom test environment, which has no Web
  Crypto implementation at all. Same "test-environment gap fix, not
  application behavior" pattern already established for the
  `ResizeObserver` stub — the served application runs in a real browser,
  which has `crypto.subtle` natively; this never executes outside Jest.
- `app.routes.ts` — the single `audit` route now points at
  `AuditLogComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest audit hash-chain → 11/11 passing (new specs alone)
npx jest (full suite)     → 225/225 passing (was 214), confirmed stable across two consecutive full-suite runs
```

- `hash-chain.spec.ts`: a seal depends on the previous seal, not just
  the entry's own content; seals are deterministic 64-hex-char SHA-256
  output, never random; `chainEntries` builds a correct chain in order;
  a genuine untampered chain verifies `ok: true`; **a tampered entry is
  detected via a broken chain, reporting exactly which index broke**
  (the required test); an early tampered entry invalidates every seal
  after it.
- `audit-log.spec.ts`: lookup fetches both real endpoints for the
  entered id; real evidence is chained and reported as verified; looking
  up the same id twice does not duplicate a chain entry; **the rendered
  page never contains "tamper-proof," only "verifies locally"** (the
  required test); a real error state on unreachable endpoints.
- One flakiness issue caught and fixed during this slice: an initial
  fixed-timer wait for the two chained async Web Crypto digests to
  settle passed in isolation but failed intermittently under full-suite
  load. Replaced with a condition-poll (checking `verification() !==
  null` directly, not guessing a duration) — confirmed stable across two
  consecutive full-suite runs afterward.

Production build:
```
npx ng build → 0 errors, 0 warnings. audit-log compiles to its own lazy
               chunk (~2.75 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially. The .NET build+test was run once immediately after adding
the backend route (confirmed clean, unchanged) and not re-run after the
purely-frontend work that followed, since nothing further touched the
backend.

## Live evidence — real host, real database, real screenshot

`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Audit`, `__1=Compliance` (memory checked
healthy at 2.98 GB beforehand); `ng serve --port 4200` alongside it.

```
GET /health/ready                                                        → Healthy, HTTP 200
GET /api/v1/compliance/analyses/639224028038165230/reviews (new route)   →
  [{"complianceReviewId":"ed12d4f4-...","sourceAnalysisId":639224028038165230,
    "verdict":"Loose fitting confirmed as cause.","state":"Pending",
    "openedAtUtc":"2026-08-15T15:00:08.8886846"}]
GET /api/v1/audit/analyses/639224028038165230/evidence                   →
  [{"auditEvidenceId":"93933e77-...","envelopeSha256Hex":"D03B04A944DB5C8B...",
    "recordedAtUtc":"2026-08-15T15:00:09.1584406", ...}]
```

`/audit` rendered live, real requests confirmed via the network log
(`200 OK` for both). Looked up two different real, known analysis ids
(`639224028038165230`, then `639223958897100060`) — real live
dev-run-residue data from earlier RootCause verdict processing, stable
across multiple prior evidence sessions (23 Audit rows, 22 Compliance
rows, unchanged from a 2026-08-23 session). The chain grew to 2 real
entries, both correctly chained (`CHAIN VERIFIES LOCALLY, NOT ANCHORED`),
and the second lookup correctly showed an honest empty compliance state
("No compliance review has been opened for this analysis") rather than
fabricating one — that analysis genuinely has no compliance review.

### Screenshot

- `audit-log.png` — `/audit`, full-width shell, sidebar correctly
  highlighting "Compliance / Audit" active, gap-declaration banner, the
  real Compliance review panel, and a real 2-entry chained Audit Log
  with the green "chain verifies locally, not anchored" badge.

Reviewed directly before reporting done.

Session/database verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x2)
```

Two sessions, matching the two composed contexts. Both processes stopped
cleanly after capture; `sys.databases` confirmed all 9 databases
`ONLINE` afterward.

## Summary

Read the full chapter before building. Investigated the exact current
route shapes (correcting an assumption that Compliance was already
live), searched the whole solution for any real actor+action+timestamp
log and any hash-chain mechanism, and confirmed neither exists anywhere
server-side. Added the one missing thin route. Built a genuine
client-side SHA-256 chain over real, previously-isolated per-record
content hashes — real cryptography over real data, assembled here for
the first time because the server never assembled it — labeled exactly
as the book's own honest fix requires, and verified with tests that
prove tamper-detection actually works and that the UI never claims more
than it can prove.
