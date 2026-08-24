# Evidence: Angular console, Ch. 6 — Plant Overview

## Scope

`OverviewComponent` (`features/overview/`), wired to the real, already-proven
`GET /api/v1/overview/units/{id}` composition endpoint (ReactorFleet +
AlarmManagement + RadiationMonitoring + Instrumentation). Full build/spec
details in the original report of this work; this file's own purpose is to
be the durable, corrected record — the initial live-evidence pass had a
real defect in its own methodology, described in full below rather than
quietly fixed and forgotten.

## A shell-level layout bug, found only after this evidence was first reported

**What happened.** The first pass of this screen's live evidence read
`get_page_text` (DOM text content) and a handful of targeted
`getComputedStyle`/token checks, confirmed every real value was present
and correctly derived, and reported the screen as verified. It was not
visually correct: a real screenshot (taken by the user, not by this
agent) showed the entire Overview screen rendered inside a narrow ~200px
column sharing space with the sidebar, with the actual wide content area
sitting empty and black.

**Root cause, confirmed by measuring actual rendered geometry
(`getBoundingClientRect()`), not by inspection alone.** `AppComponent`'s
`.app` grid (`grid-template-columns: 236px 1fr; grid-template-rows: 58px
1fr;`) never gave its three children — `<nx-sidebar />`, `<nx-topbar />`,
`<main class="content">` — any explicit grid placement. CSS Grid's default
row-major auto-placement filled the cells in document order:

| Cell | Auto-placed element | Before fix (measured) |
|---|---|---|
| row 1, col 1 | `nx-sidebar` | `x:8 y:8 w:236 h:58` |
| row 1, col 2 | `nx-topbar` | `x:244 y:8 w:533 h:58` |
| row 2, col 1 | `.content` (the router outlet) | `x:8 y:66 w:236 h:397` |
| row 2, col 2 | *(nothing — no fourth child)* | empty |

The router outlet — and therefore every screen's content — was being
placed under the sidebar in the narrow 236px column, while the actual
`1fr` area next to it sat completely empty. This is exactly the black
space and cramped column the attached screenshot showed.

**This was never Overview-specific.** It is a defect in `AppComponent`'s
own shared grid, present since Ch. 3, and would affect — was already
affecting — every routed screen, confirmed directly by re-measuring Plant
Fleet after the fix (see below). Overview simply happened to be the screen
whose rendering the user actually looked at.

## The fix

Explicit grid placement in `AppComponent`'s own styles — sidebar spans
both rows (the correct, standard shell shape this project's own Figure
mockups always implied), topbar and content share the second column:

```css
nx-sidebar { grid-column: 1; grid-row: 1 / 3; }
nx-topbar  { grid-column: 2; grid-row: 1; }
.content   { grid-column: 2; grid-row: 2; overflow: auto; padding: 14px; }
```

## Verification — real geometry, before and after

`ng serve`, `getBoundingClientRect()` read directly from the live page
(not asserted from the stylesheet):

| Element | Before | After |
|---|---|---|
| `nx-sidebar` | `w:236 h:58` (top-left only) | `w:236 h:455` (full height) |
| `.content` | `x:8 w:236` (under the sidebar) | `x:244 w:533` (the real `1fr` column) |
| `nx-overview` | `w:208` | `w:505` |
| `nx-fleet` (Plant Fleet, re-checked to confirm the shell-level claim) | *(not re-measured before the fix)* | `w:505` — same fix, same result |

Full solution regression after the fix:

```
dotnet build/ng build → 0 errors
npx jest               → 13/13 passing, unchanged
```

## Addendum — a real screenshot, captured after all

The Browser pane's own screenshot tool remained unavailable when this was
first written, so the fix above was verified by geometry measurement only.
Before accepting that as the permanent fallback, a different mechanism was
tried: Playwright (already the project's planned e2e tool, per the
companion book's own Appendix B) was installed as a real devDependency
(`@playwright/test` — landed at `1.62.1`, ahead of the book's own `1.47`
pin, same class of minor version drift already noted for TypeScript) with
its Chromium binary, and driven directly (not via the formal e2e suite,
which doesn't exist yet) with a small one-off script: launch headless
Chromium, navigate to the real `ng serve` origin with the real `Nexus1.Bff`
host running alongside it, wait for network idle, screenshot, save to
disk.

**This worked, and produced real PNGs** —
`artifacts/evidence/screenshots/overview-ch6-fixed.png` and
`fleet-ch7-fixed.png` (Plant Fleet, confirming the shell-level claim
visually too, not just by measurement). Reviewed both directly: sidebar
full-height on the left, topbar spanning the full width of the remaining
area, and the content region — stat cards, Live Signals, Section Status,
Recent Power Snapshots, Recent Alarms, Radiation/Safety — filling the
entire wide column with no cramped 236px box and no dead black space. The
Browser-pane screenshot tool's unavailability was a limitation of that one
mechanism, not of this environment overall; Playwright driven directly
is the mechanism of record going forward for any layout-sensitive
verification, ahead of the Browser pane's own preview tool when the latter
won't composite a frame.

## The methodology gap, named explicitly rather than quietly closed

**A DOM-text read (`get_page_text`) proves a value is present and
correct; it says nothing about whether that value is rendered at the
right size, in the right place, or visible at all.** That is precisely
the gap this bug fell through — every number on Ch. 6/7's screens was
real and correctly computed, and the screen was still badly broken to
look at. Reading `getComputedStyle` on a couple of tokens (as the Ch. 2/3
evidence did) narrows this gap but doesn't close it either — it checked
that `.app` itself had the right `grid-template-columns`/`rows`, but never
checked where its *children* actually landed, which is exactly where this
bug lived.

**Going forward, live frontend evidence for any screen must include an
actual rendered screenshot, reviewed for layout correctness, not just DOM
text extraction or spot-checked computed styles.** The Browser pane's own
screenshot tool remained unable to composite a frame in this session
(`the Browser pane is not displayed, so the page is not compositing
frames` — retried across viewport presets, not a one-off flake), but that
turned out to be a limitation of that one mechanism, not of the
environment — see the addendum above: Playwright, driven directly against
the real running app, produced real PNGs on the first attempt. The
standing rule from here on is a Playwright-captured screenshot, reviewed
for layout correctness, for any layout-sensitive change; `getBoundingClientRect()`
measurement remains a useful *additional* precision check (it caught the
exact pixel widths here) but is no longer the fallback of last resort now
that a real screenshot mechanism is confirmed working.

## Summary

Overview (and, discovered as a side effect, Plant Fleet) had a real,
shell-level layout defect that the original evidence pass did not catch
because its own verification method couldn't have caught it. Root-caused
to unplaced CSS Grid auto-placement in `AppComponent`, fixed with explicit
`grid-column`/`grid-row` placement, and re-verified by measuring actual
rendered geometry rather than re-reading DOM text. The gap in method is
recorded here, not just the gap in code.
