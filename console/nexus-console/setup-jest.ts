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
