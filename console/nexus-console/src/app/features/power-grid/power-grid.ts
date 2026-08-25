import { Component, DestroyRef, Input, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InstrumentationApi, UnitSignalReading } from '../../core/api/instrumentation-api';
import { PlantStateService } from '../../core/state/plant-state';
import { GridTie, buildGridTie } from './grid-tie';

// Power & Grid (Ch. 21) -- built on the same real, generic Instrumentation
// signals endpoint as the Reactor cluster, extended by one new real
// signal category (TURBINE) seeded the same way NEUTRONICS was: a new
// SignalCategory + Signal row in the existing generic model, not new
// domain/application code.
//
// The book's own central finding here is a fabricated RELATIONSHIP, not a
// fabricated value: its source computes grid frequency from local
// turbine RPM, which is backwards -- a synchronized grid has one shared
// frequency the turbine tracks, it does not set it. The fix is
// structural: turbineSpeedRpm stays real; gridFrequencyHz, phaseAngleDeg,
// breakerClosed, and inSync become a separate, unconnected field set
// (see grid-tie.ts's own guard comment -- no function anywhere derives
// one from the other).
//
// Checked directly before this slice, per the same discipline as every
// prior cluster: active power, reactive power, generator voltage, and
// power factor have never been seeded, tested, or live-verified anywhere
// in this system either. Only reactor thermal power (%RTP, the POWER
// category), neutron flux (NEUTRONICS), pump vibration (VIBRATION, in
// Maintenance), and now turbine shaft speed (TURBINE) have ever been
// real data here. Those four generator/grid-electrical quantities are
// declared as an honest gap below, not fabricated to fill out the panel.
type SignalsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; tie: GridTie };

@Component({
  selector: 'nx-power-grid',
  standalone: true,
  templateUrl: './power-grid.html',
  styleUrl: './power-grid.scss',
})
export class PowerGridComponent {
  private readonly api = inject(InstrumentationApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() focusLabel = 'Power & Grid';

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<SignalsState>({ status: 'loading' });

  constructor() {
    this.api
      .getSignals(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (signals: UnitSignalReading[]) => this.state.set({ status: 'loaded', tie: buildGridTie(signals) }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Instrumentation signals endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get tie(): GridTie | null {
    const s = this.state();
    return s.status === 'loaded' ? s.tie : null;
  }
}
