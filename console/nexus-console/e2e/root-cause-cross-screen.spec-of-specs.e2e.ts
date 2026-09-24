import { test, expect } from '@playwright/test';

// Spec-of-specs (Ch. 33): proves root-cause-cross-screen.e2e.ts's agreement
// assertion is load-bearing, not decorative -- the same standard the book
// applies to its own regression test, and the same one operator-session
// .spec-of-specs.e2e.ts already applies to selection propagation.
//
// The actual proof is NOT code in this file: it is the documented two-run
// process recorded in this slice's evidence report --
//   1. Temporarily break the "exactly one place the answer is produced"
//      property in features/incident/incident-summary.ts: replace
//        readonly topCause = this.rootCause.topCause;
//      with a hardcoded wrong cause that ignores the shared RootCauseService,
//        readonly topCause = signal({ tag: 'RCP-1B', role: 'parallel', weight: 0.07, coverage: 0 });
//      (RCP-1B is Ch.29's actual original wrong answer -- the parallel strand,
//      not the origin -- exactly the kind of silent screen-level disagreement
//      this test exists to catch), and import `signal` from '@angular/core'.
//   2. Run `npm run e2e -- root-cause-cross-screen.spec-of-specs.e2e.ts` against
//      the same live stack and confirm it FAILS with a message that names the
//      real mismatch (Incident Analysis showing RCP-1B while the graph verdict
//      is FV-104) -- not a generic timeout.
//   3. Revert the edit, rerun, confirm it passes again.
//
// @slow for the same reason as the main test: it drives the real pipeline.

const SEEDED_ORIGIN = 'FV-104';

test('root cause agreement is load-bearing @slow: Incident Analysis top cause must equal the live graph verdict', async ({ page }) => {
  test.setTimeout(180_000);

  await page.goto('/rcgraph');
  const verdict = page.locator('[data-field=verdict]');
  await expect(verdict).toBeVisible({ timeout: 170_000 });
  const graphVerdict = (await verdict.innerText()).trim();
  expect(graphVerdict).toBe(SEEDED_ORIGIN);

  await page.locator('a[href="/incident"]').click();
  await expect(page).toHaveURL(/\/incident$/);

  const incidentTopCause = page.locator('[data-field=top-cause-name]');
  await expect(incidentTopCause).toBeVisible({ timeout: 10_000 });
  await expect(incidentTopCause).toHaveText(graphVerdict);
});
