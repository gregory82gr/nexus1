import { test, expect } from '@playwright/test';

// See e2e/README.md for what this suite proves and does not prove.
//
// The third real cross-screen property (Ch. 33), and the first that exercises
// the full live RootCause diagnosis pipeline end to end -- console -> BFF ->
// RootCause.Host -> served model (Ollama) -> RootCauseDb -- not a mock and not a
// shared service tested in isolation (that is the Ch.29 Jest component test,
// which stubs RootCauseDiagnosisApi). Here both screens read the REAL backend.
//
// Root Cause Graph (/rcgraph) and Incident Analysis (/incident) both inject the
// one root-provided RootCauseService, which fetches the diagnosis once and
// caches it. Navigating between them with no reload keeps that singleton alive,
// so a single real diagnosis run feeds both screens -- they are not entitled to
// a different answer. This test proves that agreement over the live chain: the
// origin the graph screen blames (its [data-field=verdict]) is the same cause
// Incident Analysis names as its top cause ([data-field=top-cause-name]), and
// both are the seeded origin FV-104.
//
// @slow: this drives the real pipeline (cold ~100-130s on first model load,
// warm ~20-25s), far past the suite's 30s default. It is tagged so the fast
// suite can exclude it (`--grep-invert @slow`) and this one run selectively
// (`--grep @slow`). Per-test timeout is raised locally; the 30s default is
// untouched. Stack prerequisites are in e2e/README.md.

// The seeded origin cause for EVT-2026-0418 (From Flood to Cause, Figure 2.1):
// FV-104 is the engineered origin, distinct from the loudest symptom. It is the
// #1 ranked candidate and the graph's computed verdict.
const SEEDED_ORIGIN = 'FV-104';

test('root cause agreement @slow: the graph verdict and Incident Analysis top cause name the same origin over the live pipeline', async ({ page }) => {
  // Cold-tolerant: one wait, no branching, no warm-up. 170s covers a cold model
  // load (under the Host's 150s inference budget + margin) or a warm run; it
  // resolves the instant the verdict renders, so warm runs are not slowed and
  // cold runs are not flaky.
  test.setTimeout(180_000);

  // Root Cause Graph -- run the real diagnosis and read the verdict it lands.
  await page.goto('/rcgraph');
  const verdict = page.locator('[data-field=verdict]');
  await expect(verdict).toBeVisible({ timeout: 170_000 });
  const graphVerdict = (await verdict.innerText()).trim();

  // The graph's origin is the seeded FV-104, and it is the #1 ranked candidate
  // (origin == top of the ranking for this incident) -- captured dynamically
  // from the same live response, never hardcoded into the assertion path.
  expect(graphVerdict).toBe(SEEDED_ORIGIN);
  const topRankedOnGraph = (await page.locator('[data-field=candidate-tag]').first().innerText()).trim();
  expect(topRankedOnGraph).toBe(graphVerdict);

  // SPA-navigate to Incident Analysis with no reload -- the RootCauseService
  // singleton (and its one cached diagnosis run) survives, so this screen does
  // not re-run the pipeline; it reads the same answer.
  await page.locator('a[href="/incident"]').click();
  await expect(page).toHaveURL(/\/incident$/);

  // The cross-screen property: Incident Analysis's top cause equals the graph's
  // verdict from the same live run (dynamic), and equals the seeded origin.
  const incidentTopCause = page.locator('[data-field=top-cause-name]');
  await expect(incidentTopCause).toBeVisible({ timeout: 10_000 });
  await expect(incidentTopCause).toHaveText(graphVerdict);
  await expect(incidentTopCause).toHaveText(SEEDED_ORIGIN);
});
