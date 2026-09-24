# Evidence — full live E2E cross-screen root-cause test (Ch. 33 un-deferral)

**Date:** 2026-09-24
**ADR:** none — test infrastructure, documented in `console/nexus-console/e2e/README.md`
**Scope:** Un-defer the Ch.29-deferred full live E2E. A new `@slow` Playwright test
asserts that Root Cause Graph (`/rcgraph`) and Incident Analysis (`/incident`)
name the **same** origin cause when both read the **real** backend end to end
(console → BFF → RootCause.Host → served model / Ollama → RootCauseDb), plus a
spec-of-specs proving that assertion is load-bearing. No production code change.

## Built

| Piece | Location |
|---|---|
| `root-cause-cross-screen.e2e.ts` (`@slow`, `test.setTimeout(180_000)`) | console/nexus-console/e2e |
| `root-cause-cross-screen.spec-of-specs.e2e.ts` (`@slow`) | console/nexus-console/e2e |
| README: 3rd cross-screen property + `@slow` run split + fuller stack prereqs | console/nexus-console/e2e/README.md |

No `package.json` change: the selective-run mechanism is the documented
`--grep @slow` / `--grep-invert @slow` flags. No ADR (test infra). No new
incident, no backend change.

## What it asserts beyond the Ch.29 Jest component test

The Ch.29 component test (`incident-summary.spec.ts` / `root-cause.spec.ts`)
proves the shared-service *logic* with `RootCauseDiagnosisApi` **mocked** and no
backend. This E2E proves the **real network chain**: Angular real `HttpClient` →
BFF real RootCause proxy hop (ADR-035/036) → RootCause.Host real CQRS query +
diagnosis pipeline → Ollama real served model → RootCauseDb real seeded topology,
rendered on both screens. It exercises the real proxy hop, serialization, the
`providedIn:'root'` `RootCauseService` singleton surviving SPA nav (one real
diagnosis run feeding both screens), and cross-screen agreement over that live
run — none of which the mocked component test touches.

## Test discovery + `@slow` grep split (no stack needed)

`npx playwright test --list`:

```
ALL (4 tests):
  operator-session.e2e.ts …
  operator-session.spec-of-specs.e2e.ts …
  root-cause-cross-screen.e2e.ts:31 › root cause agreement @slow …
  root-cause-cross-screen.spec-of-specs.e2e.ts:28 › root cause agreement is load-bearing @slow …

--grep @slow (2 tests):        the two new root-cause tests
--grep-invert @slow (2 tests): operator-session.e2e.ts + operator-session.spec-of-specs.e2e.ts
```

The fast suite and the slow suite partition cleanly.

## Stack used for the live runs (all real, local)

Ollama `127.0.0.1:11434` (nexus-dslm + nomic-embed-text present) · RabbitMQ 5672
(RootCause.Host boot dependency) · RootCause.Host 5102 · BFF 5103 (four contexts
+ always-on RootCause proxy, `Services:RootCauseHost=http://localhost:5102`) ·
ng serve 4200. RootCauseDb verified seeded before the run:
`RootCause.Corpus` = 2 rows / 2 embedded, `RootCause.Component` = 10.
Chain sanity before the UI run: `GET …/root-cause/incidents/EVT-2026-0418/graph`
through the BFF returned all 10 nodes (FV-104, SG-1, FWP-2A, PB-2A, LF-1, CT-1,
RT-1, RCP-1B, BUS-2A, FT-7).

## Evidence A — the `@slow` test PASSED over the live pipeline (cold)

`npm run e2e -- root-cause-cross-screen.e2e.ts`:

```
Running 1 test using 1 worker
  ok 1 …root cause agreement @slow: the graph verdict and Incident Analysis top cause
        name the same origin over the live pipeline (2.5m)
  1 passed (2.6m)
```

**2.5 minutes** = a real cold model load + inference, not a mock. It captured the
graph's live verdict (FV-104), confirmed it is the #1 ranked candidate on that
screen, SPA-navigated to Incident Analysis with no reload, and asserted its top
cause equals the captured verdict **and** equals the seeded origin FV-104.

## Evidence B — spec-of-specs is load-bearing (break → fail → revert → pass)

The documented two-run ritual (per Ch.33 convention). **Break:** in
`features/incident/incident-summary.ts`, replaced
`readonly topCause = this.rootCause.topCause;` with a hardcoded wrong cause
`signal({ tag: 'RCP-1B', role: 'parallel', weight: 0.07, coverage: 0 })`
(RCP-1B = Ch.29's original wrong answer, the parallel strand, not the origin),
ignoring the shared service. ng serve recompiled (only NG8107 `?.`-redundancy
warnings from the now-non-null type — no build error; app served).

Broken run — `npm run e2e -- root-cause-cross-screen.spec-of-specs.e2e.ts`:

```
  x  1 …is load-bearing @slow… (42.1s)
    Error: expect(locator).toHaveText(expected) failed
    Locator:  locator('[data-field=top-cause-name]')
    Expected: "FV-104"
    Received: "RCP-1B"
      13 × locator resolved to <strong data-field="top-cause-name">RCP-1B</strong>
  1 failed
```

It failed **naming the real mismatch** (Expected FV-104 / Received RCP-1B), not a
generic timeout — proving the assertion catches exactly the silent screen-level
disagreement it exists to catch.

**Revert:** restored `readonly topCause = this.rootCause.topCause;` and removed
the now-unused `signal` import; ng serve recompiled clean (no warnings).
`git diff` on `incident-summary.ts` is **empty** (break fully reverted).

Reverted run:

```
  ok 1 …is load-bearing @slow… (26.3s)
  1 passed (27.8s)
```

Warm (26.3s) this time — same test, same live stack, now passing.

## Evidence C — no regression to the fast paths

Fast E2E suite — `npm run e2e -- --grep-invert @slow`:

```
Running 2 tests using 1 worker
  ok 1 operator session: selection propagates across screens … (7.5s)
  ok 2 selection propagation: select u2 … (1.8s)
  2 passed (11.2s)
```

Seconds-long, `@slow` excluded, unchanged. Jest — `npx jest`:

```
Test Suites: 55 passed, 55 total
Tests:       283 passed, 283 total
Time:        53.641 s
```

Both fast paths green and unchanged; the slow test is out of both.

## Honesty notes

- The `@slow` test is **not wired into CI** — same dev-only, run-by-hand status
  as the rest of this suite (README "does NOT prove" section). Named, not implied.
- Cold-tolerance is by construction (one `toBeVisible({timeout:170_000})` wait,
  no branching, no warm-up) — proven by a cold pass (2.5m) and a warm pass (26.3s)
  of the same code, no flakiness handling needed.
- The graph verdict and #1 ranked candidate coinciding with FV-104 is a property
  of this incident's real backend response, captured dynamically from the live
  run, not hardcoded into the assertion path (only the expected origin string
  `FV-104` is named, matching the seed).
