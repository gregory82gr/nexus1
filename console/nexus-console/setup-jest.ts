// zone.js here is a devDependency of the Jest test harness only (jest-preset-angular's
// TestBed integration) — it is not part of the served application, which
// stays zoneless per app.config.ts's provideExperimentalZonelessChangeDetection().
import 'jest-preset-angular/setup-jest';

// jsdom (the DOM jest-preset-angular runs against) does not implement
// ResizeObserver at all -- needed by TwinScene (Ch. 8's Plant 3D View) to
// keep the three.js canvas sized to its host <div>. A minimal stub is a
// test-environment gap fix, not application behavior.
if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class {
    observe(): void {}
    unobserve(): void {}
    disconnect(): void {}
  };
}

// jsdom's `crypto` global has no `subtle` (Web Crypto API) implementation --
// needed by features/audit/hash-chain.ts (Ch. 30) to compute real SHA-256
// hashes. Node's own built-in webcrypto implements the same standard
// interface; wired in only for the test environment, same "jsdom gap fix,
// not application behavior" pattern as the ResizeObserver stub above. The
// served application runs in a real browser, which has crypto.subtle
// natively -- this never executes outside Jest.
if (typeof globalThis.crypto?.subtle === 'undefined') {
  // eslint-disable-next-line @typescript-eslint/no-var-requires
  const { webcrypto } = require('node:crypto');
  Object.defineProperty(globalThis, 'crypto', { value: webcrypto, configurable: true });
}
