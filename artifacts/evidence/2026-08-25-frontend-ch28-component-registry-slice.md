# Evidence: Angular console, Ch. 28 — Component Registry

## Scope

One screen, `components` route, no backend involvement at all (no BFF
route, no database, no live host needed) — a NO SOURCE gap declaration,
same shape as the Trends & History (Ch. 26) precedent:

1. `ComponentRegistryComponent` (`features/component-registry/`) — a
   fully static component declaring why no wear-model data is shown.

Read the full chapter (pp. 284–291 of `From_File_to_Framework_Final.pdf`,
extracted via `pdftotext -f 280 -l 296 -layout`) before building.

## Investigation, reported and approved before writing final code

This chapter is different in kind from every prior gap chapter: the
book's own source has a **genuine** wear model — `health(u, c)` declines
with real accumulated service years, a real penalty per SCRAM cycle, and
a load-sensitivity term tied to real simulated operating history. The
book's own bug is purely a disclosure-*placement* one (the "illustrative,
accelerated model" note lives on one summary panel, never repeated on
the dozens of individual component cards). The book's own fix, using its
own three-way framework (relabel / remove / hoist), is HOIST: keep every
health bar, move the disclosure to the container level (the same
structural move Ch. 27 made for a tab bar, generalized from a fixed set
of tabs to an unbounded `@for` of cards), and type every component's
`basis: 'accelerated-model'` directly on the data.

Checked directly, on all three of the book's real inputs, before
assuming that premise carries over to this backend — **it does not, on
every one of them**:

- **Service years**: `Maintenance.Asset.CommissionedAtUtc` exists as a
  schema field, but is nullable and left unpopulated in the one asset
  this solution has ever seeded — real schema, not real data, and not
  even projected by either existing Maintenance BFF DTO
  (`UnitAssetConditionDto`, `ActiveDegradationCaseDto`).
- **SCRAM-cycle penalty**: `AlarmManagement.AlarmSeverity.Trip` exists
  as an enum value, but has zero real occurrences anywhere in this
  codebase — not one `AlarmEvent`, in any test or any live evidence
  session across this entire arc, has ever been raised with `Trip`
  severity. A count today would honestly always be zero.
- **Load-sensitivity term**: nothing in Maintenance ties to any
  runtime/load data for this purpose at all.
- **The combining formula itself**: every real Maintenance write path
  checked (`RecordAssetConditionCommandHandler`,
  `RecordDegradationCommandHandler`) is pass-through persistence — it
  validates and stores a value already supplied; neither computes
  anything from multiple inputs. `AssetCondition.HealthScorePercent` is
  human-assessed; `ActiveDegradationCaseDto.TrendPoints` is a bare
  `COUNT(*)`, not a rate or a score.
- **The book's own "11 to 12 tracked components per unit" premise is
  itself unsupported**: this solution's real seed data is exactly one
  asset per unit (a feedwater pump); `AssetComponent` (the entity that
  would represent a sub-component below it) is never populated by any
  code path in this solution, confirmed by grep across every test file.

**Conclusion, using the book's own framework**: this is not Ch. 28's own
situation (a real model, mis-disclosed) — there is no real value here to
hoist a disclosure for, and no independently-mislabeled real value to
relabel. It is closer to a total-absence gap, the same shape as Ch. 26's
availability finding: the computation mechanism itself does not exist,
not merely a thin instance of it. Reported this finding to the user with
two options (a NO SOURCE gap declaration, or a minimal single-asset view
reusing real `AssetCondition` data) before writing any code; approved:
**NO SOURCE gap declaration**, on the explicit reasoning that a
single-asset view would duplicate data already shown on Rod Inspection
(Ch. 16) and Ageing & Degradation (Ch. 18) — the same anti-duplication
principle applied throughout this arc (RootCause case-history → AI
Diagnostics, power-snapshot history → Overview, both in Ch. 26).

## No backend involvement

No new BFF route, no backend code touched. This screen makes zero HTTP
calls — same precedent as Training Mode and Trends & History.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `features/component-registry/component-registry.ts` — no injected API
  client, no constructor logic. The doc comment carries the full
  investigation (all three model inputs checked and found absent, the
  component-count premise checked and found unsupported, the two
  existing homes for the one real adjacent data named explicitly).
- `features/component-registry/component-registry.html/.scss` — four
  panels: the top-level gap declaration, a per-input breakdown ("Checked,
  and found absent" — service years, SCRAM-cycle penalty,
  load-sensitivity term, the combining formula itself, each with its own
  status pill and one-line explanation), the component-count-unsupported
  note, and an explicit "NOT DUPLICATED HERE" note naming both existing
  homes for the one real adjacent data.
- `app.routes.ts` — the single `components` route now points at
  `ComponentRegistryComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest component-registry → 5/5 passing (new specs alone)
npx jest (full suite)       → 214/214 passing (was 209)
```

- No fabricated health percentage or wear-model figure of any kind is
  ever rendered anywhere on the page (regex check across the full
  rendered text, not just a spot check).
- No `.lifebar`/`.compcard` element is ever rendered — since no real
  model exists to back one, the book's own health-bar UI shape is never
  reproduced with fabricated data.
- All three of the book's real model inputs (service years, SCRAM,
  load-sensitivity) are named explicitly in the rendered text.
- The book's 11-12-component premise is stated as unsupported, not
  silently dropped.
- The "not duplicated here" declaration, naming both Rod Inspection and
  Ageing & Degradation by name, is present.

Production build:
```
npx ng build → 0 errors, 0 warnings. component-registry compiles to its
               own lazy chunk (~1.53 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially (the .NET gate is unaffected by this slice but re-confirmed
per standing discipline).

## Live evidence — real host, real screenshot (no database involved)

Only `ng serve --port 4200` was started — no `Nexus1.Bff`, no SQL Server
connection, since this screen makes no network call at all (confirmed
via the network log: every request was a Vite/Angular dev-server asset,
zero application API calls).

`/components` rendered live (`get_page_text`, no application console
errors — the only console entries were Vite's own dev-server HMR
websocket reconnecting on a reused browser tab, unrelated to the app):
all four panels, matching the built component exactly.

### Screenshot

- `component-registry.png` — `/components`, full-width shell, sidebar
  correctly highlighting "Component Registry" active, all four gap
  panels rendered cleanly with distinct status pills per model input.

Reviewed directly before reporting done.

## Summary

Read the full chapter before building. Investigated all three of the
book's real wear-model inputs and its component-count premise, and found
every one of them unsupported in this backend — a materially different
conclusion from the book's own premise (a real model, only mis-disclosed).
Reported this finding and two treatment options to the user before
writing any code; built the approved NO SOURCE gap declaration,
explicitly naming what was checked, what was found absent, and why the
one real adjacent data is not duplicated here.
