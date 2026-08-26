import { test, expect } from '@playwright/test';
import { runCommand } from './support';

// See e2e/README.md for what this suite proves and does not prove.
//
// Real per-unit facts this test relies on, checked directly against the
// live dev database before writing these assertions (never assumed):
// unit 2 has zero Instrumentation signals and zero UnitPowerSnapshot rows
// -- so Overview and Reactor Kinetics render real, honest absence states
// for it, not a happier-looking fabricated number. Overview itself never
// renders unit.code/name (checked directly -- not present anywhere in
// overview.html; the topbar's "Unit 1 - PWR-900" is a separate, static
// placeholder, unrelated to PlantStateService). So step 2's proof that
// Overview is showing unit 2 and not unit 1 uses Overview's own real,
// differing content (0 signals / NO READING vs unit 1's real 4 signals /
// real power percent) rather than a literal unit-code string Overview
// has no field for.

test('operator session: selection propagates across screens, and an alarm acknowledgement is reflected by an independently-queried screen', async ({ page }) => {
  // Step 1: NX-Script Console -- select u2, confirm on-screen.
  await page.goto('/console');
  await runCommand(page, 'select u2');
  await expect(page.locator('[data-field="selected-unit"]')).toHaveText('u2');

  // Step 2: SPA-navigate (no reload) to Overview -- confirm it now shows
  // unit 2's real, differing content, not unit 1's.
  await page.locator('a[href="/overview"]').click();
  await expect(page).toHaveURL(/\/overview$/);
  await expect(page.locator('.ph', { hasText: 'Live Signals' }).locator('.tag')).toHaveText('0 signals');
  await expect(page.locator('.panel.stat', { hasText: 'POWER' })).toContainText('NO READING');

  // Step 3: SPA-navigate to Reactor Kinetics -- confirm it also reflects
  // unit 2. Unit 2 genuinely has no Instrumentation signal seeded -- the
  // honest, real result is a NO SOURCE message, not a fabricated number,
  // and that is exactly what is asserted here (per direction: don't force
  // a happier assertion than what the real data produces).
  await page.getByRole('button', { name: 'Reactor' }).click();
  await page.locator('a[href="/kinetics"]').click();
  await expect(page).toHaveURL(/\/kinetics$/);
  await expect(page.locator('main')).toContainText('No power-like signal');

  // Step 4: select u1 back, go to Alarms & Events, acknowledge a real
  // active alarm belonging to unit 1 (picked dynamically -- whichever is
  // first in the real list -- never a hardcoded id).
  await page.locator('a[href="/console"]').click();
  await runCommand(page, 'select u1');
  await expect(page.locator('[data-field="selected-unit"]')).toHaveText('u1');

  // Baseline captured BEFORE the acknowledge, via a real, independent HTTP
  // call to Overview's own endpoint (GetActiveAlarmsForUnitQuery) -- not
  // the Alarms & Events fleet-wide query this test is about to mutate
  // through. This is what "decreased by exactly one" is measured against.
  const before = await page.request.get('http://localhost:5103/api/v1/overview/units/1').then((r) => r.json());
  const baselineCount: number = before.activeAlarms?.length ?? 0;

  await page.locator('a[href="/alarms"]').click();
  await expect(page).toHaveURL(/\/alarms$/);

  const unit1Row = page.locator('.row', { hasText: 'unit 1 ·' }).first();
  await expect(unit1Row).toBeVisible();
  const acknowledgedMessage = (await unit1Row.locator('.ln').first().innerText()).split('\n')[0].trim();

  await unit1Row.getByRole('button', { name: 'ACKNOWLEDGE' }).click();
  await expect(page.locator('main')).not.toContainText(acknowledgedMessage, { timeout: 10_000 });

  // Step 5: navigate to Overview -- assert its independently-queried
  // alarmCount for unit 1 decreased by exactly one relative to the real
  // baseline captured above. Overview's own GetActiveAlarmsForUnitQuery
  // call is a completely separate HTTP round-trip from Alarms & Events'
  // GetActiveAlarmsQuery -- this is the real cross-query consistency
  // proof, not a UI-only refresh of the same data the write just touched.
  await page.locator('a[href="/overview"]').click();
  await expect(page).toHaveURL(/\/overview$/);
  await expect(page.locator('.ph', { hasText: 'Recent Alarms' }).locator('.tag')).toHaveText(`${baselineCount - 1} active`);
});
