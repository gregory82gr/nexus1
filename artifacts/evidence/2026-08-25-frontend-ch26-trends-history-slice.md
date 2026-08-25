# Evidence: Angular console, Ch. 26 — Trends & History

## Scope

One screen, `trends` route, no backend involvement at all (no BFF route,
no database, no live host needed) — two concrete corrections, no data
panel:

1. `TrendsComponent` (`features/trends/`) — corrected storage note +
   the availability figure's `NO SOURCE` declaration.

Read the full chapter (pp. 269–276 of `From_File_to_Framework_Final.pdf`,
extracted via `pdftotext -f 265 -l 280 -layout`) before building anything.

## Investigation

The chapter names two concrete, checkable things:

1. **Storage note**: the book's own screen claims "Backed by a
   time-series store (PostgreSQL + TimescaleDB)." Never true of this
   solution — every context built in this arc persists exclusively
   through EF Core over SQL Server (LocalDB in development). Trivial to
   correct, no data needed.
2. **Availability figure**: Ch. 6's original `NO SOURCE` deferral,
   computed (per the book) from two retained state transitions its own
   fictional console had already been quietly producing:
   `toggleUnit()` (online/offline) and `recordScram()` (trip, with an
   actor). The book's own method: `availability = online time / elapsed
   time` over a stated window (trailing 30 days), reported with its
   window, its transition count, and an `UNCALIBRATED` marker if the
   underlying history is simulated; a unit below a minimum transition
   count shows "insufficient history," never a fallback percentage.

Checked solution-wide before assuming either transition kind exists here
— **total absence, not a thin history**:

- **`ReactorFleet.Unit`** (`Nexus1.ReactorFleet.Domain\Unit.cs`) has
  exactly two properties: `Code`, `Name`. No status field at all,
  current or historical (confirmed: ADR-003, Phase 1 slice — the
  Schema Atlas's fuller Reactor/Equipment tables are deliberately not
  modeled). No online/offline event of any kind is ever recorded
  anywhere in this solution.
- **No scram/trip entity exists with both a timestamp and an actor.**
  Solution-wide grep for `Scram`/`Trip`: zero hits for "Scram" anywhere;
  the only "Trip" hit is `AlarmSeverity.Trip` — a severity classification
  on an automatically-raised threshold alarm (`AlarmDefinition.Evaluate`)
  with no actor field at all (only an *acknowledgment* actor/timestamp,
  recorded after the fact, not who triggered it).
- **`EventManagement`** (`Nexus1.EventManagement.Domain`) models
  incidents/investigations/timelines linked to alarms — a real, rich
  context, but about a different thing entirely; no
  SCRAM/TRIP/ONLINE/OFFLINE code was ever seeded or referenced there.
  `EventTimelineEntry` (timestamp + actor) is the closest shape in the
  whole solution, but it's a manually-typed narrative note on an
  already-opened incident, not an automatic state-transition detector.
- **No BFF route** exposes unit status history, online/offline events,
  or scram/trip events — confirmed by reading `Program.cs` in full.

**Two other real, adjacent capabilities were checked and are already
spoken for elsewhere** — reusing either here would duplicate an existing
screen, not add a new real one:
- RootCause's investigation-case history (`GET /api/v1/reporting/units/{id}`)
  — an earlier slice's own route comment literally called this "the
  Trends & History screen's" data, but it was already deliberately used
  for AI Diagnostics (Ch. 24), with explicit sign-off, before this
  cluster started.
- `ReactorFleet.Unit`'s real power-snapshot history
  (`UnitDetailDto.RecentPowerSnapshots`) is already shown as its own
  panel on the Overview screen (Ch. 6).

**Conclusion, matching the pre-agreed treatment for this exact
outcome**: build the storage-note correction (real, needs no data), and
declare the availability figure `NO SOURCE` — not "insufficient
history," which would wrongly imply a retention mechanism exists and is
merely short on data. Ours doesn't exist at all. No fabricated transition
log, no computed percentage of any kind, no substitute data panel
duplicating an existing screen.

## No BFF route, no backend code, no database

This screen makes no HTTP call at all — it is fully static, same
precedent as Training Mode (`features/training/`, "genuinely
self-contained... no BFF call anywhere"). Zero backend code touched.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `features/trends/trends.ts` — `TrendsComponent`, no injected API
  client, no constructor logic. The doc comment carries the full
  investigation (both transition kinds checked and found absent, both
  candidate substitute panels checked and found already-used elsewhere).
- `features/trends/trends.html/.scss` — three panels: the corrected
  storage note (old claim shown struck through for honest contrast, same
  "was: ..." pattern used across this console), the availability `NO
  SOURCE` declaration (explaining what the computation *would have been*
  had the mechanism existed — window, count, `UNCALIBRATED` marker — so
  the gap is precise, not vague), and an explicit note on why no other
  trend panel is shown.
- `app.routes.ts` — the single `trends` route now points at
  `TrendsComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest trends       → 4/4 passing (new specs alone)
npx jest (full suite) → 185/185 passing (was 181)
```

Mirrors the book's own test intent, adapted to what's actually true here:

- The corrected storage-note text (`.storage-note .sub`, excluding the
  struck-through `.was` quote) never *asserts* this stack is backed by
  PostgreSQL/TimescaleDB, and does name SQL Server; the `.was` element
  does quote the old claim, for contrast.
- No computed availability percentage (`\d+(\.\d)?%`) is ever rendered
  anywhere on the page.
- The availability gap's own status pill reads `NO SOURCE`; no pill
  anywhere reads "insufficient history" (that phrasing is reserved for a
  future build where the mechanism exists but a specific unit's history
  is thin — not this one).
- The "no other trend panel" declaration is present, confirming the
  RootCause/power-snapshot dead-ends are stated, not silently omitted.

Production build:
```
npx ng build → 0 errors, 0 warnings. trends compiles to its own lazy
               chunk (~1.31 KB transfer — the smallest screen built so
               far, matching its fully-static content).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially, not concurrently (the .NET gate is unaffected by this
slice but re-confirmed per standing discipline).

## Live evidence — real host, real screenshot (no database involved)

Only `ng serve --port 4200` was started — no `Nexus1.Bff`, no SQL
Server connection, since this screen makes no network call at all.

`/trends` rendered live (`get_page_text`, no console errors): the
corrected storage note, the full `NO SOURCE` availability explanation
with the hypothetical-computation description, and the "no other trend
panel" note — matching the built component exactly.

### Screenshot

- `trends.png` — `/trends`, full-width shell, sidebar correctly
  highlighting "Trends & History" active, storage note with the old
  claim struck through, availability panel with a muted `NO SOURCE` pill
  and full explanation, clean layout.

Reviewed directly before reporting done.

## Summary

Read the full chapter before building. Investigated both required real
transition kinds (unit online/offline, scram/trip-with-actor)
solution-wide and confirmed neither exists anywhere — a total absence of
the retention mechanism itself, the exact branch anticipated in advance.
Checked the two other real candidate "trend" data sources
(RootCause case-history, power-snapshot history) and confirmed both are
already honestly shown on other screens, so nothing is duplicated here
either. Built the one genuinely correctable claim (the storage note) and
declared the availability figure `NO SOURCE`, precisely — describing what
the computation would have been, never fabricating what it isn't.
