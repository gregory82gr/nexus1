import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RadiationSafetyApi, UnitRadiationMonitorReading, UnitRadiationSafety } from '../../core/api/radiation-safety-api';
import { PlantStateService } from '../../core/state/plant-state';

// Radiation & Safety (Ch. 22) -- the safety banner below is the book's own
// wording, unchanged: it is correct as-is and this project's own
// no-control-authority discipline (see reactor-instrumentation.ts's own
// doc comment on the same point for Control Rods) already agrees with it.
//
// The book's own finding here is narrower: its "Area Radiation Monitors"
// table shows 5 rows that look like 5 independent instruments, but its
// own source code computes 4 of them as linear scalings of only 2
// upstream signals (containment dose and reactor power) -- admitted in
// the source's own inline comment, but invisible on the rendered screen.
//
// Checked directly before building, same discipline as the Power & Grid
// investigation (real seeded/tested/live data, not just domain shape):
// this system's RadiationMonitoring context already has a genuinely
// separate, real per-instrument model -- RadiationMonitor (the
// instrument/siting record) and RadiationReading (an append-only value
// per monitor, keyed by RadiationMonitorId). The finder query
// (EfLatestReadingPerMonitorFinder) does nothing but pick each monitor's
// own most recent reading -- no formula, no cross-monitor arithmetic, no
// derivation from any other signal anywhere in this codebase. This is
// actually simpler and more honest than the book's own shortcut: not
// "one value scaled 5 ways," but N genuinely independent monitors, each
// with its own independently-entered value. Five real RadiationMonitor +
// RadiationReading rows were seeded for this slice (Containment Interior,
// Aux Building, Fuel Handling, Turbine Hall, Stack Effluent) -- real,
// separate rows, not a formula from 2 upstream numbers.
//
// Zones are deliberately NOT repeated on this screen: the real per-unit
// zone list this endpoint also returns is the same RadiationZone data
// the Zone Access screen (Ch. 20, features/zone-registry/) already shows
// fleet-wide -- showing it again here would be a duplicate view over the
// same real rows, not a second real capability.
type SafetyState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; monitors: UnitRadiationMonitorReading[] };

@Component({
  selector: 'nx-radiation-safety',
  standalone: true,
  templateUrl: './radiation-safety.html',
  styleUrl: './radiation-safety.scss',
})
export class RadiationSafetyComponent {
  private readonly api = inject(RadiationSafetyApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<SafetyState>({ status: 'loading' });

  constructor() {
    this.api
      .getUnitRadiationSafety(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (safety: UnitRadiationSafety) => this.state.set({ status: 'loaded', monitors: safety.monitors }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The RadiationMonitoring unit-safety endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedMonitors(): UnitRadiationMonitorReading[] {
    const s = this.state();
    return s.status === 'loaded' ? s.monitors : [];
  }
}
