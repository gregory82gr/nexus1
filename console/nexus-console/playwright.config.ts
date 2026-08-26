import { defineConfig } from '@playwright/test';

// Ch. 33's own new, real E2E infrastructure -- deliberately separate from
// jest.config.js (unit/component tests, mocked HTTP via
// HttpTestingController) and from the throwaway `capture-*.mjs` scripts
// every prior chapter used for one-off screenshot review (never committed,
// always deleted after use). This is the first COMMITTED Playwright
// project in the console.
//
// No `webServer` entry here, on purpose: every real dual-process live
// evidence session this whole project has run (Ch. 6 onward) starts
// Nexus1.Bff with a specific, per-scenario BffContexts__Enabled__N subset
// composition -- something Playwright's single-command webServer helper
// isn't a good fit for alongside a second, independently-configured
// process. Both `ng serve` and `Nexus1.Bff` (composed with ReactorFleet +
// Instrumentation + AlarmManagement + RadiationMonitoring, all four --
// Overview's endpoint resolves every handler via DI up front and 500s
// whole-endpoint if any one is missing) must already be running before
// `npm run e2e`. See e2e/README.md.
export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.e2e.ts',
  timeout: 30_000,
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'retain-on-failure',
  },
});
