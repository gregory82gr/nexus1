import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactorFleetApi, UnitSummary } from '../../core/api/reactor-fleet-api';

// Plant Fleet (Ch. 7), shaped honestly around what Nexus1.Bff's
// GET /api/v1/reactor-fleet/units actually returns -- UnitSummaryDto:
// id, code, name, a nullable power PERCENTAGE, and its recorded timestamp.
// Nothing else exists on the real Phase 1 Unit aggregate (ADR-003).
//
// Named gaps against the book's own Figure 7.1, reported rather than
// papered over:
//   - Plant Output / Installed Capacity (MWe): the domain has no MWe
//     rating or installed-capacity field at all -- only a percent-of-rated
//     reading. There is nothing to sum into an MWe total, so neither card
//     is built. "Units reporting" below is the one honest aggregate this
//     data actually supports.
//   - Units Online: there is no online/offline flag anywhere in the
//     domain. The book's three-state pill (Offline / Starting / Online,
//     threshold 25%) requires an "is this unit powered on" signal this
//     API does not have -- inventing one would misrepresent what the
//     backend actually knows. Replaced with a plain two-state pill: a
//     unit either has a recorded reading or it doesn't.
//   - Grid Frequency: the book's own chapter already names this NO SOURCE
//     until Ch. 19 connects real bus telemetry -- not attempted here either.
//   - The 400 kV bus diagram: no topology data exists in this API at all;
//     not built.
//   - On/Off toggle: no command endpoint exists (the book's own Ch. 20
//     boundary) -- not rendered, not even disabled, since there's no
//     on/off concept in the domain to represent.
//   - Cross-screen selection (the topbar driving every Reactor screen,
//     Ch. 7's own switchMap/staleness architecture): deferred. There is no
//     second screen yet for a selection to drive, and no per-unit
//     telemetry-polling endpoint proven in the BFF -- building the
//     selection-to-request join now would be infrastructure with no
//     present consumer. Select below is local, visual-only state (the
//     cyan-border affordance), not wired to any cross-screen service yet.
type FleetState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; units: UnitSummary[] };

const NO_READING_THRESHOLD_PERCENT = 25; // named constant, not a magic number (Deep Dive 6's own rule)

@Component({
  selector: 'nx-fleet',
  standalone: true,
  templateUrl: './fleet.html',
  styleUrl: './fleet.scss',
})
export class FleetComponent {
  private readonly api = inject(ReactorFleetApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly state = signal<FleetState>({ status: 'loading' });
  readonly selectedId = signal<number | null>(null);

  readonly reportingCount = computed(() => {
    const s = this.state();
    return s.status === 'loaded' ? s.units.filter((u) => u.latestPowerPercent !== null).length : 0;
  });
  readonly totalCount = computed(() => {
    const s = this.state();
    return s.status === 'loaded' ? s.units.length : 0;
  });

  constructor() {
    this.api
      .getUnits()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (units) => this.state.set({ status: 'loaded', units }),
        error: (err) =>
          this.state.set({
            status: 'error',
            message: err?.status ? `Request failed (HTTP ${err.status}).` : 'The fleet endpoint is unreachable.',
          }),
      });
  }

  // Narrowing helpers -- Angular's template compiler doesn't narrow a
  // discriminated union across repeated state() calls inside @switch/@case
  // the way plain TypeScript does, so the narrowing happens here instead.
  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedUnits(): UnitSummary[] {
    const s = this.state();
    return s.status === 'loaded' ? s.units : [];
  }

  select(unit: UnitSummary): void {
    this.selectedId.set(unit.id);
  }

  isLowReading(unit: UnitSummary): boolean {
    return unit.latestPowerPercent !== null && unit.latestPowerPercent < NO_READING_THRESHOLD_PERCENT;
  }
}
