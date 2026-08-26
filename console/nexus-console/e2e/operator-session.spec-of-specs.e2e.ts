import { test, expect } from '@playwright/test';
import { runCommand } from './support';

// Spec-of-specs (Ch. 33): proves operator-session.e2e.ts's selection-
// propagation assertions are load-bearing, not decorative, the same
// standard the book applies to its own regression test. Isolated to just
// the selection-propagation steps (no alarm acknowledgement) so proving
// this never consumes real alarm data -- see e2e/README.md.
//
// The actual proof is NOT code in this file: it is the documented
// two-run process recorded in the Ch. 33 evidence report --
//   1. Temporarily edit features/overview/overview.ts's
//      `readonly unitId = this.plantState.selectedId;` to
//      `readonly unitId = signal(1);` (a hardcoded unit, ignoring the
//      real shared selection -- exactly the kind of silent regression
//      this test exists to catch).
//   2. Run `npm run e2e -- operator-session.spec-of-specs.e2e.ts` against
//      the same live session and confirm it fails, with a message that
//      names the actual mismatch (expected "0 signals", still seeing
//      unit 1's real signal count) -- not a generic timeout.
//   3. Revert the edit, rerun, confirm it passes again.
test('selection propagation: select u2 on NX-Script Console changes what Overview and Reactor Kinetics render', async ({ page }) => {
  await page.goto('/console');
  await runCommand(page, 'select u2');
  await expect(page.locator('[data-field="selected-unit"]')).toHaveText('u2');

  await page.locator('a[href="/overview"]').click();
  await expect(page).toHaveURL(/\/overview$/);
  await expect(page.locator('.ph', { hasText: 'Live Signals' }).locator('.tag')).toHaveText('0 signals');
  await expect(page.locator('.panel.stat', { hasText: 'POWER' })).toContainText('NO READING');

  await page.getByRole('button', { name: 'Reactor' }).click();
  await page.locator('a[href="/kinetics"]').click();
  await expect(page).toHaveURL(/\/kinetics$/);
  await expect(page.locator('main')).toContainText('No power-like signal');
});
