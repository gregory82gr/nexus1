# Evidence: sidebar nav — active/focus state fixes

## Scope

Standalone shell polish, not tied to any screen: `shared/sidebar/`. Fixes
the three gaps reported before this work started (collapsed-group
indication, weak active contrast, no keyboard focus styling).

## 1. Collapsed-group indication + auto-expand

`sidebar.ts` gained a pure, exported `findActiveGroupLabel(url, nav)`
that walks the nav tree and returns the label of whichever group contains
a child matching the current URL (leading-segment match, ignoring query
strings/fragments — every real route here is one flat path segment). Two
things read it:

- **Seeding `openGroups`** at construction, from `router.url` at the
  moment the component is created — so a direct navigation or a page
  refresh onto a nested route (e.g. `/core`) never starts with its own
  group collapsed.
- **`isActiveGroup(entry)`**, a method the template binds
  `[class.active]` to on `.grouphead` — reactive via a `currentUrl`
  signal updated on every `NavigationEnd`, so it stays correct across
  in-app navigation (a plain `router.url` read wouldn't re-trigger this
  app's zoneless change detection on its own).

Auto-expand: the same `NavigationEnd` subscription adds the target
group's label to `openGroups` if it isn't already open — additive only,
never closing a group the user opened for an unrelated reason (a
manually-opened "Rod Inspection" group stays open when the user
separately navigates into "Reactor").

Deliberately, the active indicator does **not** depend on the group being
open: `isActiveGroup` is computed from the URL alone, so re-collapsing an
already-active group (a user closing what auto-opened) still shows the
header lit up. That's the actual fix — the auto-expand is a convenience,
the header state is the one that must never go dark.

## 2. Active-state contrast

`--panel` (`#0c1416`) and `--panel-2` (`#0f1a1d`) are both near-black and
close enough in value that the old `.active` background shift was close
to imperceptible — the cyan text color was doing essentially all the
work. Replaced with a genuinely visible cyan-tinted wash
(`background: rgba(34, 211, 238, 0.1)` — `--cyan`'s own RGB, not an
invented color) plus a 3px left accent bar (`border-left-color:
var(--cyan)`), applied uniformly to `.navitem.active`, `.subitem.active`,
and the new `.grouphead.active`. A 3px transparent `border-left` is
reserved on every nav row (not just active ones) so the accent bar never
shifts text position when it appears.

Also added a plain hover state (`background: var(--panel-2)`, no accent
bar) that didn't exist before — needed so "focus-visible distinct from
hover" (task 3) is a real distinction rather than a comparison against
nothing.

## 3. Keyboard focus-visible

`.navitem:focus-visible, .grouphead:focus-visible, .subitem:focus-visible`
get a `2px solid var(--violet)` outline — a different token color from
both the cyan active/hover treatment, so all three states (hover,
keyboard focus, route-active) are visually distinct from one another.

This required a real accessibility fix underneath the styling, not just
a CSS rule with nothing to attach to: `.grouphead` was a plain `<div>`
with a `(click)` handler, not reachable by keyboard at all. Added
`role="button"`, `tabindex="0"`, `[attr.aria-expanded]="isOpen(entry)"`,
and `(keydown.enter)` / `(keydown.space)` handlers (the latter calling
`preventDefault()` so Space doesn't also scroll the page) — so the
group headers are now genuinely keyboard-operable, which is the
precondition for "keyboard focus styling" meaning anything.

## Tests

New `sidebar.spec.ts`, 9 specs:

- `findActiveGroupLabel` (pure): finds the right group for a flat child
  path, returns `null` for a top-level route and for no match, ignores
  query strings/fragments.
- A direct navigation onto `/core` starts with "Reactor" already open
  (the refresh/deep-link case).
- The group is marked active even after being manually re-collapsed —
  the exact gap this fix addresses, asserted directly rather than only
  implied by the screenshot.
- A later in-app navigation auto-expands the newly-active group without
  collapsing a different group the user had opened themselves.
- A flat, non-grouped route reports no active group at all.
- The group header is keyboard-reachable (`tabindex="0"`, `role="button"`,
  correct `aria-expanded`).

```
npx jest   → 115/115 passing (was 106; 9 new specs)
```

Production build:

```
npx ng build → 0 errors, 0 warnings.
```

## Live evidence

No BFF dependency (pure shell/nav change) — `ng serve` alone, then
Playwright screenshots per the standing rule.

- `sidebar-collapsed-group-active.png` — navigated to `/core`, then
  clicked the "Reactor" header to manually re-collapse it (simulating a
  user closing what had auto-opened). The header still shows the cyan
  wash, accent bar, and bold weight, with the chevron un-rotated and the
  child links hidden — this is the exact case that showed nothing at all
  before the fix.
- `sidebar-keyboard-focus.png` — real `Tab` key presses (not a
  programmatic `.focus()` call) moved focus to "Plant 3D View"; the
  screenshot shows its violet focus-visible outline alongside "Overview"'s
  separate cyan active treatment on the actual current route — visibly
  two different states, not one being mistaken for the other.

Both reviewed directly: full sidebar renders correctly, no layout shift
from the reserved border-left, no regression to the rest of the shell.

## Summary

All three gaps fixed as one coherent change to `shared/sidebar/`: a pure,
tested URL-to-group lookup drives both the collapsed-group indicator and
non-destructive auto-expand; the active treatment now reads as a real
state change (cyan wash + accent bar) instead of relying on text color
alone; and keyboard operability was added to the group header (not just
CSS) so the new focus-visible styling has something real to attach to.
